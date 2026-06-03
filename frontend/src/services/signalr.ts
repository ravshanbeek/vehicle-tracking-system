import * as signalR from '@microsoft/signalr';
import { API_CONFIG } from '../config/api';
import type { LocationDto, SignalRStatus } from '../types';

type StatusHandler = (status: SignalRStatus) => void;
type LocationHandler = (location: LocationDto) => void;

class SignalRService {
  private readonly connection: signalR.HubConnection;
  private readonly joinedGroups = new Set<number>();
  private statusHandlers = new Set<StatusHandler>();
  private startPromise: Promise<void> | null = null;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(API_CONFIG.signalrHub)
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.onreconnecting(() => {
      this.notify('reconnecting');
    });

    this.connection.onreconnected(async () => {
      // Re-subscribe to all previously joined groups after reconnect
      const toRejoin = [...this.joinedGroups];
      this.joinedGroups.clear();
      await Promise.all(toRejoin.map((id) => this.joinVehicleGroup(id)));
      this.notify('connected');
    });

    this.connection.onclose(() => {
      this.notify('disconnected');
    });
  }

  // ─── Lifecycle ──────────────────────────────────────────────────────────────

  async start(): Promise<void> {
    if (this.connection.state === signalR.HubConnectionState.Connected) return;
    // If a connection attempt is already in progress, wait for it instead of starting a new one.
    // Without this, parallel joinVehicleGroup calls each see state=Connecting, start() returns
    // immediately, and invoke() fires on an unconnected hub → silent failure.
    if (this.startPromise) return this.startPromise;
    this.notify('connecting');
    this.startPromise = this.connection
      .start()
      .then(() => this.notify('connected'))
      .catch((err) => {
        this.notify('disconnected');
        console.error('[SignalR] Failed to connect:', err);
        throw err;
      })
      .finally(() => {
        this.startPromise = null;
      });
    return this.startPromise;
  }

  async stop(): Promise<void> {
    this.startPromise = null;
    await this.connection.stop();
  }

  // ─── Groups ─────────────────────────────────────────────────────────────────

  async joinVehicleGroup(vehicleId: number): Promise<void> {
    if (this.joinedGroups.has(vehicleId)) return;
    if (this.connection.state !== signalR.HubConnectionState.Connected) {
      await this.start();
    }
    await this.connection.invoke('JoinVehicleGroup', vehicleId);
    this.joinedGroups.add(vehicleId);
  }

  async leaveVehicleGroup(vehicleId: number): Promise<void> {
    if (!this.joinedGroups.has(vehicleId)) return;
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveVehicleGroup', vehicleId);
    }
    this.joinedGroups.delete(vehicleId);
  }

  // ─── Event subscriptions ────────────────────────────────────────────────────

  onLocationUpdated(handler: LocationHandler): () => void {
    this.connection.on('LocationUpdated', handler);
    return () => this.connection.off('LocationUpdated', handler);
  }

  onStatusChange(handler: StatusHandler): () => void {
    this.statusHandlers.add(handler);
    return () => this.statusHandlers.delete(handler);
  }

  get currentStatus(): SignalRStatus {
    switch (this.connection.state) {
      case signalR.HubConnectionState.Connected:
        return 'connected';
      case signalR.HubConnectionState.Connecting:
      case signalR.HubConnectionState.Reconnecting:
        return 'reconnecting';
      default:
        return 'disconnected';
    }
  }

  // ─── Private ────────────────────────────────────────────────────────────────

  private notify(status: SignalRStatus): void {
    this.statusHandlers.forEach((h) => h(status));
  }
}

// Singleton — one connection for the whole app
export const signalRService = new SignalRService();

import { create } from 'zustand';
import { subscribeWithSelector } from 'zustand/middleware';
import type { Vehicle, LocationDto, LocationHistoryDto, SignalRStatus } from '../types';

interface TrackingState {
  // ─── Data ──────────────────────────────────────────────────────────────────
  vehicles: Vehicle[];
  currentLocations: Record<number, LocationDto>;
  selectedVehicleId: number | null;
  historyPath: LocationHistoryDto[];

  // ─── UI state ──────────────────────────────────────────────────────────────
  isLoadingVehicles: boolean;
  isLoadingHistory: boolean;
  vehiclesError: string | null;
  historyError: string | null;
  signalRStatus: SignalRStatus;

  // ─── Actions ───────────────────────────────────────────────────────────────
  setVehicles: (vehicles: Vehicle[]) => void;
  updateLocation: (location: LocationDto) => void;
  setCurrentLocation: (location: LocationDto) => void;
  setSelectedVehicleId: (id: number | null) => void;
  setHistoryPath: (path: LocationHistoryDto[]) => void;
  setLoadingVehicles: (loading: boolean) => void;
  setLoadingHistory: (loading: boolean) => void;
  setVehiclesError: (error: string | null) => void;
  setHistoryError: (error: string | null) => void;
  setSignalRStatus: (status: SignalRStatus) => void;
  clearHistory: () => void;
}

export const useTrackingStore = create<TrackingState>()(
  subscribeWithSelector((set) => ({
    // ─── Initial state ────────────────────────────────────────────────────────
    vehicles: [],
    currentLocations: {},
    selectedVehicleId: null,
    historyPath: [],

    isLoadingVehicles: false,
    isLoadingHistory: false,
    vehiclesError: null,
    historyError: null,
    signalRStatus: 'disconnected',

    // ─── Actions ──────────────────────────────────────────────────────────────
    setVehicles: (vehicles) => set({ vehicles }),

    updateLocation: (location) =>
      set((state) => ({
        currentLocations: {
          ...state.currentLocations,
          [location.vehicleId]: location,
        },
      })),

    setCurrentLocation: (location) =>
      set((state) => ({
        currentLocations: {
          ...state.currentLocations,
          [location.vehicleId]: location,
        },
      })),

    setSelectedVehicleId: (id) => set({ selectedVehicleId: id, historyPath: [] }),

    setHistoryPath: (path) => set({ historyPath: path }),

    setLoadingVehicles: (loading) => set({ isLoadingVehicles: loading }),

    setLoadingHistory: (loading) => set({ isLoadingHistory: loading }),

    setVehiclesError: (error) => set({ vehiclesError: error }),

    setHistoryError: (error) => set({ historyError: error }),

    setSignalRStatus: (status) => set({ signalRStatus: status }),

    clearHistory: () => set({ historyPath: [] }),
  })),
);

import { useEffect } from 'react';
import { signalRService } from '../services/signalr';
import { useTrackingStore } from '../store/useTrackingStore';

/**
 * Manages the SignalR connection lifecycle.
 * - Connects on mount, disconnects on unmount.
 * - Forwards LocationUpdated events to the store.
 * - Syncs connection status to the store.
 */
export function useSignalR(): void {
  const updateLocation = useTrackingStore((s) => s.updateLocation);
  const setSignalRStatus = useTrackingStore((s) => s.setSignalRStatus);

  useEffect(() => {
    // Status changes
    const unsubStatus = signalRService.onStatusChange(setSignalRStatus);

    // Location updates — updates store, which MapView subscribes to via ref
    const unsubLocation = signalRService.onLocationUpdated(updateLocation);

    // Connect
    signalRService.start().catch((err) => {
      console.error('[SignalR] Initial connection failed:', err);
    });

    return () => {
      unsubStatus();
      unsubLocation();
      signalRService.stop().catch(() => { /* ignore on unmount */ });
    };
  }, [updateLocation, setSignalRStatus]);
}

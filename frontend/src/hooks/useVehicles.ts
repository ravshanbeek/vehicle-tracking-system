import { useEffect } from 'react';
import { fetchVehicles, fetchCurrentLocation } from '../services/api';
import { signalRService } from '../services/signalr';
import { useTrackingStore } from '../store/useTrackingStore';

/**
 * Fetches vehicles on mount, loads their current locations, then joins
 * a SignalR group for each vehicle so real-time updates start flowing.
 */
export function useVehicles(): void {
  const setVehicles = useTrackingStore((s) => s.setVehicles);
  const setCurrentLocation = useTrackingStore((s) => s.setCurrentLocation);
  const setLoadingVehicles = useTrackingStore((s) => s.setLoadingVehicles);
  const setVehiclesError = useTrackingStore((s) => s.setVehiclesError);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoadingVehicles(true);
      setVehiclesError(null);
      try {
        const vehicles = await fetchVehicles();
        if (cancelled) return;
        setVehicles(vehicles);

        // Load current locations + join SignalR groups in parallel
        await Promise.all(
          vehicles.map(async (v) => {
            // Join SignalR group so updates arrive going forward
            try {
              await signalRService.joinVehicleGroup(v.id);
            } catch (err) {
              console.warn(`[SignalR] Could not join group for vehicle ${v.id}:`, err);
            }

            // Fetch the last known position for the initial map render
            try {
              const loc = await fetchCurrentLocation(v.id);
              if (!cancelled && loc) setCurrentLocation(loc);
            } catch (err) {
              console.warn(`[API] Could not fetch location for vehicle ${v.id}:`, err);
            }
          }),
        );
      } catch (err) {
        if (!cancelled) {
          const msg = err instanceof Error ? err.message : 'Unknown error';
          setVehiclesError(msg);
        }
      } finally {
        if (!cancelled) setLoadingVehicles(false);
      }
    }

    load();
    return () => { cancelled = true; };
  }, [setVehicles, setCurrentLocation, setLoadingVehicles, setVehiclesError]);
}

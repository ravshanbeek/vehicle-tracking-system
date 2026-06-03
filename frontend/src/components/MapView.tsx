import { useEffect, useRef, useState } from 'react';
import { useYandexMaps } from '../hooks/useYandexMaps';
import { useTrackingStore } from '../store/useTrackingStore';
import type { Vehicle, LocationDto, LocationHistoryDto } from '../types';

// Default map center: Tashkent (matches simulator GPS routes)
const DEFAULT_CENTER: [number, number] = [41.3111, 69.2797];
const DEFAULT_ZOOM = 12;

const VEHICLE_COLORS = ['blue', 'red', 'green', 'orange', 'violet', 'darkBlue', 'pink', 'darkOrange'] as const;
function vehiclePreset(vehicleId: number): string {
  const color = VEHICLE_COLORS[vehicleId % VEHICLE_COLORS.length];
  return `islands#${color}AutoIcon`;
}

function buildBalloonContent(vehicle: Vehicle, loc: LocationDto): string {
  const time = new Date(loc.recordedAt).toLocaleTimeString();
  return `
    <div style="min-width:160px;font-family:sans-serif;font-size:13px">
      <strong>${vehicle.name}</strong><br/>
      <span style="color:#666">${vehicle.plateNumber}</span><br/>
      <hr style="margin:4px 0"/>
      Lat: ${loc.latitude.toFixed(5)}<br/>
      Lon: ${loc.longitude.toFixed(5)}<br/>
      Speed: ${loc.speed.toFixed(1)} km/h<br/>
      <span style="color:#999">${time}</span>
    </div>
  `;
}

export function MapView() {
  const containerRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<ymaps.Map | null>(null);
  const markersRef = useRef<Map<number, ymaps.Placemark>>(new Map());
  const polylineRef = useRef<ymaps.Polyline | null>(null);

  const ymapsStatus = useYandexMaps();
  const [mapReady, setMapReady] = useState(false);

  const vehicles = useTrackingStore((s) => s.vehicles);
  const selectedVehicleId = useTrackingStore((s) => s.selectedVehicleId);
  const historyPath = useTrackingStore((s) => s.historyPath);
  const setSelectedVehicleId = useTrackingStore((s) => s.setSelectedVehicleId);

  // ─── 1. Initialize map once Yandex Maps is ready ─────────────────────────
  useEffect(() => {
    if (ymapsStatus !== 'ready' || !containerRef.current || mapRef.current) return;

    window.ymaps.ready(() => {
      if (!containerRef.current) return;
      mapRef.current = new window.ymaps.Map(
        containerRef.current,
        { center: DEFAULT_CENTER, zoom: DEFAULT_ZOOM },
        { suppressMapOpenBlock: true },
      );
      setMapReady(true);
    });

    return () => {
      mapRef.current?.destroy();
      mapRef.current = null;
      markersRef.current.clear();
      polylineRef.current = null;
    };
  }, [ymapsStatus]);

  // ─── 2. Sync markers when vehicles list changes ───────────────────────────
  useEffect(() => {
    if (!mapReady || !mapRef.current) return;

    const { currentLocations } = useTrackingStore.getState();

    vehicles.forEach((vehicle) => {
      const loc = currentLocations[vehicle.id];
      if (!loc) return; // no known position yet — marker will be added when location arrives

      if (!markersRef.current.has(vehicle.id)) {
        addMarker(vehicle, loc, setSelectedVehicleId);
      }
    });

    // Remove markers for vehicles that no longer exist
    markersRef.current.forEach((marker, id) => {
      if (!vehicles.find((v) => v.id === id)) {
        mapRef.current?.geoObjects.remove(marker);
        markersRef.current.delete(id);
      }
    });
  }, [mapReady, vehicles, setSelectedVehicleId]);

  // ─── 3. Subscribe to real-time location updates (no re-render) ───────────
  useEffect(() => {
    if (!mapReady) return;

    return useTrackingStore.subscribe(
      (state) => state.currentLocations,
      (currentLocations) => {
        if (!mapRef.current) return;
        const { vehicles: currentVehicles } = useTrackingStore.getState();

        Object.values(currentLocations).forEach((loc) => {
          const vehicle = currentVehicles.find((v) => v.id === loc.vehicleId);
          if (!vehicle) return;

          const existing = markersRef.current.get(loc.vehicleId);
          if (existing) {
            // Update in place — no DOM re-render needed
            existing.geometry.setCoordinates([loc.latitude, loc.longitude]);
            existing.properties.set('balloonContent', buildBalloonContent(vehicle, loc));
          } else {
            // First location for this vehicle — add marker now
            addMarker(vehicle, loc, setSelectedVehicleId);
          }
        });
      },
    );
  }, [mapReady, setSelectedVehicleId]);

  // ─── 4. Pan to selected vehicle ──────────────────────────────────────────
  useEffect(() => {
    if (!mapReady || selectedVehicleId === null || !mapRef.current) return;
    const { currentLocations } = useTrackingStore.getState();
    const loc = currentLocations[selectedVehicleId];
    if (loc) {
      mapRef.current.setCenter([loc.latitude, loc.longitude], 14, {
        duration: 500,
        timingFunction: 'ease-in-out',
      });
    }
  }, [mapReady, selectedVehicleId]);

  // ─── 5. Draw / clear history polyline ────────────────────────────────────
  useEffect(() => {
    if (!mapReady || !mapRef.current) return;

    // Remove old polyline
    if (polylineRef.current) {
      mapRef.current.geoObjects.remove(polylineRef.current);
      polylineRef.current = null;
    }

    if (historyPath.length < 2) return;

    const coords = historyPath.map(
      (p): [number, number] => [p.latitude, p.longitude],
    );

    polylineRef.current = new window.ymaps.Polyline(
      coords,
      { hintContent: 'Route history' },
      { strokeColor: '#3B82F6', strokeWidth: 4, strokeOpacity: 0.8 },
    );
    mapRef.current.geoObjects.add(polylineRef.current);

    // Fit bounds to show the full path
    fitPolylineBounds(historyPath);
  }, [mapReady, historyPath]);

  // ─── Helpers ─────────────────────────────────────────────────────────────

  function addMarker(
    vehicle: Vehicle,
    loc: LocationDto,
    onSelect: (id: number | null) => void,
  ) {
    if (!mapRef.current) return;

    const marker = new window.ymaps.Placemark(
      [loc.latitude, loc.longitude],
      {
        hintContent: `${vehicle.name} · ${vehicle.plateNumber}`,
        balloonContent: buildBalloonContent(vehicle, loc),
        iconCaption: vehicle.name,
      },
      {
        preset: vehiclePreset(vehicle.id),
        iconCaptionMaxWidth: '150',
      },
    );

    marker.events.add('click', () => onSelect(vehicle.id));
    mapRef.current.geoObjects.add(marker);
    markersRef.current.set(vehicle.id, marker);
  }

  function fitPolylineBounds(path: LocationHistoryDto[]) {
    if (!mapRef.current || path.length === 0) return;
    const lats = path.map((p) => p.latitude);
    const lngs = path.map((p) => p.longitude);
    const sw: [number, number] = [Math.min(...lats), Math.min(...lngs)];
    const ne: [number, number] = [Math.max(...lats), Math.max(...lngs)];
    // Only zoom if bounding box has meaningful size
    if (Math.abs(ne[0] - sw[0]) > 0.001 || Math.abs(ne[1] - sw[1]) > 0.001) {
      mapRef.current.setCenter(
        [(sw[0] + ne[0]) / 2, (sw[1] + ne[1]) / 2],
        undefined,
        { duration: 500 },
      );
    }
  }

  // ─── Render ──────────────────────────────────────────────────────────────

  return (
    <div className="relative w-full h-full">
      <div ref={containerRef} className="w-full h-full" />

      {ymapsStatus === 'loading' && (
        <div className="absolute inset-0 flex items-center justify-center bg-surface-800/80">
          <span className="text-slate-400 text-sm animate-pulse">Loading map…</span>
        </div>
      )}

      {ymapsStatus === 'error' && (
        <div className="absolute inset-0 flex items-center justify-center bg-surface-800">
          <span className="text-red-400 text-sm">
            Failed to load Yandex Maps. Check your API key and network.
          </span>
        </div>
      )}
    </div>
  );
}

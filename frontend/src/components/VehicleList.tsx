import { useTrackingStore } from '../store/useTrackingStore';
import type { Vehicle } from '../types';

function StatusDot({ online }: { online: boolean }) {
  return (
    <span
      className={`inline-block w-2 h-2 rounded-full flex-shrink-0 ${
        online ? 'bg-emerald-400' : 'bg-slate-600'
      }`}
    />
  );
}

function VehicleRow({
  vehicle,
  isSelected,
  hasLocation,
  speed,
  onSelect,
}: {
  vehicle: Vehicle;
  isSelected: boolean;
  hasLocation: boolean;
  speed: number | null;
  onSelect: () => void;
}) {
  return (
    <button
      onClick={onSelect}
      className={`w-full text-left px-3 py-2.5 rounded-lg flex items-center gap-3 transition-colors ${
        isSelected
          ? 'bg-blue-600/30 border border-blue-500/40'
          : 'hover:bg-surface-700 border border-transparent'
      }`}
    >
      <StatusDot online={hasLocation} />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-slate-200 truncate">{vehicle.name}</p>
        <p className="text-xs text-slate-500 truncate">{vehicle.plateNumber}</p>
      </div>
      {speed !== null && (
        <span className="text-xs text-slate-400 flex-shrink-0">{speed.toFixed(0)} km/h</span>
      )}
    </button>
  );
}

export function VehicleList() {
  const vehicles = useTrackingStore((s) => s.vehicles);
  const currentLocations = useTrackingStore((s) => s.currentLocations);
  const selectedVehicleId = useTrackingStore((s) => s.selectedVehicleId);
  const isLoading = useTrackingStore((s) => s.isLoadingVehicles);
  const error = useTrackingStore((s) => s.vehiclesError);
  const setSelectedVehicleId = useTrackingStore((s) => s.setSelectedVehicleId);

  if (isLoading) {
    return (
      <div className="flex flex-col gap-2 px-3">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="h-12 rounded-lg bg-surface-700 animate-pulse" />
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div className="px-3">
        <p className="text-xs text-red-400 leading-relaxed">{error}</p>
      </div>
    );
  }

  if (vehicles.length === 0) {
    return (
      <div className="px-3">
        <p className="text-xs text-slate-500">No vehicles found.</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-1 px-2">
      {vehicles.map((v: Vehicle) => {
        const loc = currentLocations[v.id];
        return (
          <VehicleRow
            key={v.id}
            vehicle={v}
            isSelected={selectedVehicleId === v.id}
            hasLocation={!!loc}
            speed={loc?.speed ?? null}
            onSelect={() =>
              setSelectedVehicleId(selectedVehicleId === v.id ? null : v.id)
            }
          />
        );
      })}
    </div>
  );
}

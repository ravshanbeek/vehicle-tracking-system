import { useState } from 'react';
import { fetchLocationHistory } from '../services/api';
import { useTrackingStore } from '../store/useTrackingStore';

function toLocalDatetimeValue(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return (
    `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
    `T${pad(date.getHours())}:${pad(date.getMinutes())}`
  );
}

function defaultRange() {
  const to = new Date();
  const from = new Date(to.getTime() - 24 * 60 * 60 * 1000);
  return { from: toLocalDatetimeValue(from), to: toLocalDatetimeValue(to) };
}

export function HistoryPanel() {
  const selectedVehicleId = useTrackingStore((s) => s.selectedVehicleId);
  const vehicles = useTrackingStore((s) => s.vehicles);
  const setHistoryPath = useTrackingStore((s) => s.setHistoryPath);
  const clearHistory = useTrackingStore((s) => s.clearHistory);
  const isLoading = useTrackingStore((s) => s.isLoadingHistory);
  const setLoadingHistory = useTrackingStore((s) => s.setLoadingHistory);
  const historyError = useTrackingStore((s) => s.historyError);
  const setHistoryError = useTrackingStore((s) => s.setHistoryError);
  const historyPath = useTrackingStore((s) => s.historyPath);

  const [range, setRange] = useState(defaultRange);

  const selectedVehicle = vehicles.find((v) => v.id === selectedVehicleId);

  async function handleLoad() {
    if (selectedVehicleId === null) return;

    setLoadingHistory(true);
    setHistoryError(null);
    clearHistory();

    try {
      const path = await fetchLocationHistory({
        vehicleId: selectedVehicleId,
        from: new Date(range.from).toISOString(),
        to: new Date(range.to).toISOString(),
      });
      setHistoryPath(path);
      if (path.length === 0) {
        setHistoryError('No location data in the selected range.');
      }
    } catch (err) {
      setHistoryError(err instanceof Error ? err.message : 'Failed to load history.');
    } finally {
      setLoadingHistory(false);
    }
  }

  function handleClear() {
    clearHistory();
    setHistoryError(null);
  }

  const disabled = selectedVehicleId === null || isLoading;

  return (
    <div className="flex flex-col gap-3">
      {/* Header */}
      <div>
        <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
          Route History
        </p>
        {selectedVehicle && (
          <p className="text-xs text-slate-500 mt-0.5 truncate">{selectedVehicle.name}</p>
        )}
        {!selectedVehicle && (
          <p className="text-xs text-slate-600 mt-0.5">Select a vehicle above</p>
        )}
      </div>

      {/* Date range inputs */}
      <div className="flex flex-col gap-2">
        <div>
          <label className="block text-xs text-slate-500 mb-1">From</label>
          <input
            type="datetime-local"
            value={range.from}
            onChange={(e) => setRange((r) => ({ ...r, from: e.target.value }))}
            disabled={disabled}
            className="w-full bg-surface-700 border border-slate-700 rounded-md px-2 py-1.5 text-xs text-slate-300 focus:outline-none focus:border-blue-500 disabled:opacity-40 disabled:cursor-not-allowed"
          />
        </div>
        <div>
          <label className="block text-xs text-slate-500 mb-1">To</label>
          <input
            type="datetime-local"
            value={range.to}
            onChange={(e) => setRange((r) => ({ ...r, to: e.target.value }))}
            disabled={disabled}
            className="w-full bg-surface-700 border border-slate-700 rounded-md px-2 py-1.5 text-xs text-slate-300 focus:outline-none focus:border-blue-500 disabled:opacity-40 disabled:cursor-not-allowed"
          />
        </div>
      </div>

      {/* Actions */}
      <div className="flex gap-2">
        <button
          onClick={handleLoad}
          disabled={disabled}
          className="flex-1 py-1.5 rounded-md bg-blue-600 hover:bg-blue-500 disabled:opacity-40 disabled:cursor-not-allowed text-xs font-medium text-white transition-colors"
        >
          {isLoading ? 'Loading…' : 'Load Route'}
        </button>
        {historyPath.length > 0 && (
          <button
            onClick={handleClear}
            className="px-3 py-1.5 rounded-md bg-surface-600 hover:bg-surface-700 text-xs font-medium text-slate-400 transition-colors"
          >
            Clear
          </button>
        )}
      </div>

      {/* Feedback */}
      {historyError && (
        <p className="text-xs text-amber-400 leading-relaxed">{historyError}</p>
      )}
      {historyPath.length > 0 && !historyError && (
        <p className="text-xs text-emerald-400">
          {historyPath.length} points loaded
        </p>
      )}
    </div>
  );
}

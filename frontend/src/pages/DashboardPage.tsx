import { useSignalR } from '../hooks/useSignalR';
import { useVehicles } from '../hooks/useVehicles';
import { MapView } from '../components/MapView';
import { VehicleList } from '../components/VehicleList';
import { HistoryPanel } from '../components/HistoryPanel';
import { useTrackingStore } from '../store/useTrackingStore';
import type { SignalRStatus } from '../types';

function SignalRBadge({ status }: { status: SignalRStatus }) {
  const styles: Record<SignalRStatus, string> = {
    connected: 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
    connecting: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30 animate-pulse',
    reconnecting: 'bg-orange-500/20 text-orange-400 border-orange-500/30 animate-pulse',
    disconnected: 'bg-red-500/20 text-red-400 border-red-500/30',
  };
  const labels: Record<SignalRStatus, string> = {
    connected: 'Live',
    connecting: 'Connecting…',
    reconnecting: 'Reconnecting…',
    disconnected: 'Offline',
  };

  return (
    <span
      className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium border ${styles[status]}`}
    >
      <span className="w-1.5 h-1.5 rounded-full bg-current" />
      {labels[status]}
    </span>
  );
}

export function DashboardPage() {
  // Boot hooks — connect SignalR and load vehicles
  useSignalR();
  useVehicles();

  const signalRStatus = useTrackingStore((s) => s.signalRStatus);
  const vehicleCount = useTrackingStore((s) => s.vehicles.length);

  return (
    <div className="flex flex-col h-screen bg-surface-900 text-slate-200 overflow-hidden">
      {/* ── Header ─────────────────────────────────────────────────────────── */}
      <header className="flex items-center justify-between px-4 h-12 bg-surface-800 border-b border-slate-700/60 flex-shrink-0">
        <div className="flex items-center gap-3">
          <span className="text-sm font-semibold text-white tracking-tight">
            CarTracking
          </span>
          {vehicleCount > 0 && (
            <span className="text-xs text-slate-500">{vehicleCount} vehicles</span>
          )}
        </div>
        <SignalRBadge status={signalRStatus} />
      </header>

      {/* ── Body ───────────────────────────────────────────────────────────── */}
      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar */}
        <aside className="w-64 flex-shrink-0 bg-surface-800 border-r border-slate-700/60 flex flex-col overflow-hidden">
          {/* Vehicle list */}
          <div className="flex-1 overflow-y-auto py-3">
            <p className="px-4 text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
              Vehicles
            </p>
            <VehicleList />
          </div>

          {/* Divider */}
          <div className="border-t border-slate-700/60" />

          {/* History panel */}
          <div className="p-3 flex-shrink-0">
            <HistoryPanel />
          </div>
        </aside>

        {/* Map */}
        <main className="flex-1 overflow-hidden">
          <MapView />
        </main>
      </div>
    </div>
  );
}

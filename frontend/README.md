# CarTracking Frontend

Real-time vehicle tracking dashboard built with React + TypeScript + Vite + Yandex Maps + SignalR.

## Prerequisites

- Node.js 18+
- Backend running at `http://localhost:8080` (Docker) or `http://localhost:5000` (local dev)

## Setup

```bash
# 1. Install dependencies
npm install

# 2. Create environment file
cp .env.example .env
# Edit .env — set VITE_API_URL and optionally VITE_YANDEX_MAPS_API_KEY

# 3. Start dev server
npm run dev
# → http://localhost:3000
```

## Environment variables

| Variable | Default | Description |
|---|---|---|
| `VITE_API_URL` | `http://localhost:8080` | Backend base URL (no trailing slash) |
| `VITE_YANDEX_MAPS_API_KEY` | _(empty)_ | Yandex Maps JS API v2.1 key — get one at [developer.tech.yandex.ru](https://developer.tech.yandex.ru/). Leave empty to use trial mode (watermark shown). |

## Build for production

```bash
npm run build
# Output in dist/
```

## Project structure

```
src/
  config/
    api.ts              ← All endpoint URLs derived from VITE_API_URL (change here only)
  services/
    api.ts              ← Axios-based REST client (fetchVehicles, fetchLocationHistory, …)
    signalr.ts          ← SignalR singleton with auto-reconnect + group management
  store/
    useTrackingStore.ts ← Zustand store (vehicles, currentLocations, selectedVehicle, historyPath)
  hooks/
    useVehicles.ts      ← Fetches vehicles, loads initial locations, joins SignalR groups
    useSignalR.ts       ← Connects hub, forwards LocationUpdated to store
    useYandexMaps.ts    ← Lazy-loads Yandex Maps script once
  components/
    MapView.tsx         ← Yandex Map: markers in refs (no re-render on location update)
    VehicleList.tsx     ← Sidebar list with live speed readout
    HistoryPanel.tsx    ← Date range picker → fetches history → draws polyline
  pages/
    DashboardPage.tsx   ← Layout: header + sidebar + map
  types/
    index.ts            ← Domain interfaces matching backend DTOs exactly
    ymaps.d.ts          ← Minimal Yandex Maps global type declarations
```

## Integration points

| What | Where to change |
|---|---|
| Backend URL | `VITE_API_URL` in `.env` |
| Endpoint paths | `src/config/api.ts` → `endpoints` object |
| SignalR hub URL | `src/config/api.ts` → `signalrHub` |
| SignalR event name (`LocationUpdated`) | `src/services/signalr.ts` → `onLocationUpdated` |
| Hub method names (`JoinVehicleGroup`) | `src/services/signalr.ts` → `joinVehicleGroup` |
| Map default center / zoom | `src/components/MapView.tsx` → `DEFAULT_CENTER`, `DEFAULT_ZOOM` |

## Performance notes

- Marker positions are updated via `geometry.setCoordinates()` — no React re-render, no DOM diff.
- Zustand `subscribeWithSelector` used for location updates so MapView never re-renders on real-time data.
- Yandex Maps script is loaded once via a shared promise (idempotent across HMR reloads).
- Each vehicle's SignalR group is joined once; on reconnect all groups are automatically re-joined.

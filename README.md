# Vehicle Tracking System

A full-stack, real-time vehicle tracking platform: a **.NET 8** backend streams
live GPS positions over **WebSockets (SignalR)** to a **React + TypeScript**
dashboard that renders moving vehicles on a map. A companion **GPS simulator**
drives virtual vehicles along **real Tashkent roads** so the whole system can be
demoed end-to-end without any hardware.

> **Stack:** ASP.NET Core 8 · SignalR · EF Core · PostgreSQL · React 18 · TypeScript · Vite · Zustand · Tailwind · Yandex Maps · Docker

<!-- TODO: add a screenshot or GIF of the live map here -->
<!-- ![Live tracking demo](docs/demo.gif) -->

---

## Architecture

```
┌─────────────────┐     POST /api/location/update      ┌──────────────────────┐
│  GPS Simulator  │ ──────────────────────────────────▶│   ASP.NET Core API    │
│  (.NET console) │   real OSRM routes, 5 vehicles      │  Clean Architecture   │
└─────────────────┘                                     │  EF Core + PostgreSQL │
                                                         └───────────┬──────────┘
                                                                     │ SignalR
                                                       LocationUpdated │ broadcast
                                                                     ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                    React + TypeScript Dashboard                                │
│   Yandex Maps (live markers) · Vehicle list · History playback · Live status   │
└──────────────────────────────────────────────────────────────────────────────┘
```

Three independent parts, each runnable on its own:

| Folder        | What it is                                                              |
|---------------|-------------------------------------------------------------------------|
| [`backend/`](backend)   | ASP.NET Core 8 Web API — REST + SignalR, clean 4-layer architecture |
| [`simulator/`](simulator) | .NET console app — drives 5 vehicles along real OSRM road paths     |
| [`frontend/`](frontend)  | React + TypeScript + Vite dashboard with a live Yandex map         |

---

## Features

- **Real-time tracking** — every GPS update is broadcast to subscribed clients over SignalR; the map moves live.
- **Live map** — Yandex Maps with per-vehicle markers updated via `geometry.setCoordinates()` (no React re-render on location data).
- **Per-vehicle & all-vehicles subscriptions** — SignalR groups let a client follow one vehicle or the whole fleet.
- **History playback** — pick a date range and the dashboard draws the vehicle's route as a polyline.
- **Connection status** — a live/connecting/reconnecting/offline badge driven by the SignalR connection state.
- **Realistic demo data** — the simulator fetches actual driving paths from OSRM, so vehicles follow real Tashkent streets.

---

## Quick Start

### 1. Backend (Docker — recommended)

```bash
cd backend
docker-compose up --build
# API:     http://localhost:8080
# Swagger: http://localhost:8080/swagger
```

Migrations are applied automatically on startup.

### 2. Simulator

```bash
cd simulator
dotnet run --project GpsSimulator
# Creates 5 vehicles and starts streaming GPS updates. Press Enter to stop.
```

### 3. Frontend

```bash
cd frontend
npm install
cp .env.example .env        # optionally set VITE_YANDEX_MAPS_API_KEY
npm run dev                 # http://localhost:3000
```

Open the dashboard and watch the vehicles move in real time.

---

## Design highlights

**Backend**
- Clean architecture with a strict dependency flow: `API → Infrastructure → Application → Domain`.
- `VehicleCurrentLocations` upsert uses PostgreSQL `ON CONFLICT DO UPDATE` — no SELECT round-trip.
- History reads hit a `(VehicleId, RecordedAt)` composite index via projection — no full entity load.
- SignalR broadcast is fire-and-forget: a hub failure logs a warning but never fails the HTTP response.
- Global exception middleware maps domain errors to status codes (`404 / 400 / 409 / 500`).

**Frontend**
- Zustand with `subscribeWithSelector` so the map never re-renders on high-frequency location data.
- Single SignalR connection with automatic reconnect and group management.
- All endpoint/hub URLs derived from one config (`src/config/api.ts`).

**Simulator**
- 5 vehicles run concurrently; each posts every 2s and loops its route.
- Real road geometry from OSRM, with a graceful fallback to raw waypoints if OSRM is unreachable.
- Graceful shutdown via `CancellationToken` (Enter or Ctrl+C).

---

## Tests

```bash
cd backend
dotnet test
```

Unit tests (xUnit + Moq) cover the application services — vehicle creation/mapping and
the location pipeline (vehicle-existence guard, history + current-location writes, and
the real-time broadcast).

---

## Tech stack

| Layer      | Technologies                                                                 |
|------------|------------------------------------------------------------------------------|
| Backend    | ASP.NET Core 8, C#, SignalR, EF Core, PostgreSQL 16, Swagger, Docker          |
| Simulator  | .NET 8 console, `HttpClient`, OSRM routing API                                |
| Frontend   | React 18, TypeScript, Vite, Zustand, Tailwind CSS, Yandex Maps, @microsoft/signalr |
| Testing    | xUnit, Moq                                                                    |

---

## Notes & limitations

- Authentication is intentionally omitted (demo scope) — endpoints are public.
- CORS is open for local development; restrict origins in production.
- The Yandex Maps key is optional; without it the map runs in trial mode (watermark).

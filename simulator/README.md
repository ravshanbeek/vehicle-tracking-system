# GPS Simulator

A small .NET console app that drives **5 virtual vehicles along real Tashkent
roads** and streams their GPS positions to the CarTracking backend — so the
whole system (backend → SignalR → map) can be demoed without any hardware.

## How it works

1. Ensures 5 vehicles exist in the backend (creates them if missing).
2. Fetches **real driving paths** from the public [OSRM](http://project-osrm.org/)
   routing API for each vehicle's waypoints (falls back to raw waypoints if OSRM
   is unreachable).
3. Runs all vehicles concurrently — each posts a location update every 2 seconds
   and loops around its route.

Each `POST /api/location/update` triggers a SignalR broadcast, so connected map
clients see the vehicles move live.

## Run

```bash
# Backend must be running first (see ../backend)
dotnet run --project GpsSimulator

# Custom backend URL (CLI arg or env var)
dotnet run --project GpsSimulator -- http://localhost:5000
BACKEND_URL=http://localhost:5000 dotnet run --project GpsSimulator
```

Press **Enter** (or Ctrl+C) to stop all vehicles gracefully.

## Stack

.NET 8 · `HttpClient` · OSRM routing API · concurrent `Task`-based simulation

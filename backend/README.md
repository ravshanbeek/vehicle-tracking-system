# CarTracking — Real-Time Vehicle Tracking Backend

ASP.NET Core 8 Web API with PostgreSQL, Entity Framework Core, and SignalR.

## Architecture

```
CarTracking.Domain          — Entities only; zero dependencies
CarTracking.Application     — DTOs, service interfaces, service implementations
CarTracking.Infrastructure  — EF Core DbContext, Fluent API config, repositories
CarTracking.API             — Controllers, SignalR hub, middleware, DI wiring
```

Dependency flow: `API → Infrastructure → Application → Domain`

---

## Quick Start (Docker)

```bash
docker-compose up --build
```

The API starts on **http://localhost:8080**.  
Swagger UI: **http://localhost:8080/swagger**

Migrations are applied automatically on startup.

---

## Local Development

### Prerequisites
- .NET 8 SDK
- PostgreSQL 16 running locally (or use `docker-compose up postgres`)

### Run

```bash
cd src/CarTracking.API
dotnet run
```

The app reads `appsettings.Development.json` and connects to `localhost:5432`.

### Create first migration (if you modify the model)

```bash
cd src/CarTracking.Infrastructure
dotnet ef migrations add YourMigrationName \
  --startup-project ../CarTracking.API \
  --output-dir Migrations
```

---

## API Reference

### Vehicles

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/vehicles` | Create a vehicle |
| GET | `/api/vehicles` | List all vehicles |

**POST /api/vehicles**
```json
{ "name": "Truck Alpha", "plateNumber": "34-ABC-001" }
```

---

### Location

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/location/update` | Push a GPS update |
| GET | `/api/location/current?vehicleId=1` | Latest location |
| GET | `/api/location/history?vehicleId=1&from=...&to=...` | History range |

**POST /api/location/update**
```json
{
  "vehicleId": 1,
  "latitude": 41.0082,
  "longitude": 28.9784,
  "speed": 65.5,
  "recordedAt": "2024-06-15T10:30:00Z"
}
```

Validation:
- `latitude` — `-90` to `90`
- `longitude` — `-180` to `180`
- `speed` — non-negative
- `vehicleId` — must reference an existing vehicle (404 if not found)

---

## Real-Time (SignalR)

Hub endpoint: `ws://localhost:8080/hubs/vehicles`

### Client methods (invoke on server)

```js
await connection.invoke("JoinVehicleGroup", vehicleId);   // subscribe to one vehicle
await connection.invoke("LeaveVehicleGroup", vehicleId);  // unsubscribe
```

### Server events (listen on client)

```js
connection.on("LocationUpdated", (location) => {
  // { vehicleId, latitude, longitude, speed, recordedAt }
});
```

Every `POST /api/location/update` broadcasts `LocationUpdated` to:
- `vehicle-{vehicleId}` group
- `all-vehicles` group

For a full live demo, run the [React dashboard](../frontend) and the
[GPS simulator](../simulator) against this API.

---

## Design Notes

- **Upsert** on `VehicleCurrentLocations` uses PostgreSQL `ON CONFLICT DO UPDATE` — no SELECT round-trip.
- **History query** hits the `(VehicleId, RecordedAt)` composite index directly via projection (`Select`), no full entity load.
- **SignalR broadcast failure** is fire-and-forget — it logs a warning but never fails the HTTP 204 response.
- **Global exception middleware** maps `KeyNotFoundException → 404`, `ArgumentException → 400`, unhandled → 500.
- CORS is open for MVP; restrict in production via `AllowedOrigins` config.


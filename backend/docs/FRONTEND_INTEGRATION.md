# CarTracking Backend

---

## Rules

### Architecture
- Clean Architecture: Domain → Application → Infrastructure → API. Dependency faqat ichkariga qarab bo'ladi.
- Domain layerda hech qanday tashqi dependency bo'lmasin (EF Core, ASP.NET va h.k.).
- Application layer Infrastructure haqida bilmasin — faqat interface orqali ishlaydi.
- Yangi feature qo'shsang, shu 4 layerga to'g'ri joylashtir.

### Naming Conventions
- **Entities**: `sealed class`, PascalCase (`Vehicle`, `LocationHistory`)
- **DTOs**: `sealed record`, PascalCase. Read DTO = `XxxDto`, Request = `XxxRequest`, Query = `XxxQuery`
- **Interfaces**: `I` prefix (`IVehicleService`, `ILocationRepository`)
- **Implementations**: interface nomidan `I` olib tashlangan holda (`VehicleService`, `LocationRepository`)
- **Controllers**: plural nom (`VehiclesController`), `sealed class`, primary constructor
- **Configuration**: `XxxConfiguration` (`VehicleConfiguration`)
- **Async methods**: `Async` suffix (`CreateAsync`, `GetAllAsync`), har doim `CancellationToken ct` parametri bo'lsin

### Code Style
- `sealed` — barcha class va recordlarga qo'y (entity, DTO, service, repository, controller, middleware)
- Primary constructors — DI injection uchun ishlat
- `#nullable enable` — har doim yoqiq
- `AsNoTracking()` — read-only querylar uchun
- Raw SQL — faqat performance-critical joylar uchun (masalan, upsert). Har doim parameterized query ishlat.
- Fire-and-forget — faqat non-critical operatsiyalar uchun (masalan, SignalR broadcast). Log failure ni `ContinueWith` bilan ushla.

### Error Handling
- Service layerda: `KeyNotFoundException` → 404, `ArgumentException` → 400
- Middleware (`GlobalExceptionMiddleware`) barcha exceptionlarni ushlaydi
- PostgreSQL `UniqueViolation` → 409 Conflict
- Response format: `{ "error": "message" }`
- Yangi exception turini qo'shsang, middlewarega ham mapping qo'sh

### Database
- PostgreSQL 16, EF Core 8 with Npgsql
- Fluent API configuration (`IEntityTypeConfiguration<T>`) — Data Annotations faqat DTOlarda
- Migration: `dotnet ef migrations add MigrationName -p src/CarTracking.Infrastructure -s src/CarTracking.API`
- Auto-migrate: ilova ishga tushganda migratsiyalar avtomatik qo'llanadi

### DI Registration
- Repository va Service → `AddScoped`
- Broadcaster (SignalR wrapper) → `AddSingleton`
- Yangi service/repo qo'shsang `ServiceExtensions.cs` ga yoz

### Testing
- Hozircha test loyihasi yo'q. Test qo'shish kerak bo'lsa, xUnit ishlat.

---

## Skills

### Build & Run
```bash
# Docker (recommended)
docker-compose up --build

# Local dev
cd src/CarTracking.API && dotnet run

# Build only
dotnet build

# Run migrations manually
dotnet ef database update -p src/CarTracking.Infrastructure -s src/CarTracking.API
```

### Add New Entity
1. `Domain/Entities/` — yangi `sealed class` yarat
2. `Infrastructure/Data/Configurations/` — `IEntityTypeConfiguration<T>` yoz
3. `AppDbContext.cs` — `DbSet<T>` qo'sh, `OnModelCreating` da configuration qo'sh
4. Migration yarat: `dotnet ef migrations add AddXxx -p src/CarTracking.Infrastructure -s src/CarTracking.API`

### Add New Endpoint
1. `Application/DTOs/` — kerakli DTO/Request/Query recordlarni yoz
2. `Application/Interfaces/` — service va repository interfacega method qo'sh
3. `Application/Services/` — service implementatsiyasini yoz
4. `Infrastructure/Repositories/` — repository implementatsiyasini yoz
5. `API/Controllers/` — controller action qo'sh, `[ProducesResponseType]` bilan bezat
6. Agar yangi service/repo bo'lsa → `ServiceExtensions.cs` ga DI register qil

### Add New SignalR Event
1. `Application/Interfaces/ILocationBroadcaster.cs` — yangi method qo'sh
2. `API/Hubs/VehicleLocationBroadcaster.cs` — implementatsiya yoz
3. Hub group strategiyasi: `vehicle-{vehicleId}` (per-vehicle), `all-vehicles` (global)
4. Event nomi: PascalCase (`LocationUpdated`, `VehicleCreated`)

### Common Paths
| Nima | Qayerda |
|------|---------|
| Controllers | `src/CarTracking.API/Controllers/` |
| DTOs | `src/CarTracking.Application/DTOs/` |
| Service interfaces | `src/CarTracking.Application/Interfaces/` |
| Service implementations | `src/CarTracking.Application/Services/` |
| Repository interfaces | `src/CarTracking.Application/Interfaces/` |
| Repository implementations | `src/CarTracking.Infrastructure/Repositories/` |
| Entities | `src/CarTracking.Domain/Entities/` |
| EF Configurations | `src/CarTracking.Infrastructure/Data/Configurations/` |
| DbContext | `src/CarTracking.Infrastructure/Data/AppDbContext.cs` |
| DI wiring | `src/CarTracking.API/Extensions/ServiceExtensions.cs` |
| Middleware | `src/CarTracking.API/Middleware/` |
| SignalR Hub | `src/CarTracking.API/Hubs/VehicleHub.cs` |
| Broadcaster | `src/CarTracking.API/Hubs/VehicleLocationBroadcaster.cs` |
| Migrations | `src/CarTracking.Infrastructure/Migrations/` |

---

## Frontend Integration Guide

## Stack

- ASP.NET Core 8, C#, PostgreSQL 16, Entity Framework Core, SignalR
- Base URL (dev): `http://localhost:5000` | (Docker): `http://localhost:8080`
- No authentication — all endpoints are public (MVP)
- CORS: open to all origins

## Run

```bash
# Docker (recommended)
docker-compose up --build

# Local dev
cd src/CarTracking.API && dotnet run
```

Swagger: `http://localhost:8080/swagger` (dev only)

---

## REST API

### Vehicles

#### POST /api/vehicles — Create vehicle
```json
// Request
{ "name": "Truck Alpha", "plateNumber": "34-ABC-001" }

// Response 201
{ "id": 1, "name": "Truck Alpha", "plateNumber": "34-ABC-001", "createdAt": "2024-06-15T10:00:00Z" }

// Error 400 – validation failure
// Error 409 – plateNumber already exists
```

#### GET /api/vehicles — List all vehicles
```json
// Response 200
[{ "id": 1, "name": "Truck Alpha", "plateNumber": "34-ABC-001", "createdAt": "..." }]
```

---

### Location

#### POST /api/location/update — Push GPS update (triggers SignalR broadcast)
```json
// Request
{
  "vehicleId": 1,
  "latitude": 41.0082,       // -90 to 90
  "longitude": 28.9784,      // -180 to 180
  "speed": 65.5,             // >= 0
  "recordedAt": "2024-06-15T10:30:00Z"  // ISO 8601
}

// Response 204 No Content
// Error 400 – validation
// Error 404 – vehicleId not found
```

#### GET /api/location/current?vehicleId=1 — Latest position
```json
// Response 200
{ "vehicleId": 1, "latitude": 41.0082, "longitude": 28.9784, "speed": 65.5, "recordedAt": "..." }

// Error 404 – no location recorded yet
```

#### GET /api/location/history?vehicleId=1&from=...&to=... — Historical track
```
Query params: vehicleId (long), from (ISO 8601), to (ISO 8601)
```
```json
// Response 200
[{ "id": 1, "vehicleId": 1, "latitude": 41.0082, "longitude": 28.9784, "speed": 65.5, "recordedAt": "..." }]

// Error 400 – missing/invalid params
```

---

## Real-Time (SignalR)

**Hub URL:** `http://localhost:8080/hubs/vehicles`

```js
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:8080/hubs/vehicles")
  .withAutomaticReconnect()
  .build();

// Subscribe to a specific vehicle
await connection.start();
await connection.invoke("JoinVehicleGroup", vehicleId);   // number
// await connection.invoke("LeaveVehicleGroup", vehicleId);

// Listen for updates (fired by every POST /api/location/update)
connection.on("LocationUpdated", (location) => {
  // { vehicleId, latitude, longitude, speed, recordedAt }
});
```

**Groups:**
- `vehicle-{vehicleId}` — per-vehicle subscribers (use `JoinVehicleGroup`)
- `all-vehicles` — every update (join this group to track all vehicles at once)

---

## Error Response Shape

All errors follow:
```json
{ "error": "human readable message" }
```

| Status | When |
|--------|------|
| 400 | Validation failure or bad query params |
| 404 | Vehicle or location not found |
| 409 | Duplicate plate number |
| 500 | Unexpected server error |

---

## Key Data Shapes (TypeScript)

```ts
interface Vehicle {
  id: number;
  name: string;
  plateNumber: string;
  createdAt: string; // ISO 8601 UTC
}

interface LocationDto {
  vehicleId: number;
  latitude: number;
  longitude: number;
  speed: number;
  recordedAt: string; // ISO 8601 UTC
}

interface LocationHistoryDto extends LocationDto {
  id: number;
}
```

---

## Project Structure (for context only — do not modify these without reading the layer)

```
src/
  CarTracking.Domain/       → Entities (Vehicle, LocationHistory, VehicleCurrentLocation)
  CarTracking.Application/  → DTOs, service interfaces, service implementations
  CarTracking.Infrastructure/→ EF Core DbContext, repositories, migrations
  CarTracking.API/          → Controllers, SignalR hub, middleware, DI wiring
```

- **Controllers:** `src/CarTracking.API/Controllers/`
- **DTOs:** `src/CarTracking.Application/DTOs/`
- **Hub:** `src/CarTracking.API/Hubs/VehicleHub.cs`
- **Entities:** `src/CarTracking.Domain/Entities/`

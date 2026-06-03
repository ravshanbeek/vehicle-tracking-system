using CarTracking.Application.DTOs;
using CarTracking.Application.Interfaces;
using CarTracking.Domain.Entities;
using CarTracking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CarTracking.Infrastructure.Repositories;

public sealed class LocationRepository(AppDbContext db) : ILocationRepository
{
    public async Task AddHistoryAsync(LocationHistory entry, CancellationToken ct = default)
    {
        db.LocationHistory.Add(entry);
        await db.SaveChangesAsync(ct);
    }

    // Raw SQL upsert using PostgreSQL's ON CONFLICT — avoids a round-trip SELECT
    public async Task UpsertCurrentLocationAsync(VehicleCurrentLocation location, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO "VehicleCurrentLocations" ("VehicleId", "Latitude", "Longitude", "Speed", "RecordedAt")
            VALUES (@vehicleId, @latitude, @longitude, @speed, @recordedAt)
            ON CONFLICT ("VehicleId") DO UPDATE SET
                "Latitude"   = EXCLUDED."Latitude",
                "Longitude"  = EXCLUDED."Longitude",
                "Speed"      = EXCLUDED."Speed",
                "RecordedAt" = EXCLUDED."RecordedAt"
            """;

        await db.Database.ExecuteSqlRawAsync(sql,
            [
                new NpgsqlParameter("vehicleId",   location.VehicleId),
                new NpgsqlParameter("latitude",    location.Latitude),
                new NpgsqlParameter("longitude",   location.Longitude),
                new NpgsqlParameter("speed",       location.Speed),
                new NpgsqlParameter("recordedAt",  location.RecordedAt)
            ],
            ct);
    }

    public Task<LocationDto?> GetCurrentLocationAsync(long vehicleId, CancellationToken ct = default)
        => db.VehicleCurrentLocations
             .AsNoTracking()
             .Where(c => c.VehicleId == vehicleId)
             .Select(c => new LocationDto(c.VehicleId, c.Latitude, c.Longitude, c.Speed, c.RecordedAt))
             .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<LocationHistoryDto>> GetHistoryAsync(
        long vehicleId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        // Relies on the (VehicleId, RecordedAt) composite index
        return await db.LocationHistory
            .AsNoTracking()
            .Where(h => h.VehicleId == vehicleId && h.RecordedAt >= from && h.RecordedAt <= to)
            .OrderBy(h => h.RecordedAt)
            .Select(h => new LocationHistoryDto(h.Id, h.VehicleId, h.Latitude, h.Longitude, h.Speed, h.RecordedAt))
            .ToListAsync(ct);
    }
}

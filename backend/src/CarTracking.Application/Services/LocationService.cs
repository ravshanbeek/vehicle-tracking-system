using CarTracking.Application.DTOs;
using CarTracking.Application.Interfaces;
using CarTracking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CarTracking.Application.Services;

public sealed class LocationService(
    IVehicleRepository vehicleRepository,
    ILocationRepository locationRepository,
    ILocationBroadcaster broadcaster,
    ILogger<LocationService> logger) : ILocationService
{
    public async Task UpdateLocationAsync(LocationUpdateRequest request, CancellationToken ct = default)
    {
        // Validate vehicle exists before writing any records
        if (!await vehicleRepository.ExistsAsync(request.VehicleId, ct))
            throw new KeyNotFoundException($"Vehicle {request.VehicleId} not found.");

        var history = new LocationHistory
        {
            VehicleId = request.VehicleId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Speed = request.Speed,
            RecordedAt = request.RecordedAt
        };

        var current = new VehicleCurrentLocation
        {
            VehicleId = request.VehicleId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Speed = request.Speed,
            RecordedAt = request.RecordedAt
        };

        // Both writes happen inside the repository; the upsert uses ON CONFLICT
        await locationRepository.AddHistoryAsync(history, ct);
        await locationRepository.UpsertCurrentLocationAsync(current, ct);

        var dto = new LocationDto(
            request.VehicleId,
            request.Latitude,
            request.Longitude,
            request.Speed,
            request.RecordedAt);

        // Fire-and-forget broadcast — a SignalR failure must NOT fail the HTTP response
        _ = broadcaster.BroadcastAsync(dto, ct).ContinueWith(
            t => logger.LogWarning(t.Exception, "SignalR broadcast failed for vehicle {VehicleId}", request.VehicleId),
            TaskContinuationOptions.OnlyOnFaulted);

        logger.LogDebug("Location updated for vehicle {VehicleId} at {RecordedAt}", request.VehicleId, request.RecordedAt);
    }

    public Task<LocationDto?> GetCurrentLocationAsync(long vehicleId, CancellationToken ct = default)
        => locationRepository.GetCurrentLocationAsync(vehicleId, ct);

    public Task<IReadOnlyList<LocationHistoryDto>> GetHistoryAsync(LocationHistoryQuery query, CancellationToken ct = default)
        => locationRepository.GetHistoryAsync(query.VehicleId, query.From, query.To, ct);
}

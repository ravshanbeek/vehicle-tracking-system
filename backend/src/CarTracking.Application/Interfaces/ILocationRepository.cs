using CarTracking.Application.DTOs;
using CarTracking.Domain.Entities;

namespace CarTracking.Application.Interfaces;

public interface ILocationRepository
{
    Task AddHistoryAsync(LocationHistory entry, CancellationToken ct = default);
    Task UpsertCurrentLocationAsync(VehicleCurrentLocation location, CancellationToken ct = default);
    Task<LocationDto?> GetCurrentLocationAsync(long vehicleId, CancellationToken ct = default);
    Task<IReadOnlyList<LocationHistoryDto>> GetHistoryAsync(long vehicleId, DateTime from, DateTime to, CancellationToken ct = default);
}

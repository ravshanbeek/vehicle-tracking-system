using CarTracking.Application.DTOs;

namespace CarTracking.Application.Interfaces;

public interface ILocationService
{
    Task UpdateLocationAsync(LocationUpdateRequest request, CancellationToken ct = default);
    Task<LocationDto?> GetCurrentLocationAsync(long vehicleId, CancellationToken ct = default);
    Task<IReadOnlyList<LocationHistoryDto>> GetHistoryAsync(LocationHistoryQuery query, CancellationToken ct = default);
}

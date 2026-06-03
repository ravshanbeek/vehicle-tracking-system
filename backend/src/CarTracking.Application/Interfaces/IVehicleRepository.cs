using CarTracking.Domain.Entities;

namespace CarTracking.Application.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle> CreateAsync(Vehicle vehicle, CancellationToken ct = default);
    Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(long vehicleId, CancellationToken ct = default);
}

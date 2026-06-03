using CarTracking.Application.DTOs;

namespace CarTracking.Application.Interfaces;

public interface IVehicleService
{
    Task<VehicleDto> CreateAsync(CreateVehicleRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<VehicleDto>> GetAllAsync(CancellationToken ct = default);
}

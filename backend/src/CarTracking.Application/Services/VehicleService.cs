using CarTracking.Application.DTOs;
using CarTracking.Application.Interfaces;
using CarTracking.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CarTracking.Application.Services;

public sealed class VehicleService(
    IVehicleRepository vehicleRepository,
    ILogger<VehicleService> logger) : IVehicleService
{
    public async Task<VehicleDto> CreateAsync(CreateVehicleRequest request, CancellationToken ct = default)
    {
        var vehicle = new Vehicle
        {
            Name = request.Name,
            PlateNumber = request.PlateNumber,
            CreatedAt = DateTime.UtcNow
        };

        var created = await vehicleRepository.CreateAsync(vehicle, ct);
        logger.LogInformation("Vehicle created: {VehicleId} ({PlateNumber})", created.Id, created.PlateNumber);

        return Map(created);
    }

    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var vehicles = await vehicleRepository.GetAllAsync(ct);
        return vehicles.Select(Map).ToList();
    }

    private static VehicleDto Map(Vehicle v) => new(v.Id, v.Name, v.PlateNumber, v.CreatedAt);
}

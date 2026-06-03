using System.ComponentModel.DataAnnotations;

namespace CarTracking.Application.DTOs;

public sealed record VehicleDto(long Id, string Name, string PlateNumber, DateTime CreatedAt);

public sealed record CreateVehicleRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, MaxLength(20)]
    public string PlateNumber { get; init; } = string.Empty;
}

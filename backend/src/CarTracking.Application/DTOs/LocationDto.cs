using System.ComponentModel.DataAnnotations;

namespace CarTracking.Application.DTOs;

public sealed record LocationDto(
    long VehicleId,
    double Latitude,
    double Longitude,
    double Speed,
    DateTime RecordedAt);

public sealed record LocationUpdateRequest
{
    [Required]
    public long VehicleId { get; init; }

    [Required, Range(-90.0, 90.0)]
    public double Latitude { get; init; }

    [Required, Range(-180.0, 180.0)]
    public double Longitude { get; init; }

    [Required, Range(0.0, double.MaxValue)]
    public double Speed { get; init; }

    [Required]
    public DateTime RecordedAt { get; init; }
}

public sealed record LocationHistoryDto(
    long Id,
    long VehicleId,
    double Latitude,
    double Longitude,
    double Speed,
    DateTime RecordedAt);

public sealed record LocationHistoryQuery
{
    [Required]
    public long VehicleId { get; init; }

    [Required]
    public DateTime From { get; init; }

    [Required]
    public DateTime To { get; init; }
}

namespace CarTracking.Domain.Entities;

public sealed class Vehicle
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<LocationHistory> LocationHistory { get; set; } = [];
    public VehicleCurrentLocation? CurrentLocation { get; set; }
}

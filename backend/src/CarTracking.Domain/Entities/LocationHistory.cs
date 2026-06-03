namespace CarTracking.Domain.Entities;

public sealed class LocationHistory
{
    public long Id { get; set; }
    public long VehicleId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Speed { get; set; }
    public DateTime RecordedAt { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
}

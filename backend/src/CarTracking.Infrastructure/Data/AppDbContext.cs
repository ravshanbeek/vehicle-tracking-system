using CarTracking.Domain.Entities;
using CarTracking.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CarTracking.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<LocationHistory> LocationHistory => Set<LocationHistory>();
    public DbSet<VehicleCurrentLocation> VehicleCurrentLocations => Set<VehicleCurrentLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new LocationHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleCurrentLocationConfiguration());
    }
}

using CarTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarTracking.Infrastructure.Data.Configurations;

public sealed class VehicleCurrentLocationConfiguration : IEntityTypeConfiguration<VehicleCurrentLocation>
{
    public void Configure(EntityTypeBuilder<VehicleCurrentLocation> builder)
    {
        builder.ToTable("VehicleCurrentLocations");
        builder.HasKey(c => c.VehicleId);
        builder.Property(c => c.VehicleId).ValueGeneratedNever();
        builder.Property(c => c.Latitude).IsRequired();
        builder.Property(c => c.Longitude).IsRequired();
        builder.Property(c => c.Speed).IsRequired();
        builder.Property(c => c.RecordedAt).IsRequired();

        builder.HasOne(c => c.Vehicle)
               .WithOne(v => v.CurrentLocation)
               .HasForeignKey<VehicleCurrentLocation>(c => c.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

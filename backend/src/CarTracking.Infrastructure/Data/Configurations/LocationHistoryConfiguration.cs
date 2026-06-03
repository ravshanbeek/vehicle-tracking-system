using CarTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarTracking.Infrastructure.Data.Configurations;

public sealed class LocationHistoryConfiguration : IEntityTypeConfiguration<LocationHistory>
{
    public void Configure(EntityTypeBuilder<LocationHistory> builder)
    {
        builder.ToTable("LocationHistory");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).UseIdentityAlwaysColumn();
        builder.Property(l => l.Latitude).IsRequired();
        builder.Property(l => l.Longitude).IsRequired();
        builder.Property(l => l.Speed).IsRequired();
        builder.Property(l => l.RecordedAt).IsRequired();

        builder.HasOne(l => l.Vehicle)
               .WithMany(v => v.LocationHistory)
               .HasForeignKey(l => l.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);

        // Critical for range queries over time windows per vehicle
        builder.HasIndex(l => new { l.VehicleId, l.RecordedAt });
    }
}

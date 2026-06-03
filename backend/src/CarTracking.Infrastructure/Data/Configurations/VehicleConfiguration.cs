using CarTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarTracking.Infrastructure.Data.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).UseIdentityAlwaysColumn();
        builder.Property(v => v.Name).IsRequired().HasMaxLength(100);
        builder.Property(v => v.PlateNumber).IsRequired().HasMaxLength(20);
        builder.Property(v => v.CreatedAt).IsRequired();

        builder.HasIndex(v => v.PlateNumber).IsUnique();
    }
}

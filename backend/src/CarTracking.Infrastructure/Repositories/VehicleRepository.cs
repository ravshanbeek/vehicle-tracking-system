using CarTracking.Application.Interfaces;
using CarTracking.Domain.Entities;
using CarTracking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CarTracking.Infrastructure.Repositories;

public sealed class VehicleRepository(AppDbContext db) : IVehicleRepository
{
    public async Task<Vehicle> CreateAsync(Vehicle vehicle, CancellationToken ct = default)
    {
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(ct);
        return vehicle;
    }

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken ct = default)
        => await db.Vehicles.AsNoTracking().OrderBy(v => v.Id).ToListAsync(ct);

    public Task<bool> ExistsAsync(long vehicleId, CancellationToken ct = default)
        => db.Vehicles.AnyAsync(v => v.Id == vehicleId, ct);
}

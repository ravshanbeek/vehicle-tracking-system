using CarTracking.Application.DTOs;
using CarTracking.Application.Interfaces;
using CarTracking.Application.Services;
using CarTracking.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CarTracking.Tests;

public class VehicleServiceTests
{
    private readonly Mock<IVehicleRepository> _repo = new();
    private readonly VehicleService _sut;

    public VehicleServiceTests()
        => _sut = new VehicleService(_repo.Object, NullLogger<VehicleService>.Instance);

    [Fact]
    public async Task CreateAsync_persists_vehicle_and_returns_mapped_dto()
    {
        var request = new CreateVehicleRequest { Name = "Truck Alpha", PlateNumber = "01-123-AAA" };
        _repo.Setup(r => r.CreateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Vehicle v, CancellationToken _) => { v.Id = 42; return v; });

        var result = await _sut.CreateAsync(request);

        Assert.Equal(42, result.Id);
        Assert.Equal("Truck Alpha", result.Name);
        Assert.Equal("01-123-AAA", result.PlateNumber);
        _repo.Verify(r => r.CreateAsync(
            It.Is<Vehicle>(v => v.Name == "Truck Alpha" && v.PlateNumber == "01-123-AAA"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_stamps_CreatedAt_in_utc()
    {
        Vehicle? captured = null;
        _repo.Setup(r => r.CreateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Vehicle v, CancellationToken _) => { captured = v; return v; });

        var before = DateTime.UtcNow;
        await _sut.CreateAsync(new CreateVehicleRequest { Name = "X", PlateNumber = "Y" });
        var after = DateTime.UtcNow;

        Assert.NotNull(captured);
        Assert.InRange(captured!.CreatedAt, before, after);
    }

    [Fact]
    public async Task GetAllAsync_maps_every_vehicle()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Vehicle>
             {
                 new() { Id = 1, Name = "A", PlateNumber = "P1", CreatedAt = DateTime.UtcNow },
                 new() { Id = 2, Name = "B", PlateNumber = "P2", CreatedAt = DateTime.UtcNow },
             });

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            v => Assert.Equal(1, v.Id),
            v => Assert.Equal(2, v.Id));
    }

    [Fact]
    public async Task GetAllAsync_returns_empty_when_no_vehicles()
    {
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Vehicle>());

        var result = await _sut.GetAllAsync();

        Assert.Empty(result);
    }
}

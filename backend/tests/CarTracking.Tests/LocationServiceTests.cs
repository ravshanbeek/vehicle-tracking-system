using CarTracking.Application.DTOs;
using CarTracking.Application.Interfaces;
using CarTracking.Application.Services;
using CarTracking.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CarTracking.Tests;

public class LocationServiceTests
{
    private readonly Mock<IVehicleRepository> _vehicles = new();
    private readonly Mock<ILocationRepository> _locations = new();
    private readonly Mock<ILocationBroadcaster> _broadcaster = new();
    private readonly LocationService _sut;

    public LocationServiceTests()
    {
        _broadcaster.Setup(b => b.BroadcastAsync(It.IsAny<LocationDto>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
        _sut = new LocationService(_vehicles.Object, _locations.Object, _broadcaster.Object,
            NullLogger<LocationService>.Instance);
    }

    private static LocationUpdateRequest Request(long vehicleId = 1) => new()
    {
        VehicleId = vehicleId,
        Latitude = 41.0082,
        Longitude = 69.2784,
        Speed = 65.5,
        RecordedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task UpdateLocationAsync_throws_when_vehicle_missing()
    {
        _vehicles.Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateLocationAsync(Request(99)));
    }

    [Fact]
    public async Task UpdateLocationAsync_does_not_write_when_vehicle_missing()
    {
        _vehicles.Setup(r => r.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateLocationAsync(Request()));

        _locations.Verify(l => l.AddHistoryAsync(It.IsAny<LocationHistory>(), It.IsAny<CancellationToken>()), Times.Never);
        _locations.Verify(l => l.UpsertCurrentLocationAsync(It.IsAny<VehicleCurrentLocation>(), It.IsAny<CancellationToken>()), Times.Never);
        _broadcaster.Verify(b => b.BroadcastAsync(It.IsAny<LocationDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateLocationAsync_writes_history_and_current_when_vehicle_exists()
    {
        _vehicles.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _sut.UpdateLocationAsync(Request());

        _locations.Verify(l => l.AddHistoryAsync(
            It.Is<LocationHistory>(h => h.VehicleId == 1 && h.Speed == 65.5),
            It.IsAny<CancellationToken>()), Times.Once);
        _locations.Verify(l => l.UpsertCurrentLocationAsync(
            It.Is<VehicleCurrentLocation>(c => c.VehicleId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLocationAsync_broadcasts_update_when_vehicle_exists()
    {
        _vehicles.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _sut.UpdateLocationAsync(Request());

        _broadcaster.Verify(b => b.BroadcastAsync(
            It.Is<LocationDto>(d => d.VehicleId == 1 && d.Latitude == 41.0082),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentLocationAsync_delegates_to_repository()
    {
        var expected = new LocationDto(1, 41.0, 69.0, 50, DateTime.UtcNow);
        _locations.Setup(l => l.GetCurrentLocationAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetCurrentLocationAsync(1);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetHistoryAsync_delegates_to_repository_with_query_range()
    {
        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow;
        var rows = new List<LocationHistoryDto> { new(1, 1, 41.0, 69.0, 50, DateTime.UtcNow) };
        _locations.Setup(l => l.GetHistoryAsync(1, from, to, It.IsAny<CancellationToken>())).ReturnsAsync(rows);

        var result = await _sut.GetHistoryAsync(new LocationHistoryQuery { VehicleId = 1, From = from, To = to });

        Assert.Single(result);
        _locations.Verify(l => l.GetHistoryAsync(1, from, to, It.IsAny<CancellationToken>()), Times.Once);
    }
}

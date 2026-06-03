using CarTracking.Application.DTOs;
using CarTracking.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CarTracking.API.Hubs;

/// <summary>
/// Implements ILocationBroadcaster using SignalR.
/// Sends to both the per-vehicle group and the global "all-vehicles" group.
/// Lives in API layer — Application layer stays infrastructure-free.
/// </summary>
public sealed class VehicleLocationBroadcaster(IHubContext<VehicleHub> hubContext) : ILocationBroadcaster
{
    private const string EventName = "LocationUpdated";

    public async Task BroadcastAsync(LocationDto location, CancellationToken ct = default)
    {
        var vehicleGroup = $"vehicle-{location.VehicleId}";

        await Task.WhenAll(
            hubContext.Clients.Group(vehicleGroup).SendAsync(EventName, location, ct),
            hubContext.Clients.Group("all-vehicles").SendAsync(EventName, location, ct));
    }
}

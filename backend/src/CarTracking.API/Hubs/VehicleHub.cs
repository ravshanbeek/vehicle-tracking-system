using Microsoft.AspNetCore.SignalR;

namespace CarTracking.API.Hubs;

/// <summary>
/// Clients call JoinVehicleGroup/LeaveVehicleGroup to subscribe to per-vehicle updates.
/// The server pushes "LocationUpdated" events to group "vehicle-{id}" and the global "all-vehicles" group.
/// </summary>
public sealed class VehicleHub : Hub
{
    public async Task JoinVehicleGroup(long vehicleId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(vehicleId));

    public async Task LeaveVehicleGroup(long vehicleId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(vehicleId));

    private static string GroupName(long vehicleId) => $"vehicle-{vehicleId}";
}

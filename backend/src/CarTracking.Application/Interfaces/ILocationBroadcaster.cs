using CarTracking.Application.DTOs;

namespace CarTracking.Application.Interfaces;

// Abstraction over SignalR — keeps Application layer free of infrastructure concerns.
public interface ILocationBroadcaster
{
    Task BroadcastAsync(LocationDto location, CancellationToken ct = default);
}

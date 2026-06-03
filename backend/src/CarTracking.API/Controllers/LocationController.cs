using CarTracking.Application.DTOs;
using CarTracking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarTracking.API.Controllers;

[ApiController]
[Route("api/location")]
[Produces("application/json")]
public sealed class LocationController(ILocationService locationService) : ControllerBase
{
    [HttpPost("update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromBody] LocationUpdateRequest request, CancellationToken ct)
    {
        await locationService.UpdateLocationAsync(request, ct);
        return NoContent();
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent([FromQuery] long vehicleId, CancellationToken ct)
    {
        var location = await locationService.GetCurrentLocationAsync(vehicleId, ct);
        return location is null ? NotFound() : Ok(location);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<LocationHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory([FromQuery] LocationHistoryQuery query, CancellationToken ct)
    {
        var history = await locationService.GetHistoryAsync(query, ct);
        return Ok(history);
    }
}

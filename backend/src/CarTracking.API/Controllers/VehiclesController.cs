using CarTracking.Application.DTOs;
using CarTracking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarTracking.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Produces("application/json")]
public sealed class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await vehicleService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { }, vehicle);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var vehicles = await vehicleService.GetAllAsync(ct);
        return Ok(vehicles);
    }
}

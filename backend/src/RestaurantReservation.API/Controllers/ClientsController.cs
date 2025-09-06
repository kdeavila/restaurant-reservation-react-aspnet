using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    private readonly IClientService _clientService = clientService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _clientService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _clientService.GetByIdAsync(id, ct);
        return result.IsFailure ? NotFound(result.Error) : Ok(result.Value);
    }
    
    // [HttpPost]
    // public async Task<IActionResult> Create([FromBody] CreateClientDto dto, CancellationToken ct = default)
    // {
    //     if (!ModelState.IsValid) return BadRequest(ModelState);
    //
    //     var result = await _clientService.CreateClientAsync(dto, ct);
    //     return result.IsFailure
    //         ? Conflict(result.Error)
    //         : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    // }
}
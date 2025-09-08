using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Infrastructure;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/tables")]
public class TablesController(ITableService tableService) : ControllerBase
{
    private readonly ITableService _tableService = tableService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _tableService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _tableService.GetByIdAsync(id, ct);
        if (result.IsFailure) return StatusCode(result.StatusCode, result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTableDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _tableService.CreateTableAsync(dto, ct);
        if (result.IsFailure) return StatusCode(result.StatusCode, result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTableDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id) return BadRequest("ID in URL does not match ID in body.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _tableService.UpdateTableAsync(dto, ct);
        if (result.IsFailure) return StatusCode(result.StatusCode, result.Error);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _tableService.DeleteTableAsync(id, ct);
        if (result.IsFailure) return StatusCode(result.StatusCode, result.Error);

        return NoContent();
    }
}
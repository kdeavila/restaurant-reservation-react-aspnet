using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.TableType;
using RestaurantReservation.Application.Interfaces.Services;

namespace RestaurantReservation.API.Controllers;

[ApiController]
[Route("api/table-types")]
public class TableTypesController(ITableTypeService tableTypeService) : ControllerBase
{
    private readonly ITableTypeService _tableTypeService = tableTypeService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await _tableTypeService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _tableTypeService.GetByIdAsync(id, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTableTypeDto dto, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _tableTypeService.CreateAsync(dto, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTableTypeDto dto, CancellationToken ct = default)
    {
        if (id != dto.Id) return BadRequest("ID in URL does not match ID in body.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _tableTypeService.UpdateAsync(dto, ct);
        return result.IsFailure
            ? StatusCode(result.StatusCode, new { error = result.Error })
            : NoContent();
    }
}
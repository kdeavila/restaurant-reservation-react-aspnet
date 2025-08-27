using RestaurantReservation.Application.Common.Helpers;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class TableService(ITableRepository tableRepository, ITableTypeRepository tableTypeRepository) : ITableService
{
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;

    public async Task<TableDto?> CreateTableAsync(CreateTableDto dto, CancellationToken ct = default)
    {
        var tableType = await _tableTypeRepository.GetByIdAsync(dto.TableTypeId, ct);
        if (tableType is null) return null;

        // Generate the code for the tables, including the table type and adding the name and code. e.g: VIP01;
        var existingTables = await _tableRepository.GetByTableTypeIdAsync(dto.TableTypeId, ct);
        var code = TableCodeGenerator.Generate(tableType.Name, existingTables.Count());
        
        var table = new Table()
        {
            Capacity = dto.Capacity,
            Location = dto.Location,
            TableTypeId = dto.TableTypeId,
            Status = TableStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _tableRepository.AddAsync(table, ct);
        return new TableDto(table.Id, table.Code, table.Capacity, table.Location, table.TableTypeId,
            table.Status.ToString());
    }

    public async Task<TableDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(id, ct);
        if (table is null) return null;

        return new TableDto(table.Id, table.Code, table.Capacity, table.Location, table.TableTypeId,
            table.Status.ToString());
    }

    public async Task<IEnumerable<TableDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tables = await _tableRepository.GetAllAsync(ct);
        return tables.Select(t =>
            new TableDto(t.Id, t.Code, t.Capacity, t.Location, t.TableTypeId, t.Status.ToString())
        );
    }

    public async Task<bool> UpdateTableAsync(UpdateTableDto dto, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(dto.Id, ct);
        if (table is null) return false;
        
        table.Capacity = dto.Capacity ?? table.Capacity;
        table.Location = dto.Location ?? table.Location;
        table.TableTypeId = dto.TableTypeId ?? table.TableTypeId;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<TableStatus>(dto.Status, true, out var parsed))
        {
            table.Status = parsed;
        }

        await _tableRepository.UpdateAsync(table, ct);
        return true;
    }

    public async Task<bool> DeleteTableAsync(int id, CancellationToken ct = default)
    {
        await _tableRepository.DeleteAsync(id, ct);
        return true;
    }
}
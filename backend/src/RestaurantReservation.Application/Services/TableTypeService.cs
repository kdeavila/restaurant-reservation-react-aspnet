using RestaurantReservation.Application.DTOs.TableType;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Services;

public class TableTypeService(ITableTypeRepository tableTypeRepository) : ITableTypeService
{
    private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;

    public async Task<TableTypeDto?> CreateAsync(CreateTableTypeDto dto, CancellationToken ct = default)
    {
        var tableExists = await _tableTypeRepository.ExistsByNameAsync(dto.Name, ct);
        if (tableExists) return null;

        var tableType = new TableType()
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            BasePricePerHour = dto.BasePricePerHour,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _tableTypeRepository.AddAsync(tableType, ct);
        return new TableTypeDto(
            tableType.Id, tableType.Name, tableType.Description,
            tableType.BasePricePerHour, tableType.IsActive
        );
    }

    public async Task<TableTypeDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var tableType = await _tableTypeRepository.GetByIdAsync(id, ct);
        if (tableType is null) return null;

        return new TableTypeDto(
            tableType.Id, tableType.Name, tableType.Description,
            tableType.BasePricePerHour, tableType.IsActive
        );
    }

    public async Task<IEnumerable<TableTypeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tableTypes = await _tableTypeRepository.GetAllAsync(ct);
        return tableTypes.Select(tt => new TableTypeDto(tt.Id, tt.Name, tt.Description, tt.BasePricePerHour, tt.IsActive
        ));
    }

    public async Task<bool> UpdateAsync(UpdateTableTypeDto dto, CancellationToken ct = default)
    {
        var tableType = await _tableTypeRepository.GetByIdAsync(dto.Id, ct);
        if (tableType is null) return false;

        tableType.Name = dto.Name ?? tableType.Name;
        tableType.Description = dto.Description ?? tableType.Description;
        tableType.BasePricePerHour = dto.BasePricePerHour ?? tableType.BasePricePerHour;

        await _tableTypeRepository.UpdateAsync(tableType, ct);
        return true;
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var tableType = await _tableTypeRepository.GetByIdAsync(id, ct);
        if (tableType is null) return false;

        tableType.IsActive = false;
        await _tableTypeRepository.UpdateAsync(tableType, ct);
        return true;
    }
}
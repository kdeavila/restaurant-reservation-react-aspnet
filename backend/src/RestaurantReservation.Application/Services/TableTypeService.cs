using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.TableType;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Services;

public class TableTypeService(ITableTypeRepository tableTypeRepository) : ITableTypeService
{
    private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;

    public async Task<Result<TableTypeDto>> CreateAsync(CreateTableTypeDto dto, CancellationToken ct = default)
    {
        var tableExists = await _tableTypeRepository.ExistsByNameAsync(dto.Name, ct);
        if (tableExists) return Result.Failure<TableTypeDto>("Table type with the same name already exists.", 404);

        var tableType = new TableType()
        {
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            BasePricePerHour = dto.BasePricePerHour,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _tableTypeRepository.AddAsync(tableType, ct);
        var tableTypeDto = new TableTypeDto(
            tableType.Id, tableType.Name, tableType.Description,
            tableType.BasePricePerHour, tableType.IsActive
        );
        return Result.Success(tableTypeDto);
    }

    public async Task<Result<TableTypeDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var tableType = await _tableTypeRepository.GetByIdAsync(id, ct);
        if (tableType is null) return Result.Failure<TableTypeDto>("Table type not found.", 404);

        var tableTypeDto = new TableTypeDto(
            tableType.Id, tableType.Name, tableType.Description,
            tableType.BasePricePerHour, tableType.IsActive
        );
        return Result.Success(tableTypeDto);
    }

    public async Task<IEnumerable<TableTypeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tableTypes = await _tableTypeRepository.GetAllAsync(ct);
        return tableTypes.Select(tt => new TableTypeDto(tt.Id, tt.Name, tt.Description, tt.BasePricePerHour, tt.IsActive
        ));
    }

    public async Task<Result> UpdateAsync(UpdateTableTypeDto dto, CancellationToken ct = default)
    {
        var tableType = await _tableTypeRepository.GetByIdAsync(dto.Id, ct);
        if (tableType is null) return Result.Failure("Table type not found.", 404);

        if (!string.IsNullOrEmpty(dto.Name) && dto.Name != tableType.Name)
        {
            var nameExists = await _tableTypeRepository.ExistsByNameAsync(dto.Name, ct);
            if (nameExists) return Result.Failure("Table type with the same name already exists.", 409);

            tableType.Name = dto.Name;
        }

        tableType.Description = dto.Description ?? tableType.Description;
        tableType.BasePricePerHour = dto.BasePricePerHour ?? tableType.BasePricePerHour;

        await _tableTypeRepository.UpdateAsync(tableType, ct);
        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var tableType = await _tableTypeRepository.GetByIdAsync(id, ct);
        if (tableType is null) return Result.Failure("Table type not found.", 404);

        tableType.IsActive = false;
        await _tableTypeRepository.UpdateAsync(tableType, ct);
        return Result.Success();
    }
}
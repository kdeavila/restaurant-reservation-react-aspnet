using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Helpers;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.TableType;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class TableService(
    ITableRepository tableRepository,
    ITableTypeRepository tableTypeRepository
) : ITableService
{
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;

    public async Task<Result<TableDetailedDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(id, ct);
        if (table is null)
            return Result.Failure<TableDetailedDto>("Table not found", 404);

        var tableDto = table.Adapt<TableDetailedDto>();
        return Result.Success(tableDto);
    }

    public async Task<(
        IEnumerable<TableDetailedDto> Data,
        PaginationMetadata pagination
    )> GetAllAsync([FromQuery] TableQueryParams queryParams, CancellationToken ct = default)
    {
        var query = _tableRepository.Query();

        if (!string.IsNullOrWhiteSpace(queryParams.Code))
        {
            var term = queryParams.Code.Trim().ToLower();
            query = query.Where(t => t.Code.ToLower().Contains(term));
        }

        if (queryParams.Capacity.HasValue)
            query = query.Where(t => t.Capacity >= queryParams.Capacity);

        if (!string.IsNullOrWhiteSpace(queryParams.Location))
        {
            var term = queryParams.Location.Trim().ToLower();
            query = query.Where(t => t.Location.ToLower().Contains(term));
        }

        if (
            !string.IsNullOrWhiteSpace(queryParams.Status)
            && Enum.TryParse<TableStatus>(queryParams.Status, true, out var parsedStatus)
        )
        {
            query = query.Where(t => t.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(ct);

        var skipNumber = (queryParams.Page - 1) * queryParams.PageSize;
        var data = await query
            .OrderBy(table => table.Id)
            .Skip(skipNumber)
            .Take(queryParams.PageSize)
            .Select(table => table.Adapt<TableDetailedDto>())
            .ToListAsync(ct);

        var pagination = new PaginationMetadata
        {
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize),
        };

        return (data, pagination);
    }

    public async Task<Result<TableDetailedDto>> CreateAsync(
        CreateTableDto dto,
        CancellationToken ct = default
    )
    {
        var tableType = await _tableTypeRepository.GetByIdAsync(dto.TableTypeId, ct);
        if (tableType is null)
            return Result.Failure<TableDetailedDto>("Table type not found", 404);
        if (!tableType.IsActive)
            return Result.Failure<TableDetailedDto>("Table type is inactive", 400);

        // generate the code for the tables, including the table type and adding the name and code. e.g., VIP01;
        var existingTables = await _tableRepository.GetByTableTypeIdAsync(dto.TableTypeId, ct);
        var code = TableCodeGenerator.Generate(tableType.Name, existingTables.Count());

        var table = new Table()
        {
            Code = code,
            Capacity = dto.Capacity,
            Location = dto.Location,
            TableTypeId = dto.TableTypeId,
            Status = TableStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };

        await _tableRepository.AddAsync(table, ct);
        var tableDto = new TableDetailedDto(
            table.Id,
            table.Code,
            table.Capacity,
            table.Location,
            table.Status.ToString(),
            new TableTypeSimpleDto(
                table.TableTypeId,
                table.TableType.Name,
                table.TableType.BasePricePerHour,
                table.TableType.IsActive
            )
        );
        return Result.Success(tableDto);
    }

    public async Task<Result> UpdateAsync(UpdateTableDto dto, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(dto.Id, ct);
        if (table is null)
            return Result.Failure("Table not found", 404);

        if (dto.TableTypeId.HasValue && dto.TableTypeId.Value != table.TableTypeId)
        {
            return Result.Failure(
                "Cannot change table type once created. Please create a new table instead.",
                400
            );
        }

        table.Capacity = dto.Capacity ?? table.Capacity;
        table.Location = dto.Location ?? table.Location;

        if (
            !string.IsNullOrEmpty(dto.Status)
            && Enum.TryParse<TableStatus>(dto.Status, true, out var parsed)
        )
            table.Status = parsed;

        await _tableRepository.UpdateAsync(table, ct);
        return Result.Success();
    }

    public async Task<Result<string>> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var table = await _tableRepository.GetByIdAsync(id, ct);
        if (table is null)
            return Result.Failure<string>("Table not found", 404);

        if (table.Status == TableStatus.Inactive)
            return Result.Success("Table is already inactive");

        var futureReservations = table
            .Reservations.Where(r =>
                r.Status != ReservationStatus.Cancelled
                && r.Status != ReservationStatus.Completed
                && (
                    r.Date > DateTime.UtcNow.Date
                    || (r.Date == DateTime.UtcNow.Date && r.StartTime > DateTime.UtcNow.TimeOfDay)
                )
            )
            .ToList();

        if (futureReservations.Any())
            return Result.Failure<string>(
                $"Cannot delete table. It has {futureReservations.Count} future reservation(s). "
                    + "Please cancel or reassign the reservations first.",
                409
            );

        table.Status = TableStatus.Inactive;
        await _tableRepository.UpdateAsync(table, ct);
        return Result.Success("Table deactivated successfully.");
    }
}

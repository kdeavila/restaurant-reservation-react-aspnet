using Mapster;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.TableType;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Services;

public class TableTypeService(
    ITableTypeRepository tableTypeRepository,
    ITableRepository tableRepository) : ITableTypeService
{
   private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;
   private readonly ITableRepository _tableRepository = tableRepository;
   private static TableTypeDto MapToDto(TableType tableType, int tableCount = 0)
   {
      return new(
         tableType.Id,
         tableType.Name,
         tableType.Description,
         tableType.BasePricePerHour,
         tableType.IsActive,
         tableCount,
         tableType.CreatedAt
      );
   }

   public async Task<Result<TableTypeDto>> GetByIdAsync(int id, CancellationToken ct = default)
   {
      var tableType = await _tableTypeRepository.GetByIdAsync(id, ct);
      if (tableType is null)
         return Result.Failure<TableTypeDto>("Table type not found.", 404);

      var tables = await _tableRepository.GetByTableTypeIdAsync(id, ct);
      var tableCount = tables.Count();

      var tableTypeDto = MapToDto(tableType, tableCount);
      return Result.Success(tableTypeDto);
   }

   public async Task<(IEnumerable<TableTypeDto> Data, PaginationMetadata Pagination)> GetAllAsync(
       TableTypeQueryParams queryParams, CancellationToken ct = default)
   {
      var query = _tableTypeRepository.Query();

      if (!string.IsNullOrWhiteSpace(queryParams.Name))
      {
         var term = queryParams.Name.Trim().ToLower();
         query = query.Where(tt => tt.Name.ToLower().Contains(term));
      }

      if (queryParams.BasePrice.HasValue)
         query = query.Where(tt => tt.BasePricePerHour >= queryParams.BasePrice);

      var totalCount = await query.CountAsync(ct);
      var skipNumber = (queryParams.Page - 1) * queryParams.PageSize;

      var tableTypesPage = await query
          .OrderBy(tt => tt.Id)
          .Skip(skipNumber)
          .Take(queryParams.PageSize)
          .ToListAsync(ct);

      var pageIds = tableTypesPage.Select(tt => tt.Id).ToList();
      var tableCounts = await _tableRepository.GetTableCountsByTableTypeIdsAsync(pageIds, ct);

      var data = tableTypesPage.Select(tableType => MapToDto(tableType, tableCounts.GetValueOrDefault(tableType.Id, 0)));

      var pagination = new PaginationMetadata
      {
         Page = queryParams.Page,
         PageSize = queryParams.PageSize,
         TotalCount = totalCount,
         TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize)
      };

      return (data, pagination);
   }

   public async Task<Result<TableTypeDto>> CreateAsync(CreateTableTypeDto dto, CancellationToken ct = default)
   {
      var tableExists = await _tableTypeRepository.ExistsByNameAsync(dto.Name, ct);
      if (tableExists)
         return Result.Failure<TableTypeDto>(
             "Table type with the same name already exists.", 409);

      var tableType = new TableType()
      {
         Name = dto.Name,
         Description = dto.Description ?? string.Empty,
         BasePricePerHour = dto.BasePricePerHour,
         IsActive = true,
         CreatedAt = DateTime.UtcNow
      };

      await _tableTypeRepository.AddAsync(tableType, ct);
      var tableTypeDto = MapToDto(tableType, 0);
      
      return Result.Success(tableTypeDto);
   }

   public async Task<Result> UpdateAsync(UpdateTableTypeDto dto, CancellationToken ct = default)
   {
      var tableType = await _tableTypeRepository.GetByIdAsync(dto.Id, ct);
      if (tableType is null) return Result.Failure("Table type not found.", 404);

      if (!string.IsNullOrEmpty(dto.Name) && dto.Name != tableType.Name)
      {
         var nameExists = await _tableTypeRepository.ExistsByNameAsync(dto.Name, ct);
         if (nameExists)
            return Result.Failure("Table type with the same name already exists.", 409);

         tableType.Name = dto.Name;
      }

      tableType.Description = dto.Description ?? tableType.Description;
      tableType.BasePricePerHour = dto.BasePricePerHour ?? tableType.BasePricePerHour;

      await _tableTypeRepository.UpdateAsync(tableType, ct);
      return Result.Success();
   }

   public async Task<Result<string>> DeactivateAsync(int id, CancellationToken ct = default)
   {
      var tableType = await _tableTypeRepository.GetByIdAsync(id, ct);
      if (tableType is null)
         return Result.Failure<string>("Table type not found.", 404);

      var tablesUsingThisType = await _tableRepository.GetByTableTypeIdAsync(id, ct);
      var tables = tablesUsingThisType.ToList();

      if (tables.Any())
      {
         tableType.IsActive = false;
         await _tableTypeRepository.UpdateAsync(tableType, ct);
         return Result.Success($"Table type deactivated. It's being used by {tables.Count} tables.");
      }

      await _tableTypeRepository.DeleteAsync(id, ct);
      return Result.Success("Table type permanently deleted.");
   }
}
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface ITableTypeRepository
{
    Task<TableType?> GetByIdAsync(int id);
    Task<IReadOnlyList<TableType>> GetAllActiveAsync();
}
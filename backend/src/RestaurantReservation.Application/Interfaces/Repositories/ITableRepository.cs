using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface ITableRepository
{
    Task<Table?> GetByIdAsync(int id);
    Task<Table?> GetByCodeAsync(string code);
    Task<IReadOnlyList<Table>> GetByTypeAsync(int tableTypeId);

    Task<IReadOnlyList<Table>> FindAvailableAsync(
        DateTime date, TimeSpan start, TimeSpan end, int minCapacity);
}
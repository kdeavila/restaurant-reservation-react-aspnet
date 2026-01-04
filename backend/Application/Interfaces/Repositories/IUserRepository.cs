using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Repositories;

public interface IUserRepository
{
    IQueryable<ApplicationUser> Query();
    Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<ApplicationUser>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ApplicationUser ApplicationUser, CancellationToken ct = default);
    Task UpdateAsync(ApplicationUser ApplicationUser, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}

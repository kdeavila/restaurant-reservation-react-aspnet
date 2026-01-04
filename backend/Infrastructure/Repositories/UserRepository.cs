using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class UserRepository(RestaurantReservationDbContext context) : IUserRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public IQueryable<ApplicationUser> Query()
    {
        return _context.Users;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await _context.Users.FindAsync([id], ct);

    public async Task<ApplicationUser?> GetByEmailAsync(
        string email,
        CancellationToken ct = default
    ) => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Users.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(ApplicationUser ApplicationUser, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(ApplicationUser, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ApplicationUser ApplicationUser, CancellationToken ct = default)
    {
        _context.Users.Update(ApplicationUser);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var ApplicationUser = await _context.Users.FindAsync([id], ct);
        if (ApplicationUser is null)
            return;

        _context.Users.Remove(ApplicationUser);
        await _context.SaveChangesAsync(ct);
    }
}

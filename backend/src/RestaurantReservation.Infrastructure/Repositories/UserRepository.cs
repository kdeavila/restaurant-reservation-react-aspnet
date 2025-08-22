using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;

namespace RestaurantReservation.Infrastructure.Repositories;

public class UserRepository(RestaurantReservationDbContext context) : IUserRepository
{
    private readonly RestaurantReservationDbContext _context = context;

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.Users.FindAsync([id], ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        => await _context.Users
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await _context.Users.FindAsync([id], ct);
        if (user is null) return;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);
    }
}
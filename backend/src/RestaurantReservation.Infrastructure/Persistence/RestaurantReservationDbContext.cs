using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Infrastructure.Persistence;

public class RestaurantReservationDbContext(DbContextOptions<RestaurantReservationDbContext> options)
    : DbContext(options)
{
    // DbSet for each Entity
    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<TableType> TableTypes { get; set; }
    public DbSet<PricingRule> PricingRules { get; set; }
    public DbSet<PricingRuleDays> PricingRuleDays { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RestaurantReservationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
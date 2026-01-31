using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantReservation.Infrastructure.Persistence;
using RestaurantReservation.Infrastructure.Persistence.Seeding;
using Xunit;

namespace RestaurantReservation.API.Tests.Fixtures;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgresTestContainer _postgres = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var overrideSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.ConnectionString,
                ["Jwt:Key"] = "pn2Z6xOEycMJ4Phi1tMCKr3k743MBOOX",
                ["Jwt:Issuer"] = "RestaurantReservationAPI",
                ["Jwt:Audience"] = "RestaurantReservationClient",
            };

            configBuilder.AddInMemoryCollection(overrideSettings!);
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<RestaurantReservationDbContext>)
            );
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<RestaurantReservationDbContext>(options =>
            {
                options.UseNpgsql(_postgres.ConnectionString, x => x.MigrationsAssembly("Infrastructure"));
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();
        
        // Migrate and seed after container is ready
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
        await db.Database.MigrateAsync();
        await DataSeeder.SeedAsync(db);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
        
        // Delete all data and re-seed
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"AspNetUserRoles\" RESTART IDENTITY CASCADE");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"AspNetUserClaims\" RESTART IDENTITY CASCADE");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"AspNetUsers\" RESTART IDENTITY CASCADE");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PricingRuleDays\" RESTART IDENTITY CASCADE");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PricingRules\" RESTART IDENTITY CASCADE");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Reservations\" RESTART IDENTITY CASCADE");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Clients\" RESTART IDENTITY CASCADE");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Tables\" RESTART IDENTITY CASCADE");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"TableTypes\" RESTART IDENTITY CASCADE");
        
        await DataSeeder.SeedAsync(db);
    }
}

using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Infrastructure.Persistence.Seeding;

public class DataSeeder
{
    public static async Task SeedAsync(RestaurantReservationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        TableType? vipType = null;

        if (!context.TableTypes.Any())
        {
            var normal = new TableType
            {
                Name = "Normal",
                Description = "Standard dining table",
                BasePricePerHour = 20,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var vip = new TableType
            {
                Name = "VIP",
                Description = "Premium dining table",
                BasePricePerHour = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.TableTypes.AddRange(normal, vip);
            await context.SaveChangesAsync();

            vipType = vip;
        }
        else
        {
            vipType = await context.TableTypes.FirstOrDefaultAsync(t => t.Name == "VIP");
        }

        if (!context.PricingRules.Any() && vipType != null)
        {
            var rule = new PricingRule
            {
                RuleName = "Saturday Night Surcharge",
                RuleType = "Weekend Surcharge",
                StartTime = new TimeSpan(18, 0, 0),
                EndTime = new TimeSpan(23, 59, 59),
                SurchargePercentage = 20m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(5),
                TableTypeId = vipType.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.PricingRules.Add(rule);
            await context.SaveChangesAsync();

            context.PricingRuleDays.Add(new PricingRuleDays
            {
                PricingRuleId = rule.Id,
                DayOfWeek = DaysOfWeek.Saturday
            });

            await context.SaveChangesAsync();
        }
    }
}
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
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!context.TableTypes.Any())
        {
            context.TableTypes.AddRange(
                new TableType
                {
                    Name = "Normal", Description = "Standard dining table", BasePricePerHour = 20, IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TableType
                {
                    Name = "VIP", Description = "Premium dining table", BasePricePerHour = 50, IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }

        if (!context.PricingRules.Any())
        {
            var vipType = context.TableTypes.FirstOrDefault(t => t.Name == "VIP");

            var rule = new PricingRule
            {
                RuleName = "Saturday Night Surcharge",
                RuleType = "Weekend Surcharge",
                StartTime = new DateTime(1, 1, 1, 18, 0, 0),
                EndTime = new DateTime(1, 1, 1, 23, 59, 59),
                SurchargePercentage = 0.20m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(5),
                TableTypeId = vipType?.Id ?? 1,
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
        }

        await context.SaveChangesAsync();
    }
}
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Infrastructure.Persistence.Seeding;

public static class DataSeeder
{
   public static async Task SeedAsync(RestaurantReservationDbContext context)
   {
      await context.Database.MigrateAsync();

        var adminUserId = await EnsureAdminAsync(context);

      // Table types
      TableType vipTableType = null!;
      TableType standardTableType = null!;

      if (!context.TableTypes.Any())
      {
         vipTableType = new TableType
         {
            Name = "VIP",
            Description = "Premium dining table with exclusive services",
            BasePricePerHour = 50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
         };

         standardTableType = new TableType
         {
            Name = "Standard",
            Description = "Regular dining table",
            BasePricePerHour = 25m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
         };

         context.TableTypes.AddRange(vipTableType, standardTableType);
         await context.SaveChangesAsync();
      }
      else
      {
         vipTableType = await context.TableTypes.FirstAsync(t => t.Name == "VIP");
         standardTableType = await context.TableTypes.FirstAsync(t => t.Name == "Standard");
      }

      // Tables
      if (!context.Tables.Any())
      {
         var tables = new List<Table>
            {
                new Table
                {
                    Code = "VIP01", Capacity = 6, Location = "Main hall", TableTypeId = vipTableType.Id,
                    Status = TableStatus.Active, CreatedAt = DateTime.UtcNow
                },
                new Table
                {
                    Code = "VIP02", Capacity = 4, Location = "Terrace", TableTypeId = vipTableType.Id,
                    Status = TableStatus.Active, CreatedAt = DateTime.UtcNow
                },
                new Table
                {
                    Code = "STD01", Capacity = 4, Location = "Main hall", TableTypeId = standardTableType.Id,
                    Status = TableStatus.Active, CreatedAt = DateTime.UtcNow
                },
                new Table
                {
                    Code = "STD02", Capacity = 2, Location = "Terrace", TableTypeId = standardTableType.Id,
                    Status = TableStatus.Active, CreatedAt = DateTime.UtcNow
                },
                new Table
                {
                    Code = "STD03", Capacity = 8, Location = "Private room", TableTypeId = standardTableType.Id,
                    Status = TableStatus.Active, CreatedAt = DateTime.UtcNow
                }
            };

         context.Tables.AddRange(tables);
         await context.SaveChangesAsync();
      }

      // Clients
      List<Client> clients = new();
      if (!context.Clients.Any())
      {
         clients = new List<Client>
            {
                new Client
                {
                    FirstName = "Keyner", LastName = "De Ávila", Email = "kda.ts@gmail.com", Phone = "3022851699",
                    Status = ClientStatus.Active, CreatedAt = DateTime.UtcNow
                },
                new Client
                {
                    FirstName = "John", LastName = "Doe", Email = "johndoe_12@yahoo.com", Phone = "4155557285",
                    Status = ClientStatus.Active, CreatedAt = DateTime.UtcNow
                },
                new Client
                {
                    FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@hotmail.com", Phone = "3055551234",
                    Status = ClientStatus.Active, CreatedAt = DateTime.UtcNow
                },
                new Client
                {
                    FirstName = "Carlos", LastName = "Rodriguez", Email = "c.rodriguez@gmail.com", Phone = "7865559876",
                    Status = ClientStatus.Active, CreatedAt = DateTime.UtcNow
                }
            };

         context.Clients.AddRange(clients);
         await context.SaveChangesAsync();
      }
      else
      {
         clients = await context.Clients.ToListAsync();
      }

      // Pricing rules
      if (!context.PricingRules.Any())
      {
         var startDate = DateTime.UtcNow;
         var endDate = DateTime.UtcNow.AddMonths(6);

         var pricingRules = new List<PricingRule>
            {
                new PricingRule
                {
                    RuleName = "Midday VIP Surcharge",
                    RuleType = "Peak Hour",
                    StartTime = new TimeSpan(12, 0, 0),
                    EndTime = new TimeSpan(14, 0, 0),
                    SurchargePercentage = 20m,
                    StartDate = startDate,
                    EndDate = endDate,
                    TableTypeId = vipTableType.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new PricingRule
                {
                    RuleName = "Weekend Surcharge",
                    RuleType = "Weekend",
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0),
                    SurchargePercentage = 30m,
                    StartDate = startDate,
                    EndDate = endDate,
                    TableTypeId = vipTableType.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new PricingRule
                {
                    RuleName = "Early Bird Discount",
                    RuleType = "Discount",
                    StartTime = new TimeSpan(17, 0, 0),
                    EndTime = new TimeSpan(18, 30, 0),
                    SurchargePercentage = -15m,
                    StartDate = startDate,
                    EndDate = endDate,
                    TableTypeId = standardTableType.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

         context.PricingRules.AddRange(pricingRules);
         await context.SaveChangesAsync();

         // Add days for pricing rules
         var pricingRuleDays = new List<PricingRuleDays>();
         foreach (var rule in pricingRules)
         {
            if (rule.RuleName == "Weekend Surcharge")
            {
               pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Friday });
               pricingRuleDays.Add(
                   new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Saturday });
            }
            else
            {
               for (int i = 1; i <= 5; i++)
               {
                  pricingRuleDays.Add(new PricingRuleDays
                  {
                     PricingRuleId = rule.Id,
                     DayOfWeek = (DaysOfWeek)i
                  });
               }
            }
         }

         context.PricingRuleDays.AddRange(pricingRuleDays);
         await context.SaveChangesAsync();
      }

      // Reservations
      if (!context.Reservations.Any())
      {
         var tables = await context.Tables.ToListAsync();
         var client = clients.First();

         var reservations = new List<Reservation>
            {
                new Reservation
                {
                    ClientId = client.Id,
                    TableId = tables[0].Id,
                    Date = DateTime.UtcNow.Date.AddDays(1),
                    StartTime = new TimeSpan(12, 0, 0),
                    EndTime = new TimeSpan(14, 0, 0),
                    NumberOfGuests = 4,
                    BasePrice = 100m,
                    TotalPrice = 120m,
                    Status = ReservationStatus.Confirmed,
                    Notes = "Business meeting",
                    CreatedByUserId = adminUserId,
                    CreatedAt = DateTime.UtcNow
                },
                new Reservation
                {
                    ClientId = client.Id,
                    TableId = tables[2].Id,
                    Date = DateTime.UtcNow.Date.AddDays(2),
                    StartTime = new TimeSpan(17, 0, 0),
                    EndTime = new TimeSpan(18, 30, 0),
                    NumberOfGuests = 2,
                    BasePrice = 37.5m,
                    TotalPrice = 31.88m,
                    Status = ReservationStatus.Confirmed,
                    Notes = "Romantic dinner",
                    CreatedByUserId = adminUserId,
                    CreatedAt = DateTime.UtcNow
                },
                new Reservation
                {
                    ClientId = clients[1].Id,
                    TableId = tables[1].Id,
                    Date = DateTime.UtcNow.Date.AddDays(7),
                    StartTime = new TimeSpan(19, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    NumberOfGuests = 3,
                    BasePrice = 150m,
                    TotalPrice = 195m,
                    Status = ReservationStatus.Pending,
                    Notes = "Birthday celebration",
                    CreatedByUserId = adminUserId,
                    CreatedAt = DateTime.UtcNow
                }
            };

         context.Reservations.AddRange(reservations);
         await context.SaveChangesAsync();
      }
   }

    private static async Task<string> EnsureAdminAsync(RestaurantReservationDbContext context)
    {
        var adminRoleId = await EnsureRoleAsync(context, "Admin");
        await EnsureRoleAsync(context, "Manager");
        await EnsureRoleAsync(context, "Employee");

        var admin = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                Status = ApplicationUserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var hasher = new PasswordHasher<ApplicationUser>();
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }

        var adminRoleExists = await context.UserRoles.AnyAsync(ur => ur.UserId == admin.Id && ur.RoleId == adminRoleId);
        if (!adminRoleExists)
        {
            context.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = admin.Id,
                RoleId = adminRoleId
            });
            await context.SaveChangesAsync();
        }

        return admin.Id;
    }

    private static async Task<string> EnsureRoleAsync(RestaurantReservationDbContext context, string roleName)
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role is null)
        {
            role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };

            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        return role.Id;
    }
}
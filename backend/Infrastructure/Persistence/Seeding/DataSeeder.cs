using System;
using System.Collections.Generic;
using System.Linq;
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

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var adminUserId = await EnsureAdminAsync(context);
        var managerUserId = await EnsureStaffUserAsync(
            context,
            userName: "manager",
            email: "manager@example.com",
            password: "Manager123!",
            roleName: "Manager",
            status: ApplicationUserStatus.Active,
            now
        );
        var employeeUserId = await EnsureStaffUserAsync(
            context,
            userName: "employee",
            email: "employee@example.com",
            password: "Employee123!",
            roleName: "Employee",
            status: ApplicationUserStatus.Active,
            now
        );
        var employee2UserId = await EnsureStaffUserAsync(
            context,
            userName: "employee2",
            email: "employee2@example.com",
            password: "Employee123!",
            roleName: "Employee",
            status: ApplicationUserStatus.Active,
            now
        );

        // Table types
        var vipTableType = await EnsureTableTypeAsync(
            context,
            name: "VIP",
            description: "Premium dining area with exclusive services",
            basePricePerHour: 50m,
            now
        );
        var standardTableType = await EnsureTableTypeAsync(
            context,
            name: "Standard",
            description: "Regular dining table for everyday service",
            basePricePerHour: 25m,
            now
        );
        var terraceTableType = await EnsureTableTypeAsync(
            context,
            name: "Terrace",
            description: "Outdoor tables with a terrace view",
            basePricePerHour: 32m,
            now
        );
        var privateRoomTableType = await EnsureTableTypeAsync(
            context,
            name: "Private Room",
            description: "Closed space for events, groups and special occasions",
            basePricePerHour: 60m,
            now
        );

        // Tables
        if (!context.Tables.Any())
        {
            var tables = new List<Table>
            {
                new() { Code = "VIP01", Capacity = 6, Location = "Main hall", TableTypeId = vipTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "VIP02", Capacity = 4, Location = "Window side", TableTypeId = vipTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "VIP03", Capacity = 2, Location = "Chef counter", TableTypeId = vipTableType.Id, Status = TableStatus.Maintenance, CreatedAt = now },
                new() { Code = "STD01", Capacity = 4, Location = "Main hall", TableTypeId = standardTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "STD02", Capacity = 2, Location = "Bar area", TableTypeId = standardTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "STD03", Capacity = 8, Location = "Family zone", TableTypeId = standardTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "STD04", Capacity = 6, Location = "Terrace entrance", TableTypeId = standardTableType.Id, Status = TableStatus.Inactive, CreatedAt = now },
                new() { Code = "TER01", Capacity = 4, Location = "Terrace north", TableTypeId = terraceTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "TER02", Capacity = 2, Location = "Terrace center", TableTypeId = terraceTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "TER03", Capacity = 6, Location = "Rooftop", TableTypeId = terraceTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "PRIV01", Capacity = 8, Location = "Private room A", TableTypeId = privateRoomTableType.Id, Status = TableStatus.Active, CreatedAt = now },
                new() { Code = "PRIV02", Capacity = 12, Location = "Private room B", TableTypeId = privateRoomTableType.Id, Status = TableStatus.Active, CreatedAt = now },
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
                new() { FirstName = "Keyner", LastName = "De Ávila", Email = "kda.ts@gmail.com", Phone = "3022851699", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "John", LastName = "Doe", Email = "johndoe_12@yahoo.com", Phone = "4155557285", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@hotmail.com", Phone = "3055551234", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Carlos", LastName = "Rodriguez", Email = "c.rodriguez@gmail.com", Phone = "7865559876", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Ana", LastName = "Martinez", Email = "ana.martinez@gmail.com", Phone = "3125551001", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Luis", LastName = "Fernandez", Email = "luis.fernandez@gmail.com", Phone = "3125551002", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Sofia", LastName = "Lopez", Email = "sofia.lopez@gmail.com", Phone = "3125551003", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Diego", LastName = "Perez", Email = "diego.perez@gmail.com", Phone = "3125551004", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Valentina", LastName = "Gomez", Email = "valentina.gomez@gmail.com", Phone = "3125551005", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Andres", LastName = "Castro", Email = "andres.castro@gmail.com", Phone = "3125551006", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Camila", LastName = "Ruiz", Email = "camila.ruiz@gmail.com", Phone = "3125551007", Status = ClientStatus.Active, CreatedAt = now },
                new() { FirstName = "Javier", LastName = "Moreno", Email = "javier.moreno@gmail.com", Phone = "3125551008", Status = ClientStatus.Inactive, CreatedAt = now },
            };

            context.Clients.AddRange(clients);
            await context.SaveChangesAsync();
        }
        else
        {
            clients = await context.Clients.OrderBy(c => c.Id).ToListAsync();
        }

        // Pricing rules
        if (!context.PricingRules.Any())
        {
            var startDate = today;
            var endDate = DateOnly.FromDateTime(now.AddMonths(6));

            var pricingRules = new List<PricingRule>
            {
                new() { RuleName = "VIP Lunch Surcharge", RuleType = "Peak Hour", StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(14, 0, 0), SurchargePercentage = 20m, StartDate = startDate, EndDate = endDate, TableTypeId = vipTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "VIP Dinner Surcharge", RuleType = "Weekend", StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(23, 0, 0), SurchargePercentage = 30m, StartDate = startDate, EndDate = endDate, TableTypeId = vipTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "VIP Sunday Brunch", RuleType = "Special Event", StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(13, 0, 0), SurchargePercentage = 18m, StartDate = startDate, EndDate = endDate, TableTypeId = vipTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "Standard Early Bird Discount", RuleType = "Discount", StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(18, 30, 0), SurchargePercentage = -15m, StartDate = startDate, EndDate = endDate, TableTypeId = standardTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "Standard Weekend Prime", RuleType = "Weekend", StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(22, 0, 0), SurchargePercentage = 10m, StartDate = startDate, EndDate = endDate, TableTypeId = standardTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "Family Lunch Promo", RuleType = "Discount", StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(15, 0, 0), SurchargePercentage = -10m, StartDate = startDate, EndDate = endDate, TableTypeId = standardTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "Terrace Sunset Surcharge", RuleType = "Peak Hour", StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(21, 0, 0), SurchargePercentage = 15m, StartDate = startDate, EndDate = endDate, TableTypeId = terraceTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "Terrace Brunch Surcharge", RuleType = "Weekend", StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(13, 0, 0), SurchargePercentage = 12m, StartDate = startDate, EndDate = endDate, TableTypeId = terraceTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "Private Room Premium", RuleType = "Premium Service", StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(22, 0, 0), SurchargePercentage = 25m, StartDate = startDate, EndDate = endDate, TableTypeId = privateRoomTableType.Id, IsActive = true, CreatedAt = now },
                new() { RuleName = "Private Room Late Event", RuleType = "Event", StartTime = new TimeSpan(20, 0, 0), EndTime = new TimeSpan(23, 0, 0), SurchargePercentage = 35m, StartDate = startDate, EndDate = endDate, TableTypeId = privateRoomTableType.Id, IsActive = true, CreatedAt = now },
            };

            context.PricingRules.AddRange(pricingRules);
            await context.SaveChangesAsync();

            var pricingRuleDays = new List<PricingRuleDays>();
            foreach (var rule in pricingRules)
            {
                if (rule.RuleName == "VIP Dinner Surcharge")
                {
                    pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Friday });
                    pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Saturday });
                }
                else if (rule.RuleName is "VIP Sunday Brunch" or "Terrace Brunch Surcharge")
                {
                    pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Saturday });
                    pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Sunday });
                }
                else if (rule.RuleName == "Private Room Late Event")
                {
                    pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Friday });
                    pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Saturday });
                    pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = DaysOfWeek.Sunday });
                }
                else
                {
                    for (var i = 1; i <= 5; i++)
                    {
                        pricingRuleDays.Add(new PricingRuleDays { PricingRuleId = rule.Id, DayOfWeek = (DaysOfWeek)i });
                    }
                }
            }

            context.PricingRuleDays.AddRange(pricingRuleDays);
            await context.SaveChangesAsync();
        }

        // Reservations
        if (!context.Reservations.Any())
        {
            var tables = await context.Tables.OrderBy(t => t.Id).ToListAsync();

            var reservations = new List<Reservation>
            {
                new() { ClientId = clients[0].Id, TableId = tables[0].Id, Date = now.Date.AddDays(1), StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(14, 0, 0), NumberOfGuests = 4, BasePrice = 100m, TotalPrice = 120m, Status = ReservationStatus.Confirmed, Notes = "Business meeting", CreatedByUserId = adminUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[1].Id, TableId = tables[2].Id, Date = now.Date.AddDays(2), StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(18, 30, 0), NumberOfGuests = 2, BasePrice = 37.5m, TotalPrice = 31.88m, Status = ReservationStatus.Confirmed, Notes = "Romantic dinner", CreatedByUserId = managerUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[2].Id, TableId = tables[3].Id, Date = now.Date.AddDays(3), StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(21, 0, 0), NumberOfGuests = 3, BasePrice = 75m, TotalPrice = 82.50m, Status = ReservationStatus.Pending, Notes = "Birthday celebration", CreatedByUserId = employeeUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[3].Id, TableId = tables[4].Id, Date = now.Date.AddDays(4), StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(12, 0, 0), NumberOfGuests = 2, BasePrice = 50m, TotalPrice = 45m, Status = ReservationStatus.Completed, Notes = "Breakfast meeting", CreatedByUserId = employee2UserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[4].Id, TableId = tables[5].Id, Date = now.Date.AddDays(5), StartTime = new TimeSpan(12, 30, 0), EndTime = new TimeSpan(14, 30, 0), NumberOfGuests = 6, BasePrice = 120m, TotalPrice = 108m, Status = ReservationStatus.Confirmed, Notes = "Family lunch", CreatedByUserId = adminUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[5].Id, TableId = tables[6].Id, Date = now.Date.AddDays(6), StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(20, 0, 0), NumberOfGuests = 4, BasePrice = 96m, TotalPrice = 110.40m, Status = ReservationStatus.Pending, Notes = "Anniversary dinner", CreatedByUserId = managerUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[6].Id, TableId = tables[7].Id, Date = now.Date.AddDays(7), StartTime = new TimeSpan(20, 0, 0), EndTime = new TimeSpan(22, 0, 0), NumberOfGuests = 2, BasePrice = 64m, TotalPrice = 73.60m, Status = ReservationStatus.Confirmed, Notes = "Date night on terrace", CreatedByUserId = employeeUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[7].Id, TableId = tables[8].Id, Date = now.Date.AddDays(8), StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(13, 0, 0), NumberOfGuests = 5, BasePrice = 100m, TotalPrice = 115m, Status = ReservationStatus.Completed, Notes = "Brunch with friends", CreatedByUserId = employee2UserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[8].Id, TableId = tables[9].Id, Date = now.Date.AddDays(9), StartTime = new TimeSpan(21, 0, 0), EndTime = new TimeSpan(23, 0, 0), NumberOfGuests = 8, BasePrice = 240m, TotalPrice = 300m, Status = ReservationStatus.Confirmed, Notes = "Private celebration", CreatedByUserId = adminUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[9].Id, TableId = tables[10].Id, Date = now.Date.AddDays(10), StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(16, 0, 0), NumberOfGuests = 10, BasePrice = 360m, TotalPrice = 450m, Status = ReservationStatus.Pending, Notes = "Corporate lunch", CreatedByUserId = managerUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[10].Id, TableId = tables[11].Id, Date = now.Date.AddDays(11), StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(21, 0, 0), NumberOfGuests = 12, BasePrice = 540m, TotalPrice = 675m, Status = ReservationStatus.Confirmed, Notes = "Executive dinner", CreatedByUserId = employeeUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[11].Id, TableId = tables[1].Id, Date = now.Date.AddDays(12), StartTime = new TimeSpan(16, 30, 0), EndTime = new TimeSpan(18, 0, 0), NumberOfGuests = 3, BasePrice = 60m, TotalPrice = 54m, Status = ReservationStatus.Cancelled, Notes = "Last minute cancellation", CreatedByUserId = employee2UserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[0].Id, TableId = tables[4].Id, Date = now.Date.AddDays(13), StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(15, 30, 0), NumberOfGuests = 4, BasePrice = 62.5m, TotalPrice = 56.25m, Status = ReservationStatus.Confirmed, Notes = "Team lunch", CreatedByUserId = adminUserId, CreatedAt = now, UpdatedAt = now },
                new() { ClientId = clients[1].Id, TableId = tables[7].Id, Date = now.Date.AddDays(14), StartTime = new TimeSpan(19, 30, 0), EndTime = new TimeSpan(21, 30, 0), NumberOfGuests = 2, BasePrice = 64m, TotalPrice = 73.60m, Status = ReservationStatus.Pending, Notes = "Sunset dinner", CreatedByUserId = managerUserId, CreatedAt = now, UpdatedAt = now },
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
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var hasher = new PasswordHasher<ApplicationUser>();
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }

        var adminRoleExists = await context.UserRoles.AnyAsync(ur =>
            ur.UserId == admin.Id && ur.RoleId == adminRoleId
        );
        if (!adminRoleExists)
        {
            context.UserRoles.Add(
                new IdentityUserRole<string> { UserId = admin.Id, RoleId = adminRoleId }
            );
            await context.SaveChangesAsync();
        }

        return admin.Id;
    }

    private static async Task<string> EnsureStaffUserAsync(
        RestaurantReservationDbContext context,
        string userName,
        string email,
        string password,
        string roleName,
        ApplicationUserStatus status,
        DateTime now
    )
    {
        var roleId = await EnsureRoleAsync(context, roleName);

        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                Status = status,
                CreatedAt = now,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var hasher = new PasswordHasher<ApplicationUser>();
            user.PasswordHash = hasher.HashPassword(user, password);

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var userRoleExists = await context.UserRoles.AnyAsync(ur =>
            ur.UserId == user.Id && ur.RoleId == roleId
        );
        if (!userRoleExists)
        {
            context.UserRoles.Add(
                new IdentityUserRole<string> { UserId = user.Id, RoleId = roleId }
            );
            await context.SaveChangesAsync();
        }

        return user.Id;
    }

    private static async Task<TableType> EnsureTableTypeAsync(
        RestaurantReservationDbContext context,
        string name,
        string description,
        decimal basePricePerHour,
        DateTime now
    )
    {
        var tableType = await context.TableTypes.FirstOrDefaultAsync(tt => tt.Name == name);
        if (tableType is null)
        {
            tableType = new TableType
            {
                Name = name,
                Description = description,
                BasePricePerHour = basePricePerHour,
                IsActive = true,
                CreatedAt = now,
            };

            context.TableTypes.Add(tableType);
            await context.SaveChangesAsync();
        }

        return tableType;
    }

    private static async Task<string> EnsureRoleAsync(
        RestaurantReservationDbContext context,
        string roleName
    )
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role is null)
        {
            role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
            };

            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        return role.Id;
    }
}

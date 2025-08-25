using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common.Helpers;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;
using RestaurantReservation.Infrastructure.Persistence.Seeding;
using RestaurantReservation.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<RestaurantReservationDbContext>(options => options.UseSqlServer
    (connectionString, x => x.MigrationsAssembly("RestaurantReservation.Infrastructure")));
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IPricingRuleDaysRepository, PricingRuleDaysRepository>();
builder.Services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<ITableTypeRepository, TableTypeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<PricingCalculator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
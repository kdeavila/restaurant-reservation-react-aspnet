using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Infrastructure.Persistence;
using RestaurantReservation.Infrastructure.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<RestaurantReservationDbContext>
(options => options.UseSqlServer(connectionString,
    x => x.MigrationsAssembly("RestaurantReservation.Infrastructure"))
);
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
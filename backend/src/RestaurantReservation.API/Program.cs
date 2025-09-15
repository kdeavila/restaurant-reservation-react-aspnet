using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Application.UseCases.PricingRules;
using RestaurantReservation.Application.UseCases.Reservations;
using RestaurantReservation.Infrastructure.Persistence;
using RestaurantReservation.Infrastructure.Persistence.Seeding;
using RestaurantReservation.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddControllers();

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        }
    );

builder.Services.AddAuthorization();

// Register DbContext with SQL Server and specify the migration assembly
builder.Services.AddDbContext<RestaurantReservationDbContext>(options => options.UseSqlServer
    (connectionString, x => x.MigrationsAssembly("RestaurantReservation.Infrastructure")));

// Register repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<ITableTypeRepository, TableTypeRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
builder.Services.AddScoped<IPricingRuleDaysRepository, PricingRuleDaysRepository>();

// Register services
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<ITableTypeService, TableTypeService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPricingRuleService, PricingRuleService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Register UseCases
builder.Services.AddScoped<CreateReservationUseCase>();
builder.Services.AddScoped<UpdateReservationUseCase>();
builder.Services.AddScoped<CreatePricingRuleUseCase>();

// Register Helpers (there are no helpers yet, but this is where they would go)

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
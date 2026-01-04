using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using DotNetEnv;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RestaurantReservation.API.Middlewares;
using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Application.UseCases.PricingRules;
using RestaurantReservation.Application.UseCases.Reservations;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Infrastructure.Persistence;
using RestaurantReservation.Infrastructure.Persistence.Seeding;
using RestaurantReservation.Infrastructure.Repositories;

Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Require authentication by default for all endpoints
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Register DbContext with SQL Server and specify the migration assembly
builder.Services.AddDbContext<RestaurantReservationDbContext>(options =>
{
    options.UseSqlServer(connectionString, x => x.MigrationsAssembly("Infrastructure"));
});

// Configure Identity first
builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<RestaurantReservationDbContext>();

// Configure JWT authentication (replaces Identity's default authentication)
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrManager", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("AllRoles", policy => policy.RequireRole("Admin", "Manager", "Employee"));
});

// Type adapter configuration Mapster DTOs
TypeAdapterConfig<Client, ClientDto>
    .NewConfig()
    .Map(dest => dest.TotalReservations, src => src.Reservations.Count);

TypeAdapterConfig<PricingRule, PricingRuleDto>
    .NewConfig()
    .Map(dest => dest.DaysOfWeek, src => src.PricingRuleDays.Select(d => d.DayOfWeek));

TypeAdapterConfig<ApplicationUser, UserSimpleDto>
    .NewConfig()
    .Map(dest => dest.Username, src => src.UserName ?? string.Empty)
    .Map(dest => dest.Email, src => src.Email ?? string.Empty)
    .Map(dest => dest.Status, src => src.Status.ToString());

TypeAdapterConfig<Reservation, ReservationDto>
    .NewConfig()
    .Map(dest => dest.Date, src => DateOnly.FromDateTime(src.Date));

TypeAdapterConfig<PricingRule, PricingRuleDto>
    .NewConfig()
    .Map(dest => dest.DaysOfWeek, src => src.PricingRuleDays.Select(d => d.DayOfWeek));

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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Restaurant Reservation API",
            Version = "v1",
            Description = "API versionada. Prefijo de ruta: api/v1/",
        }
    );
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description =
                "Introduce el token JWT en el header Authorization como: Bearer {tu_token_jwt}",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer",
        }
    );
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );

    // Render TimeSpan as simple HH:mm:ss strings in Swagger UI (helps query params like startTime/endTime)
    options.MapType<TimeSpan>(() =>
        new OpenApiSchema
        {
            Type = "string",
            Format = "time-span",
            Example = new OpenApiString("12:00:00"),
        }
    );

    options.MapType<TimeSpan?>(() =>
        new OpenApiSchema
        {
            Type = "string",
            Format = "time-span",
            Example = new OpenApiString("14:00:00"),
            Nullable = true,
        }
    );

    options.DocInclusionPredicate((docName, apiDesc) => apiDesc.GroupName == docName);
});

// Configure JSON options
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

apiVersioningBuilder.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "frontend",
        app =>
        {
            app.WithOrigins("http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
});

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 256 * 1024; // 256 KB - sufficient for catalog data
    options.UseCaseSensitivePaths = false;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();

    var apiVersionDescriptionProvider =
        app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"API {description.GroupName.ToUpperInvariant()}"
            );
        }
    });

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<RestaurantReservationDbContext>();
    await DataSeeder.SeedAsync(context);
}

// Middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

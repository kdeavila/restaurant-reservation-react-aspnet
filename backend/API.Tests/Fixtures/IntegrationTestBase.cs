using Microsoft.Extensions.DependencyInjection;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Infrastructure.Persistence;
using RestaurantReservation.Infrastructure.Persistence.Seeding;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace RestaurantReservation.API.Tests.Fixtures;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected HttpClient Client = default!;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
        await SeedAsync();
        Client = Factory.CreateClient();
        await AuthenticateAsAdminAsync();
    }

    protected async Task AuthenticateAsAdminAsync()
    {
        var loginDto = new LoginDto { Email = "admin@example.com", Password = "Admin123!" };
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthDto>>();
        SetAuthToken(loginBody!.Data!.Token);
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected async Task WithScopedService(Func<IServiceProvider, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    protected async Task SeedAsync()
    {
        await WithScopedService(async sp =>
        {
            var db = sp.GetRequiredService<RestaurantReservationDbContext>();
            await DataSeeder.SeedAsync(db);
        });
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}

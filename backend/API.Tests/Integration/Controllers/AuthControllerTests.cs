using FluentAssertions;
using RestaurantReservation.API.Tests.Fixtures;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.User;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RestaurantReservation.API.Tests.Integration.Controllers;

public class AuthControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_returns_token_for_seed_admin()
    {
        var loginDto = new LoginDto { Email = "admin@example.com", Password = "Admin123!" };

        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthDto>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data!.Token.Should().NotBeNullOrEmpty();
        body.Data.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Register_creates_user_and_allows_login()
    {
        var email = $"user_{Guid.NewGuid():N}@example.com";
        var registerDto = new CreateUserDto
        {
            Username = $"user_{Guid.NewGuid():N}",
            Email = email,
            Password = "Passw0rd!",
            Role = Domain.Enums.ApplicationUserRole.Employee,
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginDto = new LoginDto { Email = email, Password = registerDto.Password };
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthDto>>();
        loginBody.Should().NotBeNull();
        loginBody!.Success.Should().BeTrue();
        loginBody.Data!.Email.Should().Be(email);
    }
}

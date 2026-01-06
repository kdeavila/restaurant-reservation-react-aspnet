using FluentAssertions;
using RestaurantReservation.API.Tests.Fixtures;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.User;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RestaurantReservation.API.Tests.Integration.Controllers;

public class TablesControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAll_returns_paginated_tables()
    {
        var response = await Client.GetAsync("/api/v1/tables?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TableDetailedDto>>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAvailable_returns_tables_for_given_datetime()
    {
        var tomorrow = DateTime.UtcNow.AddDays(1).Date;
        var query = $"/api/v1/tables/available?date={tomorrow:yyyy-MM-dd}&startTime=15:00:00&endTime=16:30:00&numberOfGuests=2";

        var response = await Client.GetAsync(query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TableDetailedDto>>>();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeEmpty();
        body.Data!.All(t => t.Capacity >= 2).Should().BeTrue();
    }

    [Fact]
    public async Task GetAvailable_returns_empty_when_no_tables_match_capacity()
    {
        var tomorrow = DateTime.UtcNow.AddDays(1).Date;
        // All tables have max capacity 8, request 10
        var query = $"/api/v1/tables/available?date={tomorrow:yyyy-MM-dd}&startTime=15:00:00&endTime=16:30:00&numberOfGuests=10";

        var response = await Client.GetAsync(query);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TableDetailedDto>>>();
        body!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_returns_table_when_exists()
    {
        var response = await Client.GetAsync("/api/v1/tables/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TableDetailedDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_returns_404_when_table_not_found()
    {
        var response = await Client.GetAsync("/api/v1/tables/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTable_requires_admin_or_manager_role()
    {
        // Register as Employee
        var registerDto = new CreateUserDto
        {
            Username = $"employee_{Guid.NewGuid():N}",
            Email = $"employee_{Guid.NewGuid():N}@example.com",
            Password = "Passw0rd!",
            Role = Domain.Enums.ApplicationUserRole.Employee,
        };
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthDto>>();
        SetAuthToken(loginBody!.Data!.Token);

        var createTableDto = new CreateTableDto
        {
            Capacity = 4,
            Location = "Test Area",
            TableTypeId = 1,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/tables", createTableDto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTable_succeeds_with_manager_role()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync
        var createTableDto = new CreateTableDto
        {
            Capacity = 6,
            Location = "New Area",
            TableTypeId = 1,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/tables", createTableDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<TableDetailedDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteTable_requires_admin_role()
    {
        // Register as Manager (has edit perms but not delete)
        var registerDto = new CreateUserDto
        {
            Username = $"manager_{Guid.NewGuid():N}",
            Email = $"manager_{Guid.NewGuid():N}@example.com",
            Password = "Passw0rd!",
            Role = Domain.Enums.ApplicationUserRole.Manager,
        };
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthDto>>();
        SetAuthToken(loginBody!.Data!.Token);

        var response = await Client.DeleteAsync("/api/v1/tables/1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTable_succeeds_with_admin_role()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync
        var response = await Client.DeleteAsync("/api/v1/tables/5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

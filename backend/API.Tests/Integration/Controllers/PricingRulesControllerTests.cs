using FluentAssertions;
using RestaurantReservation.API.Tests.Fixtures;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.PricingRule;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RestaurantReservation.API.Tests.Integration.Controllers;

public class PricingRulesControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAll_returns_paginated_pricing_rules()
    {
        var response = await Client.GetAsync("/api/v1/pricing-rules?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PricingRuleDto>>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetById_returns_pricing_rule_when_exists()
    {
        var response = await Client.GetAsync("/api/v1/pricing-rules/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PricingRuleDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_returns_404_when_pricing_rule_not_found()
    {
        var response = await Client.GetAsync("/api/v1/pricing-rules/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePricingRule_requires_admin_or_manager_role()
    {
        // Register as Employee
        var registerDto = new CreateUserDto
        {
            Username = $"employee_{Guid.NewGuid():N}",
            Email = $"employee_{Guid.NewGuid():N}@example.com",
            Password = "Passw0rd!",
            Role = ApplicationUserRole.Employee,
        };
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthDto>>();
        SetAuthToken(loginBody!.Data!.Token);

        var createRuleDto = new CreatePricingRuleDto
        {
            RuleName = "Test Rule",
            RuleType = "Peak",
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            SurchargePercentage = 25m,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            TableTypeId = 1,
            DaysOfWeek = new() { DaysOfWeek.Monday, DaysOfWeek.Tuesday, DaysOfWeek.Wednesday, DaysOfWeek.Thursday, DaysOfWeek.Friday }
        };

        var response = await Client.PostAsJsonAsync("/api/v1/pricing-rules", createRuleDto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePricingRule_succeeds_with_valid_data()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync
        var createRuleDto = new CreatePricingRuleDto
        {
            RuleName = "Late Night Surcharge",
            RuleType = "Peak",
            StartTime = TimeSpan.FromHours(22),
            EndTime = TimeSpan.FromHours(23),
            SurchargePercentage = 50m,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            TableTypeId = 2,
            DaysOfWeek = new() { DaysOfWeek.Friday, DaysOfWeek.Saturday }
        };

        var response = await Client.PostAsJsonAsync("/api/v1/pricing-rules", createRuleDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PricingRuleDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.RuleName.Should().Be("Late Night Surcharge");
        body.Data.SurchargePercentage.Should().Be(50m);
    }

    [Fact]
    public async Task UpdatePricingRule_succeeds_with_valid_changes()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync
        var updateDto = new UpdatePricingRuleDto
        {
            Id = 1,
            SurchargePercentage = 35m,
        };

        var response = await Client.PatchAsJsonAsync("/api/v1/pricing-rules/1", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<PricingRuleDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.SurchargePercentage.Should().Be(35m);
    }

    [Fact]
    public async Task DeletePricingRule_requires_admin_role()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync
        var response = await Client.DeleteAsync("/api/v1/pricing-rules/3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

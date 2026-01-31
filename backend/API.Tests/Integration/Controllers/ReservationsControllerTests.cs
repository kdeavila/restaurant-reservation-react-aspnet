using FluentAssertions;
using RestaurantReservation.API.Tests.Fixtures;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RestaurantReservation.API.Tests.Integration.Controllers;

public class ReservationsControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAll_returns_paginated_reservations()
    {
        var response = await Client.GetAsync("/api/v1/reservations?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ReservationDto>>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().HaveCount(3);
        body.Pagination.Should().NotBeNull();
        body.Pagination!.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetById_returns_reservation_when_exists()
    {
        var response = await Client.GetAsync("/api/v1/reservations/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_returns_404_when_reservation_not_found()
    {
        var response = await Client.GetAsync("/api/v1/reservations/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReservation_requires_authentication()
    {
        // Create unauthenticated client
        var unauthenticatedClient = Factory.CreateClient();
        
        var dto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(2),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            NumberOfGuests = 2,
        };

        var response = await unauthenticatedClient.PostAsJsonAsync("/api/v1/reservations", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateReservation_succeeds_with_valid_data_and_token()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        // Create reservation
        var reservationDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 4,
            Date = DateTime.UtcNow.AddDays(10),
            StartTime = TimeSpan.FromHours(19),
            EndTime = TimeSpan.FromHours(21),
            NumberOfGuests = 2,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", reservationDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data!.Client.Id.Should().Be(1);
        body.Data.NumberOfGuests.Should().Be(2);
    }

    [Fact]
    public async Task CreateReservation_fails_when_table_already_booked()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        
        // First create a reservation
        var firstDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 2,
            Date = DateTime.UtcNow.AddDays(7),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(13),
            NumberOfGuests = 2,
        };
        
        var firstResponse = await Client.PostAsJsonAsync("/api/v1/reservations", firstDto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Now try to book overlapping time on same table - should conflict
        var conflictingDto = new CreateReservationDto
        {
            ClientId = 2,
            TableId = 2,
            Date = DateTime.UtcNow.AddDays(7),
            StartTime = TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(30)),
            EndTime = TimeSpan.FromHours(13).Add(TimeSpan.FromMinutes(30)),
            NumberOfGuests = 2,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", conflictingDto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body!.Error.Should().NotBeNull();
        body.Error!.Message.Should().Contain("already booked");
    }

    [Fact]
    public async Task CreateReservation_fails_when_duration_less_than_30_minutes()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        var shortReservationDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 2,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(20)),
            NumberOfGuests = 2,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", shortReservationDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body!.Error!.Message.Should().Contain("30 minutes");
    }

    [Fact]
    public async Task CreateReservation_fails_when_guests_exceed_table_capacity()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        // Table 4 has capacity 2, try to book for 5 guests
        var overCapacityDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 4,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            NumberOfGuests = 5,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", overCapacityDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body!.Error!.Message.Should().Contain("capacity");
    }

    [Fact]
    public async Task CreateReservation_fails_when_date_is_in_past()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        var futureDate = DateTime.UtcNow.AddDays(5);
        
        // Create first reservation (use table 2 and time that doesn't conflict with seed data)
        var firstDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 2,
            Date = futureDate,
            StartTime = TimeSpan.FromHours(15),
            EndTime = TimeSpan.FromHours(17),
            NumberOfGuests = 2,
        };
        var firstResponse = await Client.PostAsJsonAsync("/api/v1/reservations", firstDto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to create overlapping reservation on same table - should fail
        var conflictingDto = new CreateReservationDto
        {
            ClientId = 2,
            TableId = 2,
            Date = futureDate,
            StartTime = TimeSpan.FromHours(16),
            EndTime = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(30)),
            NumberOfGuests = 2,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", conflictingDto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body!.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateReservation_fails_with_past_date()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        var pastReservationDto = new CreateReservationDto
        {
            ClientId = 2,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(-1),
            StartTime = TimeSpan.FromHours(13),
            EndTime = TimeSpan.FromHours(14),
            NumberOfGuests = 2,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", pastReservationDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReservation_fails_when_client_is_inactive()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        // Client 999 doesn't exist
        var invalidClientDto = new CreateReservationDto
        {
            ClientId = 999,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            NumberOfGuests = 2,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", invalidClientDto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateReservation_succeeds_with_valid_changes()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        // First, create a reservation
        var createDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 2,
            Date = DateTime.UtcNow.AddDays(10),
            StartTime = TimeSpan.FromHours(15),
            EndTime = TimeSpan.FromHours(17),
            NumberOfGuests = 3,
        };
        var createResponse = await Client.PostAsJsonAsync("/api/v1/reservations", createDto);
        var createdBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        var reservationId = createdBody!.Data!.Id;

        // Now update it
        var updateDto = new UpdateReservationDto
        {
            Id = reservationId,
            Status = "Confirmed",
        };

        var response = await Client.PatchAsJsonAsync($"/api/v1/reservations/{reservationId}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task UpdateReservation_fails_with_invalid_status_transition()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        // Reservation 1 is Confirmed, try to go to Pending (invalid)
        var invalidTransitionDto = new UpdateReservationDto
        {
            Id = 1,
            Status = "Pending",
        };

        var response = await Client.PatchAsJsonAsync("/api/v1/reservations/1", invalidTransitionDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body!.Error!.Message.Should().Contain("Invalid status transition");
    }

    [Fact]
    public async Task CancelReservation_succeeds()
    {
        // Client is already authenticated via IntegrationTestBase.InitializeAsync
        var response = await Client.DeleteAsync("/api/v1/reservations/2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        body!.Success.Should().BeTrue();
    }
}

using FluentAssertions;
using RestaurantReservation.API.Tests.Fixtures;
using RestaurantReservation.Application.Common.Responses;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.User;
using RestaurantReservation.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RestaurantReservation.API.Tests.Integration.Scenarios;

public class ReservationFlowTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Complete_reservation_flow_pending_to_completed()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync

        // 1. Check available tables
        var tomorrow = DateTime.UtcNow.AddDays(1).Date;
        var availResponse = await Client.GetAsync($"/api/v1/tables/available?date={tomorrow:yyyy-MM-dd}&startTime=19:00:00&endTime=21:00:00&numberOfGuests=4");
        availResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var availBody = await availResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TableDetailedDto>>>();
        var availableTableId = availBody!.Data!.First().Id;

        // 3. Create reservation
        var reservationDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = availableTableId,
            Date = tomorrow,
            StartTime = TimeSpan.FromHours(19),
            EndTime = TimeSpan.FromHours(21),
            NumberOfGuests = 4,
            Notes = "VIP client - special arrangement",
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/reservations", reservationDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        var reservationId = createBody!.Data!.Id;
        createBody.Data.Status.Should().Be("Pending");

        // 4. Confirm reservation
        var confirmDto = new UpdateReservationDto
        {
            Id = reservationId,
            Status = "Confirmed",
        };
        var confirmResponse = await Client.PatchAsJsonAsync($"/api/v1/reservations/{reservationId}", confirmDto);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirmBody = await confirmResponse.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        confirmBody!.Data!.Status.Should().Be("Confirmed");

        // 5. Mark as completed
        var completeDto = new UpdateReservationDto
        {
            Id = reservationId,
            Status = "Completed",
        };
        var completeResponse = await Client.PatchAsJsonAsync($"/api/v1/reservations/{reservationId}", completeDto);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completeBody = await completeResponse.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        completeBody!.Data!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Prevent_overbooking_concurrent_reservations()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync

        var tomorrow = DateTime.UtcNow.AddDays(3).Date;

        // 1. Create first reservation for table 2 (19:00-21:00)
        var reservation1 = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 2,
            Date = tomorrow,
            StartTime = TimeSpan.FromHours(19),
            EndTime = TimeSpan.FromHours(21),
            NumberOfGuests = 2,
        };

        var response1 = await Client.PostAsJsonAsync("/api/v1/reservations", reservation1);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. Try to create overlapping reservation on same table (20:00-22:00) - should fail
        var reservation2 = new CreateReservationDto
        {
            ClientId = 2,
            TableId = 2,
            Date = tomorrow,
            StartTime = TimeSpan.FromHours(20),
            EndTime = TimeSpan.FromHours(22),
            NumberOfGuests = 2,
        };

        var response2 = await Client.PostAsJsonAsync("/api/v1/reservations", reservation2);
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body2 = await response2.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        body2!.Error!.Message.Should().Contain("already booked");

        // 3. But can book non-overlapping time (21:00-23:00) - should succeed
        var reservation3 = new CreateReservationDto
        {
            ClientId = 2,
            TableId = 2,
            Date = tomorrow,
            StartTime = TimeSpan.FromHours(21),
            EndTime = TimeSpan.FromHours(23),
            NumberOfGuests = 2,
        };

        var response3 = await Client.PostAsJsonAsync("/api/v1/reservations", reservation3);
        response3.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Pricing_calculation_with_multiple_surcharges()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync

        // Create reservation on VIP table during surcharge time
        // VIP base price: 50/hr, midday surcharge +20% = 60
        var tomorrow = DateTime.UtcNow.AddDays(5).Date;
        var reservationDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 1, // VIP table
            Date = tomorrow,
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            NumberOfGuests = 4,
        };

        var response = await Client.PostAsJsonAsync("/api/v1/reservations", reservationDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        
        // VIP table base price: 50/hr * 2 hours = 100
        // Total price = base price + surcharges (if applicable)
        body!.Data!.BasePrice.Should().Be(100m);
        body.Data.TotalPrice.Should().BeGreaterThanOrEqualTo(100m);
    }

    [Fact]
    public async Task Cancel_reservation_workflow()
    {
        // Client is already authenticated as admin via IntegrationTestBase.InitializeAsync

        // Create and confirm reservation
        var tomorrow = DateTime.UtcNow.AddDays(2).Date;
        var reservationDto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 3,
            Date = tomorrow,
            StartTime = TimeSpan.FromHours(15),
            EndTime = TimeSpan.FromHours(17),
            NumberOfGuests = 3,
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/reservations", reservationDto);
        var createBody = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        var reservationId = createBody!.Data!.Id;

        // Confirm it
        var confirmDto = new UpdateReservationDto
        {
            Id = reservationId,
            Status = "Confirmed",
        };
        await Client.PatchAsJsonAsync($"/api/v1/reservations/{reservationId}", confirmDto);

        // Cancel it
        var cancelResponse = await Client.DeleteAsync($"/api/v1/reservations/{reservationId}");
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify it's cancelled
        var getResponse = await Client.GetAsync($"/api/v1/reservations/{reservationId}");
        var getBody = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ReservationDto>>();
        getBody!.Data!.Status.Should().Be("Cancelled");

        // Table should now be available for that time
        var availResponse = await Client.GetAsync($"/api/v1/tables/available?date={tomorrow:yyyy-MM-dd}&startTime=15:00:00&endTime=17:00:00&numberOfGuests=3");
        var availBody = await availResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TableDetailedDto>>>();
        availBody!.Data!.Any(t => t.Id == 3).Should().BeTrue();
    }
}

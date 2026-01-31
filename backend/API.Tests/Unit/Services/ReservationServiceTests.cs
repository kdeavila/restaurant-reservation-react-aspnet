using FluentAssertions;
using Moq;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Application.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using Xunit;

namespace RestaurantReservation.API.Tests.Unit.Services;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepo = new();

    private ReservationService CreateSut() => new ReservationService(_reservationRepo.Object);

    [Fact]
    public async Task CreateAsync_succeeds_with_valid_data()
    {
        var dto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            NumberOfGuests = 4,
        };

        var sut = CreateSut();
        var result = await sut.CreateAsync(dto, "user123", 100m, 120m);

        result.IsFailure.Should().BeFalse();
        result.Value.ClientId.Should().Be(1);
        result.Value.Status.Should().Be(ReservationStatus.Pending);
        result.Value.BasePrice.Should().Be(100m);
        result.Value.TotalPrice.Should().Be(120m);
    }

    [Fact]
    public async Task CreateAsync_fails_when_duration_less_than_30_minutes()
    {
        var dto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(20)),
            NumberOfGuests = 2,
        };

        var sut = CreateSut();
        var result = await sut.CreateAsync(dto, "user123", 50m, 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("30 minutes");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_fails_when_date_is_in_past()
    {
        var dto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(-1),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            NumberOfGuests = 2,
        };

        var sut = CreateSut();
        var result = await sut.CreateAsync(dto, "user123", 50m, 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("future");
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateAsync_fails_when_prices_negative()
    {
        var dto = new CreateReservationDto
        {
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            NumberOfGuests = 2,
        };

        var sut = CreateSut();
        var result = await sut.CreateAsync(dto, "user123", -10m, 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("non-negative");
    }

    [Fact]
    public async Task UpdateAsync_fails_when_reservation_not_found()
    {
        _reservationRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var updateDto = new UpdateReservationDto { Id = 999 };
        var sut = CreateSut();
        var result = await sut.UpdateAsync(updateDto, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateAsync_fails_when_modifying_past_reservation()
    {
        var reservation = new Reservation
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(-1),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            Status = ReservationStatus.Completed,
        };

        _reservationRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var updateDto = new UpdateReservationDto { Id = 1, NumberOfGuests = 5 };
        var sut = CreateSut();
        var result = await sut.UpdateAsync(updateDto, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("past");
    }

    [Fact]
    public async Task UpdateAsync_fails_with_invalid_status_transition()
    {
        var reservation = new Reservation
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            Status = ReservationStatus.Completed,
        };

        _reservationRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Try to transition from Completed to Pending (invalid)
        var updateDto = new UpdateReservationDto
        {
            Id = 1,
            Status = "Pending",
        };
        var sut = CreateSut();
        var result = await sut.UpdateAsync(updateDto, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid status transition");
    }

    [Fact]
    public async Task UpdateAsync_succeeds_with_valid_state_transition()
    {
        var reservation = new Reservation
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            Status = ReservationStatus.Pending,
            NumberOfGuests = 2,
        };

        _reservationRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var updateDto = new UpdateReservationDto
        {
            Id = 1,
            Status = "Confirmed",
        };
        var sut = CreateSut();
        var result = await sut.UpdateAsync(updateDto, null, null);

        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("Reservation updated successfully.");
    }

    [Fact]
    public async Task CancelAsync_fails_when_reservation_not_found()
    {
        _reservationRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var sut = CreateSut();
        var result = await sut.CancelAsync(999);

        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CancelAsync_succeeds_with_valid_reservation()
    {
        var reservation = new Reservation
        {
            Id = 1,
            ClientId = 1,
            TableId = 1,
            Date = DateTime.UtcNow.AddDays(5),
            StartTime = TimeSpan.FromHours(12),
            EndTime = TimeSpan.FromHours(14),
            Status = ReservationStatus.Confirmed,
        };

        _reservationRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var sut = CreateSut();
        var result = await sut.CancelAsync(1);

        result.IsFailure.Should().BeFalse();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }
}

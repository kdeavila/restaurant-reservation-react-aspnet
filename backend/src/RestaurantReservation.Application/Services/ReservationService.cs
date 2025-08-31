using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class ReservationService(
    IReservationRepository reservationRepository
) : IReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    
    public async Task<Result<Reservation>> CreateReservationAsync(CreateReservationDto dto, decimal basePrice,
        decimal totalPrice,
        CancellationToken ct = default)
    {
        var duration = dto.EndTime - dto.StartTime;
        if (duration.TotalMinutes < 30)
            return Result.Failure<Reservation>("Reservation must be at least 30 minutes long.");

        if (dto.Date.Date < DateTime.UtcNow.Date)
            return Result.Failure<Reservation>("Reservation date cannot be in the past.");

        if (dto.StartTime >= dto.EndTime)
            return Result.Failure<Reservation>("End time must be after start time.");

        if (basePrice < 0 || totalPrice < 0)
            return Result.Failure<Reservation>("Prices must be non-negative.");

        var reservation = new Reservation()
        {
            ClientId = dto.ClientId,
            TableId = dto.TableId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            NumberOfGuests = dto.NumberOfGuests,
            BasePrice = basePrice,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Pending,
            Notes = dto.Notes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reservationRepository.AddAsync(reservation, ct);
        return Result.Success(reservation);
    }

    public async Task<Result> UpdateReservationAsync(Reservation reservation, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> CancelReservationAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
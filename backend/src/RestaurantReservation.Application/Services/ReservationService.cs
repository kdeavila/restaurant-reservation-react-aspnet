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

    public async Task<IEnumerable<ReservationDto>> GetAllAsync(CancellationToken ct = default)
    {
        var reservations = await _reservationRepository.GetAllAsync(ct);
        return reservations.Select(r => new ReservationDto(
            r.Id,
            r.ClientId,
            $"{r.Client.FirstName} {r.Client.LastName}",
            r.TableId,
            r.Table.Code,
            r.Date,
            r.StartTime,
            r.EndTime,
            r.NumberOfGuests,
            r.BasePrice,
            r.TotalPrice,
            r.Status.ToString(),
            r.Notes
        ));
    }

    public async Task<Result<ReservationDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null) return Result.Failure<ReservationDto>("Reservation not found", 404);

        var reservationDto = new ReservationDto(
            reservation.Id,
            reservation.ClientId,
            $"{reservation.Client.FirstName} {reservation.Client.LastName}",
            reservation.TableId,
            reservation.Table.Code,
            reservation.Date,
            reservation.StartTime,
            reservation.EndTime,
            reservation.NumberOfGuests,
            reservation.BasePrice,
            reservation.TotalPrice,
            reservation.Status.ToString(),
            reservation.Notes
        );
        return Result.Success(reservationDto);
    }

    public async Task<Result<Reservation>> CreateReservationAsync(
        CreateReservationDto dto, int createdByUserId,
        decimal basePrice, decimal totalPrice, CancellationToken ct = default)
    {
        var duration = dto.EndTime - dto.StartTime;
        if (duration.TotalMinutes < 30)
            return Result.Failure<Reservation>("Reservation must be at least 30 minutes long.", 400);

        if (dto.Date.Date < DateTime.UtcNow.Date)
            return Result.Failure<Reservation>("Reservation date cannot be in the past.", 400);

        if (dto.StartTime >= dto.EndTime)
            return Result.Failure<Reservation>("End time must be after start time.", 400);

        if (basePrice < 0 || totalPrice < 0)
            return Result.Failure<Reservation>("Prices must be non-negative.", 400);

        var reservation = new Reservation()
        {
            ClientId = dto.ClientId,
            TableId = dto.TableId,
            CreatedByUserId = createdByUserId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            NumberOfGuests = dto.NumberOfGuests,
            BasePrice = basePrice,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Pending,
            Notes = dto.Notes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _reservationRepository.AddAsync(reservation, ct);
        return Result.Success(reservation);
    }

    public async Task<Result> UpdateReservationAsync
        (UpdateReservationDto dto, decimal? basePrice, decimal? totalPrice, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(dto.Id, ct);
        if (reservation is null) return Result.Failure("Reservation not found.", 404);

        if (dto is { EndTime: not null, StartTime: not null })
        {
            var duration = dto.EndTime.Value - dto.StartTime.Value;
            if (duration.TotalMinutes < 30)
                return Result.Failure<Reservation>("Reservation must be at least 30 minutes long.", 400);

            if (dto.StartTime >= dto.EndTime)
                return Result.Failure<Reservation>("End time must be after start time.", 400);
        }

        reservation.TableId = dto.TableId ?? reservation.TableId;
        reservation.Date = dto.Date ?? reservation.Date;
        reservation.StartTime = dto.StartTime ?? reservation.StartTime;
        reservation.EndTime = dto.EndTime ?? reservation.EndTime;
        reservation.NumberOfGuests = dto.NumberOfGuests ?? reservation.NumberOfGuests;
        reservation.Notes = dto.Notes ?? reservation.Notes;

        if (!string.IsNullOrEmpty(dto.Status) &&
            Enum.TryParse<ReservationStatus>(dto.Status, out var parsed))
        {
            reservation.Status = parsed;
        }

        if (basePrice.HasValue && totalPrice.HasValue)
        {
            reservation.BasePrice = basePrice.Value;
            reservation.TotalPrice = totalPrice.Value;
        }

        reservation.UpdatedAt = DateTime.UtcNow;
        await _reservationRepository.UpdateAsync(reservation, ct);
        return Result.Success();
    }

    public async Task<Result> CancelReservationAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null) return Result.Failure("Reservation not found.", 404);

        if (reservation.Status == ReservationStatus.Cancelled)
            return Result.Failure("Reservation is already cancelled.", 400);

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation, ct);
        return Result.Success();
    }
}
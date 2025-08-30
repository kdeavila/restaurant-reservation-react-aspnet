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

    public async Task<Result<Reservation>> CreateReservationAsync(
        CreateReservationDto dto, decimal basePrice, decimal totalPrice, CancellationToken ct = default)
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

    public async Task<Result<ReservationDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null) return Result.Failure<ReservationDto>("Reservation not found.");

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
            r.Notes)
        ).ToList();
    }

    public async Task<IEnumerable<ReservationDto>> GetByClientIdAsync(int clientId, CancellationToken ct = default)
    {
        var reservations = await _reservationRepository.GetByClientIdAsync(clientId, ct);
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

    public async Task<IEnumerable<ReservationDto>> GetByTableIdAsync(int tableId, CancellationToken ct = default)
    {
        var reservations = await _reservationRepository.GetByTableIdAsync(tableId, ct);
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

    public async Task<Result> UpdateReservationAsync(Reservation reservation, CancellationToken ct = default)
    {
        await _reservationRepository.UpdateAsync(reservation, ct);
        return Result.Success();
    }

    public async Task<Result> CancelReservationAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null) return Result.Failure("Reservation not found.");

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation, ct);
        return Result.Success();
    }

    public async Task<Result> DeleteReservationAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null) return Result.Failure("Reservation not found.");

        await _reservationRepository.DeleteAsync(reservation.Id, ct);
        return Result.Success();
    }
}
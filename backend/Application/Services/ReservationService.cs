using Mapster;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class ReservationService(IReservationRepository reservationRepository) : IReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;

    public async Task<(
        IEnumerable<ReservationDto> Data,
        PaginationMetadata Pagination
    )> GetAllAsync(ReservationQueryParams queryParams, CancellationToken ct = default)
    {
        var query = _reservationRepository.Query();

        if (queryParams.ClientId.HasValue)
            query = query.Where(r => r.ClientId == queryParams.ClientId.Value);

        if (queryParams.TableId.HasValue)
            query = query.Where(r => r.TableId == queryParams.TableId.Value);

        if (queryParams.Date.HasValue)
        {
            // Ensure date is UTC for PostgreSQL compatibility
            var utcDate = DateTime.SpecifyKind(queryParams.Date.Value, DateTimeKind.Utc);
            query = query.Where(r => r.Date == utcDate);
        }

        if (queryParams.StartTime.HasValue)
            query = query.Where(r => r.StartTime >= queryParams.StartTime.Value);

        if (queryParams.EndTime.HasValue)
            query = query.Where(r => r.EndTime <= queryParams.EndTime.Value);

        if (
            !string.IsNullOrWhiteSpace(queryParams.Status)
            && Enum.TryParse<ReservationStatus>(queryParams.Status, true, out var parsedStatus)
        )
        {
            query = query.Where(r => r.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(ct);

        var skipNumber = (queryParams.Page - 1) * queryParams.PageSize;
        var data = await query
            .OrderBy(r => r.Id)
            .Skip(skipNumber)
            .Take(queryParams.PageSize)
            .Select(reservation => reservation.Adapt<ReservationDto>())
            .ToListAsync(ct);

        var pagination = new PaginationMetadata
        {
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize),
        };

        return (data, pagination);
    }

    public async Task<Result<ReservationDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null)
            return Result.Failure<ReservationDto>("Reservation not found", 404);

        var reservationDto = reservation.Adapt<ReservationDto>();
        return Result.Success(reservationDto);
    }

    public async Task<Result<Reservation>> CreateAsync(
        CreateReservationDto dto,
        string createdByUserId,
        decimal basePrice,
        decimal totalPrice,
        CancellationToken ct = default
    )
    {
        var duration = dto.EndTime - dto.StartTime;
        if (duration.TotalMinutes < 30)
            return Result.Failure<Reservation>(
                "Reservation must be at least 30 minutes long.",
                400
            );

        // Ensure date is UTC for PostgreSQL compatibility
        var utcDate = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);
        var reservationDateTime = utcDate.Date + dto.StartTime;
        if (reservationDateTime <= DateTime.UtcNow)
            return Result.Failure<Reservation>("Reservation date must be in the future.", 400);

        if (basePrice < 0 || totalPrice < 0)
            return Result.Failure<Reservation>("Prices must be non-negative.", 400);

        var reservation = new Reservation()
        {
            ClientId = dto.ClientId,
            TableId = dto.TableId,
            CreatedByUserId = createdByUserId,
            Date = utcDate,
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

    public async Task<Result<string>> UpdateAsync(
        UpdateReservationDto dto,
        decimal? basePrice,
        decimal? totalPrice,
        CancellationToken ct = default
    )
    {
        var reservation = await _reservationRepository.GetByIdAsync(dto.Id, ct);
        if (reservation is null)
            return Result.Failure<string>("Reservation not found.", 404);

        // Prevent modifying past reservations
        var reservationStartDateTime = reservation.Date + reservation.StartTime;
        if (DateTime.UtcNow > reservationStartDateTime)
            return Result.Failure<string>("Cannot modify a past reservation.", 400);

        var finalStartTime = dto.StartTime ?? reservation.StartTime;
        var finalEndTime = dto.EndTime ?? reservation.EndTime;

        var duration = finalEndTime - finalStartTime;
        if (duration.TotalMinutes < 30)
            return Result.Failure<string>("Reservation must be at least 30 minutes long.", 400);

        if (finalStartTime >= finalEndTime)
            return Result.Failure<string>("End time must be after start time.", 400);

        // Validate status transitions (state machine)
        if (
            !string.IsNullOrEmpty(dto.Status)
            && Enum.TryParse<ReservationStatus>(dto.Status, out var parsedStatus)
        )
        {
            var validTransitions = new Dictionary<ReservationStatus, List<ReservationStatus>>
            {
                {
                    ReservationStatus.Pending,
                    new() { ReservationStatus.Confirmed, ReservationStatus.Cancelled }
                },
                {
                    ReservationStatus.Confirmed,
                    new() { ReservationStatus.Cancelled, ReservationStatus.Completed }
                },
                { ReservationStatus.Completed, new() },
                { ReservationStatus.Cancelled, new() },
            };

            if (!validTransitions[reservation.Status].Contains(parsedStatus))
                return Result.Failure<string>(
                    $"Invalid status transition from {reservation.Status} to {parsedStatus}.",
                    400
                );

            reservation.Status = parsedStatus;
        }

        // Check for table conflicts if table is being changed
        if (dto.TableId.HasValue && dto.TableId.Value != reservation.TableId)
        {
            // Ensure date is UTC for PostgreSQL compatibility
            var finalDate = dto.Date.HasValue ? DateTime.SpecifyKind(dto.Date.Value, DateTimeKind.Utc) : reservation.Date;
            var hasConflict = await _reservationRepository.ExistsOverlappingReservationAsync(
                dto.TableId.Value,
                finalDate,
                finalStartTime,
                finalEndTime,
                reservation.Id,
                ct
            );

            if (hasConflict)
                return Result.Failure<string>(
                    "The selected table has conflicting reservations for the specified time.",
                    409
                );
        }

        reservation.TableId = dto.TableId ?? reservation.TableId;
        reservation.Date = dto.Date.HasValue ? DateTime.SpecifyKind(dto.Date.Value, DateTimeKind.Utc) : reservation.Date;
        reservation.StartTime = finalStartTime;
        reservation.EndTime = finalEndTime;
        reservation.NumberOfGuests = dto.NumberOfGuests ?? reservation.NumberOfGuests;
        reservation.Notes = dto.Notes ?? reservation.Notes;

        if (basePrice.HasValue && totalPrice.HasValue)
        {
            reservation.BasePrice = basePrice.Value;
            reservation.TotalPrice = totalPrice.Value;
        }

        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation, ct);

        await _reservationRepository.GetByIdAsync(dto.Id, ct);
        return Result.Success("Reservation updated successfully.");
    }

    public async Task<Result<string>> CancelAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null)
            return Result.Failure<string>("Reservation not found.", 404);

        if (reservation.Status == ReservationStatus.Cancelled)
            return Result.Failure<string>("Reservation is already cancelled.", 400);

        // Prevent cancelling completed reservations
        if (reservation.Status == ReservationStatus.Completed)
            return Result.Failure<string>("Cannot cancel a completed reservation.", 400);

        // Prevent cancelling past reservations
        var reservationEndDateTime = reservation.Date + reservation.EndTime;
        if (DateTime.UtcNow > reservationEndDateTime)
            return Result.Failure<string>("Cannot cancel a past reservation.", 400);

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation, ct);
        return Result.Success($"Reservation #{id} has been cancelled.");
    }
}

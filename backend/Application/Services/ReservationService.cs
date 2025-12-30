using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.DTOs.Table;
using RestaurantReservation.Application.DTOs.User;

namespace RestaurantReservation.Application.Services;

public class ReservationService(
    IReservationRepository reservationRepository
) : IReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;

    public async Task<(IEnumerable<ReservationDto> Data, PaginationMetadata Pagination)> GetAllAsync(
        ReservationQueryParams queryParams,
        CancellationToken ct = default)
    {
        var query = _reservationRepository.Query();

        if (queryParams.ClientId.HasValue)
            query = query.Where(r => r.ClientId == queryParams.ClientId.Value);

        if (queryParams.TableId.HasValue)
            query = query.Where(r => r.TableId == queryParams.TableId.Value);

        if (queryParams.Date.HasValue)
            query = query.Where(r => r.Date == queryParams.Date.Value);

        if (queryParams.StartTime.HasValue)
            query = query.Where(r => r.StartTime >= queryParams.StartTime.Value);

        if (queryParams.EndTime.HasValue)
            query = query.Where(r => r.EndTime <= queryParams.EndTime.Value);

        if (!string.IsNullOrEmpty(queryParams.Status))
            query = query.Where(r => r.Status.ToString() == queryParams.Status);

        var totalCount = await query.CountAsync(ct);

        var skipNumber = (queryParams.Page - 1) * queryParams.PageSize;
        var data = await query
            .Skip(skipNumber)
            .Take(queryParams.PageSize)
            .Select(r => new ReservationDto(
                r.Id,
                new ClientDto(
                    r.Client.Id,
                    r.Client.FirstName,
                    r.Client.LastName,
                    r.Client.Email,
                    r.Client.Phone,
                    r.Client.Status.ToString(),
                    r.Client.Reservations.Count,
                    r.Client.CreatedAt),
                new TableDto(
                    r.Table.Id,
                    r.Table.Code,
                    r.Table.Capacity,
                    r.Table.Location,
                    r.Table.TableTypeId,
                    r.Table.Status.ToString()),
                r.Date,
                r.StartTime,
                r.EndTime,
                r.NumberOfGuests,
                r.BasePrice,
                r.TotalPrice,
                r.Status.ToString(),
                r.Notes,
                new UserDto(
                    r.User.Id,
                    r.User.Username,
                    r.User.Email,
                    r.User.Role.ToString(),
                    r.User.Status.ToString())
            ))
            .ToListAsync(ct);

        var pagination = new PaginationMetadata
        {
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize)
        };

        return (data, pagination);
    }

    public async Task<Result<ReservationDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null)
            return Result.Failure<ReservationDto>("Reservation not found", 404);

        var reservationDto = new ReservationDto(
            reservation.Id,
            new ClientDto(
                reservation.Client.Id,
                reservation.Client.FirstName,
                reservation.Client.LastName,
                reservation.Client.Email,
                reservation.Client.Phone,
                reservation.Client.Status.ToString(),
                reservation.Client.Reservations.Count,
                reservation.Client.CreatedAt),
            new TableDto(
                reservation.Table.Id,
                reservation.Table.Code,
                reservation.Table.Capacity,
                reservation.Table.Location,
                reservation.Table.TableTypeId,
                reservation.Table.Status.ToString()),
            reservation.Date,
            reservation.StartTime,
            reservation.EndTime,
            reservation.NumberOfGuests,
            reservation.BasePrice,
            reservation.TotalPrice,
            reservation.Status.ToString(),
            reservation.Notes,
            new UserDto(
                reservation.User.Id,
                reservation.User.Username,
                reservation.User.Email,
                reservation.User.Role.ToString(),
                reservation.User.Status.ToString())
        );
        return Result.Success(reservationDto);
    }

    public async Task<Result<Reservation>> CreateAsync(
        CreateReservationDto dto, int createdByUserId,
        decimal basePrice, decimal totalPrice, CancellationToken ct = default)
    {
        var duration = dto.EndTime - dto.StartTime;
        if (duration.TotalMinutes < 30)
            return Result.Failure<Reservation>("Reservation must be at least 30 minutes long.", 400);

        var reservationDateTime = dto.Date.Date + dto.StartTime;
        if (reservationDateTime <= DateTime.Now)
            return Result.Failure<Reservation>("Reservation date must be in the future.", 400);

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

    public async Task<Result<string>> UpdateAsync
        (UpdateReservationDto dto, decimal? basePrice, decimal? totalPrice, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(dto.Id, ct);
        if (reservation is null)
            return Result.Failure<string>("Reservation not found.", 404);
        
        var finalStartTime = dto.StartTime ?? reservation.StartTime;
        var finalEndTime = dto.EndTime ?? reservation.EndTime;

        var duration = finalEndTime - finalStartTime;
        if (duration.TotalMinutes < 30)
            return Result.Failure<string>("Reservation must be at least 30 minutes long.", 400);

        if (finalStartTime >= finalEndTime)
            return Result.Failure<string>("End time must be after start time.", 400);

        reservation.TableId = dto.TableId ?? reservation.TableId;
        reservation.Date = dto.Date ?? reservation.Date;
        reservation.StartTime = finalStartTime;
        reservation.EndTime = finalEndTime;
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

        var updatedReservation = await _reservationRepository.GetByIdAsync(dto.Id, ct);
        return Result.Success<string>("Reservation updated successfully.");
    }

    public async Task<Result<string>> CancelAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null)
            return Result.Failure<string>("Reservation not found.", 404);

        if (reservation.Status == ReservationStatus.Cancelled)
            return Result.Failure<string>("Reservation is already cancelled.", 400);

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation, ct);
        return Result.Success<string>($"Reservation #{id} has been cancelled.");
    }
}
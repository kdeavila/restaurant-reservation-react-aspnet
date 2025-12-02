using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Domain.Entities;

namespace RestaurantReservation.Application.Interfaces.Services;

public interface IReservationService
{
    Task<Result<ReservationDto>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(IEnumerable<ReservationDto> Data, PaginationMetadata Pagination)> GetAllAsync(
        ReservationQueryParams queryParams, CancellationToken ct = default);

    Task<Result<Reservation>> CreateAsync(
        CreateReservationDto dto, int createdByUserId, decimal basePrice,
        decimal totalPrice, CancellationToken ct = default);

    Task<Result<string>> UpdateAsync
    (UpdateReservationDto dto, decimal? basePrice,
        decimal? totalPrice, CancellationToken ct = default);

    Task<Result<string>> CancelAsync(int id, CancellationToken ct = default);
}
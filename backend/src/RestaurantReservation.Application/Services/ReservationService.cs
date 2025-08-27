using RestaurantReservation.Application.Common.Helpers;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class ReservationService(
    IReservationRepository reservationRepository,
    ITableRepository tableRepository,
    IClientRepository clientRepository,
    IPricingService pricingService
) : IReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITableRepository _tableRepository = tableRepository;
    private readonly IPricingService _pricingService = pricingService;

    public async Task<ReservationDto?> CreateReservationAsync(CreateReservationDto dto, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(dto.ClientId, ct);
        if (client is null || client.Status != ClientStatus.Active) return null;

        var table = await _tableRepository.GetByIdAsync(dto.TableId, ct);
        if (table is null || table.Status != TableStatus.Active) return null;

        var overlap =
            await _reservationRepository.ExistsOverlappingReservationAsync(dto.TableId, dto.Date, dto.StartTime,
                dto.EndTime, ct);
        if (overlap) return null;

        var (basePrice, totalPrice) = await _pricingService.CalculatePriceAsync(
            dto.TableId,
            dto.Date,
            dto.StartTime,
            dto.EndTime,
            ct
        );

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

        return new ReservationDto(
            reservation.Id,
            reservation.ClientId,
            $"{client.FirstName} {client.LastName}",
            reservation.TableId,
            table.Code,
            reservation.Date,
            reservation.StartTime,
            reservation.EndTime,
            reservation.NumberOfGuests,
            reservation.BasePrice,
            reservation.TotalPrice,
            reservation.Status.ToString(),
            reservation.Notes
        );
    }

    public async Task<ReservationDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null) return null;

        return new ReservationDto(
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

    public async Task<bool> UpdateReservationAsync(UpdateReservationDto dto, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(dto.Id, ct);
        if (reservation is null) return false;

        reservation.TableId = dto.TableId ?? reservation.TableId;
        reservation.Date = dto.Date ?? reservation.Date;
        reservation.StartTime = dto.StartTime ?? reservation.StartTime;
        reservation.EndTime = dto.EndTime ?? reservation.EndTime;
        reservation.NumberOfGuests = dto.NumberOfGuests ?? reservation.NumberOfGuests;
        reservation.Notes = dto.Notes ?? reservation.Notes;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<ReservationStatus>(dto.Status, out var parsed))
            reservation.Status = parsed;

        if (dto.TableId.HasValue || dto.Date.HasValue || dto.StartTime.HasValue || dto.EndTime.HasValue)
        {
            var (basePrice, totalPrice) = await _pricingService.CalculatePriceAsync(
                reservation.TableId,
                reservation.Date,
                reservation.StartTime,
                reservation.EndTime,
                ct
            );

            reservation.BasePrice = basePrice;
            reservation.TotalPrice = totalPrice;
        }

        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation, ct);
        return true;
    }

    public async Task<bool> CancelReservationAsync(int id, CancellationToken ct = default)
    {
        {
            var reservation = await _reservationRepository.GetByIdAsync(id, ct);
            if (reservation is null) return false;

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _reservationRepository.UpdateAsync(reservation, ct);
            return true;
        }
    }

    public async Task<bool> DeleteReservationAsync(int id, CancellationToken ct = default)
    {
        {
            var reservation = await _reservationRepository.GetByIdAsync(id, ct);
            if (reservation is null) return false;

            await _reservationRepository.DeleteAsync(reservation.Id, ct);
            return true;
        }
    }
}
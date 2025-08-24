using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class ReservationService(
    IReservationRepository reservationRepository,
    ITableRepository tableRepository,
    IClientRepository clientRepository
) : IReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly ITableRepository _tableRepository = tableRepository;

    public async Task<ReservationDto?> CreateReservationAsync(CreateReservationDto dto, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(dto.ClientId, ct);
        var table = await _tableRepository.GetByIdAsync(dto.TableId, ct);
        if (client is null || table is null) return null;

        // TODO: Add validation for overlapping reservations here
        // TODO: apply surcharge rules
        var reservation = new Reservation()
        {
            ClientId = dto.ClientId,
            TableId = dto.TableId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            NumberOfGuests = dto.NumberOfGuests,
            BasePrice = table.TableType.BasePricePerHour,
            TotalPrice = table.TableType.BasePricePerHour,
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

        reservation.Date = dto.Date ?? reservation.Date;
        reservation.StartTime = dto.StartTime ?? reservation.StartTime;
        reservation.EndTime = dto.EndTime ?? reservation.EndTime;
        reservation.NumberOfGuests = dto.NumberOfGuests ?? reservation.NumberOfGuests;
        reservation.Notes = dto.Notes ?? reservation.Notes;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<ReservationStatus>(dto.Status, out var parsed))
            reservation.Status = parsed;

        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation, ct);
        return true;
    }

    public async Task<bool> DeleteReservationAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id, ct);
        if (reservation is null) return false;

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation, ct);
        return true;
    }
}
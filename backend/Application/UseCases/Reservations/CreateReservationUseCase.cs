using Mapster;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Reservation;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.UseCases.Reservations;

public class CreateReservationUseCase(
    IClientRepository clientRepository,
    ITableRepository tableRepository,
    IReservationRepository reservationRepository,
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IReservationService reservationService,
    IPricingService pricingService,
    ITableTypeRepository tableTypeRepository)
{
   private readonly IClientRepository _clientRepository = clientRepository;
   private readonly ITableRepository _tableRepository = tableRepository;
   private readonly IReservationRepository _reservationRepository = reservationRepository;
   private readonly ICurrentUserService _currentUserService = currentUserService;
   private readonly IPricingService _pricingService = pricingService;
   private readonly IReservationService _reservationService = reservationService;
   private readonly IUserRepository _userRepository = userRepository;
   private readonly ITableTypeRepository _tableTypeRepository = tableTypeRepository;

   public async Task<Result<ReservationDto>> ExecuteAsync(
       CreateReservationDto dto, CancellationToken ct = default)
   {
      if (_currentUserService.UserId is not { } userId)
         return Result.Failure<ReservationDto>("User not authenticated.", 401);

      var user = await _userRepository.GetByIdAsync(userId, ct);
      if (user is null)
         return Result.Failure<ReservationDto>("User not found.", 404);

      var client = await _clientRepository.GetByIdAsync(dto.ClientId, ct);
      if (client is null || client.Status != ClientStatus.Active)
         return Result.Failure<ReservationDto>("Client not found or inactive.", 404);

      var table = await _tableRepository.GetByIdAsync(dto.TableId, ct);
      if (table is null || table.Status != TableStatus.Active)
         return Result.Failure<ReservationDto>("Table not found or inactive.", 404);

      var tableType = await _tableTypeRepository.GetByIdAsync(table.TableTypeId, ct);
      if (tableType is null || !tableType.IsActive)
         return Result.Failure<ReservationDto>("Table type is not available.", 400);

      if (dto.NumberOfGuests > table.Capacity)
         return Result.Failure<ReservationDto>
             ("The number of guests exceeds the table's capacity", 400);

      var overlap =
          await _reservationRepository.ExistsOverlappingReservationAsync
              (dto.TableId, dto.Date, dto.StartTime, dto.EndTime, null, ct);
      if (overlap)
         return Result.Failure<ReservationDto>
             ("Table is already booked for the selected time.", 409);

      var priceResult = await _pricingService.CalculatePriceAsync(
          dto.TableId,
          dto.Date,
          dto.StartTime,
          dto.EndTime,
          ct
      );
      if (priceResult.IsFailure)
         return Result.Failure<ReservationDto>(priceResult.Error, priceResult.StatusCode);
      var (basePrice, totalPrice) = priceResult.Value;

      var result = await
          _reservationService.CreateAsync(dto, userId, basePrice, totalPrice, ct);

      if (result.IsFailure)
         return Result.Failure<ReservationDto>(result.Error, priceResult.StatusCode);

      var reservation = result.Value;
      
      // Fetch the reservation with all navigations loaded
      var createdReservation = await _reservationRepository.GetByIdAsync(reservation.Id, ct);
      if (createdReservation is null)
         return Result.Failure<ReservationDto>("Failed to retrieve created reservation.", 500);

      var reservationDto = createdReservation.Adapt<ReservationDto>();

      return Result.Success(reservationDto);
   }
}
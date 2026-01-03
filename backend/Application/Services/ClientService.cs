using Mapster;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.Common.Pagination;
using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class ClientService(IClientRepository clientRepository) : IClientService
{
   private readonly IClientRepository _clientRepository = clientRepository;

   public async Task<Result<ClientDto>> CreateAsync(CreateClientDto dto, CancellationToken ct = default)
   {
      var emailExists = await _clientRepository.EmailExistsAsync(dto.Email, ct);
      if (emailExists) return Result.Failure<ClientDto>("Email address is already in use.", 409);

      var client = new Client()
      {
         FirstName = dto.FirstName,
         LastName = dto.LastName,
         Email = dto.Email,
         Phone = dto.Phone ?? string.Empty,
         Status = ClientStatus.Active,
         CreatedAt = DateTime.UtcNow
      };

      await _clientRepository.AddAsync(client, ct);
      var clientDto = client.Adapt<ClientDto>();

      return Result.Success(clientDto);
   }

   public async Task<Result<ClientDto>> GetByIdAsync(int id, CancellationToken ct = default)
   {
      var client = await _clientRepository.GetByIdAsync(id, ct);
      if (client is null) return Result.Failure<ClientDto>("Client not found", 404);

      var clientDto = client.Adapt<ClientDto>();
      return Result.Success(clientDto);
   }

   public async Task<(IEnumerable<ClientDto> Data, PaginationMetadata Pagination)> GetAllAsync(
       ClientQueryParams queryParams, CancellationToken ct = default)
   {
      var query = _clientRepository.Query();

      if (!string.IsNullOrWhiteSpace(queryParams.FirstName))
      {
         var term = queryParams.FirstName.Trim().ToLower();
         query = query.Where(c => c.FirstName.ToLower().Contains(term));
      }

      if (!string.IsNullOrWhiteSpace(queryParams.LastName))
      {
         var term = queryParams.LastName.Trim().ToLower();
         query = query.Where(c => c.LastName.ToLower().Contains(term));
      }

      if (!string.IsNullOrWhiteSpace(queryParams.Email))
      {
         var term = queryParams.Email.Trim().ToLower();
         query = query.Where(c => c.Email.ToLower().Contains(term));
      }

      if (!string.IsNullOrWhiteSpace(queryParams.Phone))
      {
         var term = queryParams.Phone.Trim().ToLower();
         query = query.Where(c => c.Phone != null && c.Phone.ToLower().Contains(term));
      }

      var totalCount = await query.CountAsync(ct);
      var skipNumber = (queryParams.Page - 1) * queryParams.PageSize;

      var clientsPage = await query
          .OrderBy(c => c.Id)
          .Skip(skipNumber)
          .Take(queryParams.PageSize)
          .ToListAsync(ct);

      var data = clientsPage.Select(client => client.Adapt<ClientDto>());

      var pagination = new PaginationMetadata
      {
         Page = queryParams.Page,
         PageSize = queryParams.PageSize,
         TotalCount = totalCount,
         TotalPages = (int)Math.Ceiling(totalCount / (double)queryParams.PageSize)
      };

      return (data, pagination);
   }

   public async Task<Result> UpdateAsync(UpdateClientDto dto, CancellationToken ct = default)
   {
      var client = await _clientRepository.GetByIdAsync(dto.Id, ct);
      if (client is null) return Result.Failure("Client not found", 404);

      if (!string.IsNullOrEmpty(dto.Email))
      {
         var emailExists = await _clientRepository.EmailExistsForOthersClients(dto.Email, dto.Id, ct);
         if (emailExists) return Result.Failure("Email address is already in use", 409);

         client.Email = dto.Email;
      }

      client.FirstName = dto.FirstName ?? client.FirstName;
      client.LastName = dto.LastName ?? client.LastName;
      client.Phone = dto.Phone ?? client.Phone;

      if (!string.IsNullOrEmpty(dto.Status) &&
          Enum.TryParse<ClientStatus>(dto.Status, true, out var parsed))
         client.Status = parsed;

      await _clientRepository.UpdateAsync(client, ct);
      return Result.Success();
   }

   public async Task<Result<string>> DeactivateAsync(int id, CancellationToken ct = default)
   {
      var client = await _clientRepository.GetByIdAsync(id, ct);
      if (client is null)
         return Result.Failure<string>("Client not found", 404);

      var futureReservations = client.Reservations
          .Where(r => r.Status != ReservationStatus.Cancelled
                      && r.Status != ReservationStatus.Completed &&
                      (r.Date > DateTime.UtcNow.Date ||
                       (r.Date == DateTime.UtcNow.Date &&
                        r.StartTime > DateTime.UtcNow.TimeOfDay))
          ).ToList();

      if (futureReservations.Any())
         return Result.Failure<string>(
             $"Cannot deactivate client. They have {futureReservations.Count} future reservation(s). " +
             $"Please cancel or reassign the reservations first.",
             409);

      client.Status = ClientStatus.Inactive;
      await _clientRepository.UpdateAsync(client, ct);

      return Result.Success<string>("Client deactivated successfully.");
   }
}
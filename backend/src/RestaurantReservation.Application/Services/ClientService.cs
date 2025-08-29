using RestaurantReservation.Application.Common;
using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class ClientService(IClientRepository clientRepository) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task<Result<ClientDto>> CreateClientAsync(CreateClientDto dto, CancellationToken ct = default)
    {
        var emailExists = await _clientRepository.EmailExistsAsync(dto.Email, ct);
        if (emailExists) return Result.Failure<ClientDto>("Email already in use");

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
        var clientDto = new ClientDto(client.Id, client.FirstName, client.LastName, client.Email, client.Phone,
            client.Status.ToString());
        return Result.Success(clientDto);
    }

    public async Task<Result<ClientDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client is null) return Result.Failure<ClientDto>("Client not found");

        var clientDto = new ClientDto(client.Id, client.FirstName, client.LastName, client.Email, client.Phone,
            client.Status.ToString());
        return Result.Success(clientDto);
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync(CancellationToken ct = default)
    {
        var clients = await _clientRepository.GetAllAsync(ct);
        return clients.Select(c => new ClientDto(c.Id, c.FirstName, c.LastName, c.Email, c.Phone,
            c.Status.ToString()));
    }

    public async Task<Result> UpdateClientAsync(UpdateClientDto dto, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(dto.Id, ct);
        if (client is null) return Result.Failure("Client not found");

        if (!string.IsNullOrEmpty(dto.Email))
        {
            var emailExists = await _clientRepository.EmailExistsForOthersClients(dto.Email, dto.Id, ct);
            if (emailExists) return Result.Failure("Email already in use");

            client.Email = dto.Email;
        }

        client.FirstName = dto.FirstName ?? client.FirstName;
        client.LastName = dto.LastName ?? client.LastName;
        client.Phone = dto.Phone ?? client.Phone;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<ClientStatus>(dto.Status, true, out var parsed))
            client.Status = parsed;

        await _clientRepository.UpdateAsync(client, ct);
        return Result.Success();
    }

    public async Task<Result> DeleteClientAsync(int id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client is null) return Result.Failure("Client not found");

        await _clientRepository.DeleteAsync(client.Id, ct);
        return Result.Success();
    }
}
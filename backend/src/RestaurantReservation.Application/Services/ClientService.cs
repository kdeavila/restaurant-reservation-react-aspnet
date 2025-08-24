using RestaurantReservation.Application.DTOs.Client;
using RestaurantReservation.Application.Interfaces.Repositories;
using RestaurantReservation.Application.Interfaces.Services;
using RestaurantReservation.Domain.Entities;
using RestaurantReservation.Domain.Enums;

namespace RestaurantReservation.Application.Services;

public class ClientService(IClientRepository clientRepository) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task<ClientDto?> CreateClientAsync(CreateClientDto dto, CancellationToken ct = default)
    {
        var emailExists = await _clientRepository.EmailExistsAsync(dto.Email, ct);
        if (emailExists) return null;

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
        return new ClientDto(client.Id, client.FirstName, client.LastName, client.Email, client.Phone,
            client.Status.ToString());
    }

    public async Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client is null) return null;

        return new ClientDto(client.Id, client.FirstName, client.LastName, client.Email, client.Phone,
            client.Status.ToString());
    }

    public async Task<IEnumerable<ClientDto>> GetAllAsync(CancellationToken ct = default)
    {
        var clients = await _clientRepository.GetAllAsync(ct);
        return clients.Select(c => new ClientDto(c.Id, c.FirstName, c.LastName, c.Email, c.Phone,
            c.Status.ToString()));
    }

    public async Task<bool> UpdateClientAsync(UpdateClientDto dto, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(dto.Id, ct);
        if (client is null) return false;

        if (!string.IsNullOrEmpty(dto.Email))
        {
            // Check if the user exists
            var emailExists = await _clientRepository.EmailExistsForOthersClients(dto.Email, dto.Id, ct);
            if (emailExists) return false;

            client.Email = dto.Email;
        }

        client.FirstName = dto.FirstName ?? client.FirstName;
        client.LastName = dto.LastName ?? client.LastName;
        client.Phone = dto.Phone ?? client.Phone;

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<ClientStatus>(dto.Status, true, out var parsed))
            client.Status = parsed;

        await _clientRepository.UpdateAsync(client, ct);
        return true;
    }

    public async Task<bool> DeleteClientAsync(int id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct);
        if (client is null) return false;

        await _clientRepository.DeleteAsync(client.Id, ct);
        return true;
    }
}
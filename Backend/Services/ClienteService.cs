namespace Backend.Services;

using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<List<ClienteResponse>> ListarAsync()
    {
        var clientes = await _clienteRepository.ListarAsync();

        return clientes.Select(cliente => new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email
        }).ToList();
    }

    public async Task<ClienteResponse?> BuscarPorIdAsync(int id)
    {
        var cliente = await _clienteRepository.BuscarPorIdAsync(id);

        if (cliente == null)
        {
            return null;
        }

        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email
        };
    }

    public async Task<ClienteResponse> CriarAsync(
        CriarClienteRequest request
    )
    {
        var cliente = new Cliente
        {
            Nome = request.Nome,
            Email = request.Email,
            CriadoEm = DateTime.UtcNow
        };

        var clienteCriado =
            await _clienteRepository.CriarAsync(cliente);

        return new ClienteResponse
        {
            Id = clienteCriado.Id,
            Nome = clienteCriado.Nome,
            Email = clienteCriado.Email
        };
    }

    public async Task<ClienteResponse?> AtualizarAsync(
        int id,
        CriarClienteRequest request
    )
    {
        var cliente = await _clienteRepository.BuscarPorIdAsync(id);

        if (cliente == null)
        {
            return null;
        }

        cliente.Nome = request.Nome;
        cliente.Email = request.Email;

        var clienteAtualizado =
            await _clienteRepository.AtualizarAsync(cliente);

        return new ClienteResponse
        {
            Id = clienteAtualizado.Id,
            Nome = clienteAtualizado.Nome,
            Email = clienteAtualizado.Email
        };
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var cliente = await _clienteRepository.BuscarPorIdAsync(id);

        if (cliente == null)
        {
            return false;
        }

        await _clienteRepository.ExcluirAsync(cliente);

        return true;
    }
}
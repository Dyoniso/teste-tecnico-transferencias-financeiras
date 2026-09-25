using Backend.DTOs.Requests;
using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IClienteService
{
    Task<List<ClienteResponse>> ListarAsync();

    Task<ClienteResponse?> BuscarPorIdAsync(int id);

    Task<ClienteResponse> CriarAsync(
        CriarClienteRequest request
    );

    Task<ClienteResponse?> AtualizarAsync(
        int id,
        CriarClienteRequest request
    );

    Task<bool> ExcluirAsync(int id);
}
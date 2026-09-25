namespace Backend.Repositories.Interfaces;

using Backend.Models;

public interface IClienteRepository
{
    Task<List<Cliente>> ListarAsync();

    Task<Cliente?> BuscarPorIdAsync(int id);

    Task<Cliente> CriarAsync(Cliente cliente);

    Task<Cliente> AtualizarAsync(Cliente cliente);

    Task ExcluirAsync(Cliente cliente);
}
namespace Backend.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Data;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cliente>> ListarAsync()
    {
        return await _context.Clientes.ToListAsync();
    }
    
    public async Task<Cliente?> BuscarPorIdAsync(int id)
    {
        return await _context.Clientes.FindAsync(id);
    }

    public async Task<Cliente> CriarAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<Cliente> AtualizarAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task ExcluirAsync(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }
}
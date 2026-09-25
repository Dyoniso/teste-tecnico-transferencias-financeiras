using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class AccountRepository :
    IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<Account?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Accounts
            .Include(x => x.Person)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
}
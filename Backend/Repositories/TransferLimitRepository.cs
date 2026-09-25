using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class TransferLimitRepository :
    ITransferLimitRepository
{
    private readonly AppDbContext _context;

    public TransferLimitRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<TransferLimit?>
        GetByPersonIdAsync(
            int personId,
            CancellationToken cancellationToken = default)
    {
        return _context.TransferLimits
            .FirstOrDefaultAsync(
                x => x.PersonId == personId,
                cancellationToken);
    }
}
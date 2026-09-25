using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class OutboxRepository :
    IOutboxRepository
{
    private readonly AppDbContext _context;

    public OutboxRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task<List<OutboxMessage>>
        GetPendingAsync(
            int limit,
            CancellationToken cancellationToken = default)
    {
        return _context.OutboxMessages
            .Where(x =>
                x.ProcessedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public void Add(
        OutboxMessage message)
    {
        _context.OutboxMessages.Add(
            message);
    }
}
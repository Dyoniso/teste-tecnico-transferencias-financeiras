using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> Persons =>
        Set<Person>();

    public DbSet<Account> Accounts =>
        Set<Account>();

    public DbSet<Transfer> Transfers =>
        Set<Transfer>();

    public DbSet<TransferAttempt> TransferAttempts =>
        Set<TransferAttempt>();

    public DbSet<TransferLimit> TransferLimits =>
        Set<TransferLimit>();

    public DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
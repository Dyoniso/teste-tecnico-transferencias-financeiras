using Backend.Models.Enums;

namespace Backend.Models;

public class Account
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public decimal Balance { get; set; }

    public decimal OverdraftLimit { get; set; }

    public AccountStatus Status { get; set; }

    public uint Version { get; set; }

    public Person Person { get; set; } = null!;
}
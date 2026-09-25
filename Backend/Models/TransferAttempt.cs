namespace Backend.Models;

public class TransferAttempt
{
    public long Id { get; set; }

    public int AccountId { get; set; }

    public Guid? TransferId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool Success { get; set; }

    public string? FailureReason { get; set; }

    public Account Account { get; set; } = null!;
}
using Backend.Models.Enums;

namespace Backend.Models;

public class Transfer
{
    public Guid Id { get; set; }

    public int SourceAccountId { get; set; }

    public int DestinationAccountId { get; set; }

    public decimal Amount { get; set; }

    public TransferStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? DispatchRequestedAt { get; set; }

    public string? FailureReason { get; set; }

    public string? IdempotencyKey { get; set; }

    public uint Version { get; set; }

    public Account SourceAccount { get; set; } = null!;

    public Account DestinationAccount { get; set; } = null!;
}
namespace Backend.DTOs.Responses;

public class TransferResponse
{
    public Guid Id { get; set; }

    public int SourceAccountId { get; set; }

    public int DestinationAccountId { get; set; }

    public string? SourceAccountName { get; set; }

    public string? DestinationAccountName { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? FailureReason { get; set; }
}
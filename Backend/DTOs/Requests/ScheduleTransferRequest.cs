using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class ScheduleTransferRequest
{
    [Required]
    public int SourceAccountId { get; set; }

    [Required]
    public int DestinationAccountId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }
}
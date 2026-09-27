using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

namespace Backend.DTOs.Requests;

public class UpdateAccountRequest
{
    [Range(1, int.MaxValue)]
    public int PersonId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OverdraftLimit { get; set; }

    [Required]
    public AccountStatus Status { get; set; }
}
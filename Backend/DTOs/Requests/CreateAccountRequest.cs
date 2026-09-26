using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

namespace Backend.DTOs.Requests;

public class CreateAccountRequest
{
    [Required]
    public int PersonId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OverdraftLimit { get; set; }

    [Required]
    public AccountStatus Status { get; set; }
}
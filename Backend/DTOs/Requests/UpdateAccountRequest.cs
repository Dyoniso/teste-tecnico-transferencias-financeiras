using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

namespace Backend.DTOs.Requests;

public class UpdateAccountRequest
{
    [Range(0, double.MaxValue)]
    public decimal OverdraftLimit { get; set; }

    [Required]
    public AccountStatus Status { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Requests;

public class UpdatePersonRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } =
        string.Empty;

    [MaxLength(20)]
    public string? Document { get; set; }

    public DateOnly? BirthDate { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(10)]
    public string? ZipCode { get; set; }

    [MaxLength(150)]
    public string? Street { get; set; }

    [MaxLength(20)]
    public string? Number { get; set; }

    [MaxLength(100)]
    public string? Complement { get; set; }

    [MaxLength(100)]
    public string? Neighborhood { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(2)]
    public string? State { get; set; }
}
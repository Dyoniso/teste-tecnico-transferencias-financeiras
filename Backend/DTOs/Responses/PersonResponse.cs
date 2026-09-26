namespace Backend.DTOs.Responses;

public class PersonResponse
{
    public int Id { get; set; }

    public string Name { get; set; } =
        string.Empty;

    public string? Document { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? ZipCode { get; set; }

    public string? Street { get; set; }

    public string? Number { get; set; }

    public string? Complement { get; set; }

    public string? Neighborhood { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public int? AccountId { get; set; }
}
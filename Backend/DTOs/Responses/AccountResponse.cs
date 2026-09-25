namespace Backend.DTOs.Responses;

public class AccountResponse
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public decimal OverdraftLimit { get; set; }

    public decimal AvailableBalance { get; set; }

    public string Status { get; set; } = string.Empty;
}
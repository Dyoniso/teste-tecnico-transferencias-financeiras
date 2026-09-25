namespace Backend.DTOs.Requests;

public class CriarClienteRequest
{
    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
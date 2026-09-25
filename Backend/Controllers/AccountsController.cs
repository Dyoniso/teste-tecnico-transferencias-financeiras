using Backend.DTOs.Responses;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController :
    ControllerBase
{
    private readonly IAccountService
        _accountService;

    public AccountsController(
        IAccountService accountService)
    {
        _accountService =
            accountService;
    }

    /// <summary>
    /// Consulta uma conta pelo identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(AccountResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult>
        GetById(
            int id,
            CancellationToken cancellationToken)
    {
        var account =
            await _accountService
                .GetByIdAsync(
                    id,
                    cancellationToken);

        return Ok(
            account);
    }
}
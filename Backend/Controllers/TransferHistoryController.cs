using Backend.DTOs.Responses;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/transfer-history")]
public class TransferHistoryController :
    ControllerBase
{
    private readonly ITransferService
        _transferService;

    public TransferHistoryController(
        ITransferService transferService)
    {
        _transferService =
            transferService;
    }

    /// <summary>
    /// Lista transferências enviadas, recebidas e agendadas de uma conta.
    /// </summary>
    [HttpGet("accounts/{accountId:int}")]
    [ProducesResponseType(
        typeof(IReadOnlyList<TransferResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAccountId(
        int accountId,
        CancellationToken cancellationToken)
    {
        var transfers =
            await _transferService
                .GetHistoryByAccountIdAsync(
                    accountId,
                    cancellationToken);

        return Ok(transfers);
    }
}
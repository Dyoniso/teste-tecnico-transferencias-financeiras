using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/transfers")]
public class TransfersController :
    ControllerBase
{
    private readonly ITransferService
        _transferService;

    public TransfersController(
        ITransferService transferService)
    {
        _transferService =
            transferService;
    }

    /// <summary>
    /// Realiza uma transferência imediatamente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(TransferResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create(
        [FromBody]
        CreateTransferRequest request,

        [FromHeader(
            Name = "Idempotency-Key")]
        string? idempotencyKey,

        CancellationToken cancellationToken)
    {
        var transfer =
            await _transferService
                .TransferAsync(
                    request,
                    idempotencyKey,
                    cancellationToken);

        return Ok(
            transfer);
    }

    /// <summary>
    /// Agenda uma transferência para uma data futura.
    /// </summary>
    [HttpPost("scheduled")]
    [ProducesResponseType(
        typeof(TransferResponse),
        StatusCodes.Status201Created)]
    public async Task<IActionResult> Schedule(
        [FromBody]
        ScheduleTransferRequest request,

        [FromHeader(
            Name = "Idempotency-Key")]
        string? idempotencyKey,

        CancellationToken cancellationToken)
    {
        var transfer =
            await _transferService
                .ScheduleAsync(
                    request,
                    idempotencyKey,
                    cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = transfer.Id
            },
            transfer);
    }

    /// <summary>
    /// Consulta uma transferência.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(TransferResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var transfer =
            await _transferService
                .GetByIdAsync(
                    id,
                    cancellationToken);

        return Ok(
            transfer);
    }

    /// <summary>
    /// Cancela uma transferência agendada.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _transferService
            .CancelAsync(
                id,
                cancellationToken);

        return NoContent();
    }
}
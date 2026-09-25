namespace Backend.Controllers;

using Microsoft.AspNetCore.Mvc;
using Backend.DTOs.Requests;
using Backend.Services.Interfaces;
using Backend.DTOs.Responses;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>
    /// Lista todos os clientes cadastrados.
    /// </summary>
    /// <returns>Lista de clientes.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClienteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var clientes = await _clienteService.ListarAsync();

        return Ok(clientes);
    }

    /// <summary>
    /// Cria um novo cliente.
    /// </summary>
    /// <param name="request">Dados necessários para criação do cliente.</param>
    /// <returns>Cliente criado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarClienteRequest request
    )
    {
        var cliente = await _clienteService.CriarAsync(request);

        return CreatedAtAction(
            nameof(Listar),
            new { id = cliente.Id },
            cliente
        );
    }
}
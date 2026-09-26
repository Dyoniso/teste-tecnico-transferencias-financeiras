using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/persons")]
public class PersonsController :
    ControllerBase
{
    private readonly IPersonService
        _personService;

    public PersonsController(
        IPersonService personService)
    {
        _personService =
            personService;
    }

    /// <summary>
    /// Lista todas as pessoas cadastradas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(List<PersonResponse>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var persons =
            await _personService
                .GetAllAsync(
                    cancellationToken);

        return Ok(
            persons);
    }

    /// <summary>
    /// Consulta uma pessoa pelo identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(PersonResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var person =
            await _personService
                .GetByIdAsync(
                    id,
                    cancellationToken);

        return Ok(
            person);
    }

    /// <summary>
    /// Registra uma nova pessoa.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(PersonResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody]
        CreatePersonRequest request,
        CancellationToken cancellationToken)
    {
        var person =
            await _personService
                .CreateAsync(
                    request,
                    cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = person.Id
            },
            person);
    }

    /// <summary>
    /// Atualiza os dados de uma pessoa.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(PersonResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody]
        UpdatePersonRequest request,
        CancellationToken cancellationToken)
    {
        var person =
            await _personService
                .UpdateAsync(
                    id,
                    request,
                    cancellationToken);

        return Ok(
            person);
    }

    /// <summary>
    /// Exclui uma pessoa.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await _personService
            .DeleteAsync(
                id,
                cancellationToken);

        return NoContent();
    }
}
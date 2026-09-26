using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Middlewares;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class PersonService :
    IPersonService
{
    private readonly AppDbContext _context;

    private readonly IPersonRepository
        _personRepository;

    public PersonService(
        AppDbContext context,
        IPersonRepository personRepository)
    {
        _context = context;
        _personRepository =
            personRepository;
    }

    public async Task<List<PersonResponse>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        var persons =
            await _personRepository
                .GetAllAsync(
                    cancellationToken);

        return persons
            .Select(Map)
            .ToList();
    }

    public async Task<PersonResponse>
        GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new BusinessException(
                "O identificador da pessoa é inválido.");
        }

        var person =
            await _personRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (person is null)
        {
            throw new NotFoundException(
                "Pessoa não encontrada.");
        }

        return Map(
            person);
    }

    public async Task<PersonResponse>
        CreateAsync(
            CreatePersonRequest request,
            CancellationToken cancellationToken = default)
    {
        ValidateName(
            request.Name);

        var document =
            NormalizeDocument(
                request.Document);

        var email =
            NormalizeEmail(
                request.Email);

        if (document is not null)
        {
            var existingDocument =
                await _personRepository
                    .GetByDocumentAsync(
                        document,
                        cancellationToken);

            if (existingDocument is not null)
            {
                throw new BusinessException(
                    "Já existe uma pessoa cadastrada com este documento.");
            }
        }

        if (email is not null)
        {
            var existingEmail =
                await _personRepository
                    .GetByEmailAsync(
                        email,
                        cancellationToken);

            if (existingEmail is not null)
            {
                throw new BusinessException(
                    "Já existe uma pessoa cadastrada com este e-mail.");
            }
        }

        ValidateBirthDate(
            request.BirthDate);

        ValidateState(
            request.State);

        var person =
            new Person
            {
                Name =
                    request.Name.Trim(),

                Document =
                    document,

                BirthDate =
                    request.BirthDate,

                Email =
                    email,

                Phone =
                    Normalize(
                        request.Phone),

                ZipCode =
                    Normalize(
                        request.ZipCode),

                Street =
                    Normalize(
                        request.Street),

                Number =
                    Normalize(
                        request.Number),

                Complement =
                    Normalize(
                        request.Complement),

                Neighborhood =
                    Normalize(
                        request.Neighborhood),

                City =
                    Normalize(
                        request.City),

                State =
                    NormalizeState(
                        request.State)
            };

        _personRepository.Add(
            person);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Map(
            person);
    }

    public async Task<PersonResponse>
        UpdateAsync(
            int id,
            UpdatePersonRequest request,
            CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new BusinessException(
                "O identificador da pessoa é inválido.");
        }

        ValidateName(
            request.Name);

        ValidateBirthDate(
            request.BirthDate);

        ValidateState(
            request.State);

        var person =
            await _personRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (person is null)
        {
            throw new NotFoundException(
                "Pessoa não encontrada.");
        }

        var document =
            NormalizeDocument(
                request.Document);

        var email =
            NormalizeEmail(
                request.Email);

        /*
         * Verifica documento duplicado,
         * ignorando a própria pessoa.
         */
        if (document is not null)
        {
            var existingDocument =
                await _personRepository
                    .GetByDocumentAsync(
                        document,
                        cancellationToken);

            if (existingDocument is not null &&
                existingDocument.Id != id)
            {
                throw new BusinessException(
                    "Já existe uma pessoa cadastrada com este documento.");
            }
        }

        /*
         * Verifica e-mail duplicado,
         * ignorando a própria pessoa.
         */
        if (email is not null)
        {
            var existingEmail =
                await _personRepository
                    .GetByEmailAsync(
                        email,
                        cancellationToken);

            if (existingEmail is not null &&
                existingEmail.Id != id)
            {
                throw new BusinessException(
                    "Já existe uma pessoa cadastrada com este e-mail.");
            }
        }

        person.Name =
            request.Name.Trim();

        person.Document =
            document;

        person.BirthDate =
            request.BirthDate;

        person.Email =
            email;

        person.Phone =
            Normalize(
                request.Phone);

        person.ZipCode =
            Normalize(
                request.ZipCode);

        person.Street =
            Normalize(
                request.Street);

        person.Number =
            Normalize(
                request.Number);

        person.Complement =
            Normalize(
                request.Complement);

        person.Neighborhood =
            Normalize(
                request.Neighborhood);

        person.City =
            Normalize(
                request.City);

        person.State =
            NormalizeState(
                request.State);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Map(
            person);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new BusinessException(
                "O identificador da pessoa é inválido.");
        }

        var person =
            await _personRepository
                .GetByIdAsync(
                    id,
                    cancellationToken);

        if (person is null)
        {
            throw new NotFoundException(
                "Pessoa não encontrada.");
        }

        /*
         * Pessoa com conta não pode ser
         * removida diretamente.
         */
        if (person.Account is not null)
        {
            throw new BusinessException(
                "A pessoa não pode ser excluída porque possui uma conta vinculada.");
        }

        /*
         * Também verifica limite de transferência,
         * caso exista uma configuração associada.
         */
        var hasTransferLimit =
            await _context.TransferLimits
                .AnyAsync(
                    x => x.PersonId == id,
                    cancellationToken);

        if (hasTransferLimit)
        {
            throw new BusinessException(
                "A pessoa não pode ser excluída porque possui limites de transferência configurados.");
        }

        _personRepository.Remove(
            person);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    private static void ValidateName(
        string name)
    {
        if (string.IsNullOrWhiteSpace(
                name))
        {
            throw new BusinessException(
                "O nome da pessoa é obrigatório.");
        }

        if (name.Trim().Length > 150)
        {
            throw new BusinessException(
                "O nome da pessoa deve possuir no máximo 150 caracteres.");
        }
    }

    private static void ValidateBirthDate(
        DateOnly? birthDate)
    {
        if (birthDate is null)
        {
            return;
        }

        var today =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        if (birthDate > today)
        {
            throw new BusinessException(
                "A data de nascimento não pode ser futura.");
        }
    }

    private static void ValidateState(
        string? state)
    {
        if (string.IsNullOrWhiteSpace(
                state))
        {
            return;
        }

        if (state.Trim().Length != 2)
        {
            throw new BusinessException(
                "O estado deve ser informado com a sigla de 2 caracteres.");
        }
    }

    private static string? Normalize(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? NormalizeEmail(
        string? value)
    {
        var email =
            Normalize(value);

        return email?
            .ToLowerInvariant();
    }

    private static string? NormalizeDocument(
        string? value)
    {
        var document =
            Normalize(value);

        if (document is null)
        {
            return null;
        }

        /*
         * Remove pontuação de CPF/CNPJ.
         *
         * Ex:
         * 123.456.789-00
         *
         * vira
         *
         * 12345678900
         */
        return new string(
            document
                .Where(char.IsDigit)
                .ToArray());
    }

    private static string? NormalizeState(
        string? value)
    {
        var state =
            Normalize(value);

        return state?
            .ToUpperInvariant();
    }

    private static PersonResponse Map(
        Person person)
    {
        return new PersonResponse
        {
            Id =
                person.Id,

            Name =
                person.Name,

            Document =
                person.Document,

            BirthDate =
                person.BirthDate,

            Email =
                person.Email,

            Phone =
                person.Phone,

            ZipCode =
                person.ZipCode,

            Street =
                person.Street,

            Number =
                person.Number,

            Complement =
                person.Complement,

            Neighborhood =
                person.Neighborhood,

            City =
                person.City,

            State =
                person.State,

            AccountId =
                person.Account?.Id
        };
    }
}
// Iot2Project.API/Controllers/UsersController.cs
using Iot2Project.Application.Users.CreateUser;
using Iot2Project.Application.Users.GetAllUsers;
using Iot2Project.Application.Users.GetUserById;
using Iot2Project.Application.Users.UpdateUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Iot2Project.Application.Users.DeleteUser;
using System.Net;

namespace Iot2Project.API.Controllers;

/// <summary>
/// Opera um CRUDE (Create, Read, Update, Delete, Exclude) de usuários.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly CreateUserService _create;
    private readonly GetAllUsersService _getAll;   // (opcional) listar
    private readonly GetUserByIdService _getByID;
    private readonly UpdateUserService _update;   // (opcional) atualizar
    private readonly DeleteUserService _delete;   // (opcional) excluir

    public UsersController(
        CreateUserService create,
        GetAllUsersService getAll, 
        GetUserByIdService getByID,
        UpdateUserService update,
        DeleteUserService delete)
    {
        _create = create;
        _getAll = getAll;
        _getByID = getByID;
        _update=update;
        _delete=delete;
    }

    // ------------------------------------------------------------------
    // POST api/users   →  cria usuário
    // ------------------------------------------------------------------
    /// <summary>Cadastra um novo usuário.</summary>
    /// <remarks>
    /// Retorna <b>201 Created</b> com o JSON do usuário recém-criado.<br/>
    /// Se o e-mail já existir, devolve 409.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(
        [FromBody] CreateUserRequest dto,
        CancellationToken ct)
    {
        UserDto created = await _create.ExecuteAsync(dto, ct);
        return CreatedAtAction(nameof(GetUserById), new { id = created.UserId }, created);
    }

    // ------------------------------------------------------------------
    // GET api/users    →  lista todos
    // ------------------------------------------------------------------
    /// <summary>Lista todos os usuários ativos (não deletados).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _getAll.ExecuteAsync(ct);
        return Ok(users);                       // ← IEnumerable<UserDto>
    }

    // ------------------------------------------------------------------
    // GET api/users/{id}  →  detalhe (stub)
    // ------------------------------------------------------------------
    /// <summary>Obtém um usuário pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        // TODO: injetar e chamar um GetUserByIdService
        var user = await _getByID.ExecuteAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Atualiza campos específicos de um usuário.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(
    int id,
    [FromBody] UpdateUserRequest dto,
    CancellationToken ct)
    {
        var updated = await _update.ExecuteAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Marca um usuário como excluído.</summary>
    /// <response code="204">Usuário excluído</response>
    /// <response code="404">Id não encontrado</response>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        bool ok = await _delete.ExecuteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }




}

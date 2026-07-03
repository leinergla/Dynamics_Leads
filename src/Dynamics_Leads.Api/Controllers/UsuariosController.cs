using System.Security.Claims;
using Dynamics_Leads.Application.Auth;
using Dynamics_Leads.Application.DTOs;
using Dynamics_Leads.Application.DTOs.Auth;
using Dynamics_Leads.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dynamics_Leads.Api.Controllers;

/// <summary>Gestión de usuarios. Requiere el permiso usuarios.manage.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Policy = Permisos.UsuariosManage)]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarios;

    public UsuariosController(IUsuarioService usuarios)
    {
        _usuarios = usuarios;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UsuarioResponse>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        => Ok(await _usuarios.ListAsync(page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.GetByIdAsync(id, cancellationToken);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request, CancellationToken cancellationToken)
    {
        var creado = await _usuarios.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUsuarioRequest request, CancellationToken cancellationToken)
    {
        var actualizado = await _usuarios.UpdateAsync(id, request, cancellationToken);
        return actualizado ? NoContent() : NotFound();
    }

    /// <summary>
    /// Restablece la contraseña de otro usuario. No se permite cambiar la propia
    /// contraseña por esta vía (un administrador solo puede cambiar la de otros).
    /// </summary>
    [HttpPut("{id:guid}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sub, out var currentUserId) && currentUserId == id)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Operación no permitida",
                Detail = "No puedes cambiar tu propia contraseña por esta vía.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var actualizado = await _usuarios.ChangePasswordAsync(id, request, cancellationToken);
        return actualizado ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await _usuarios.DeleteAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound();
    }
}

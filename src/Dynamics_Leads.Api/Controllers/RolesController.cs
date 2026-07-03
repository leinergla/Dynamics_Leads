using Dynamics_Leads.Application.Auth;
using Dynamics_Leads.Application.DTOs.Auth;
using Dynamics_Leads.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dynamics_Leads.Api.Controllers;

/// <summary>Listado de roles (para formularios de gestión de usuarios).</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Policy = Permisos.UsuariosManage)]
public sealed class RolesController : ControllerBase
{
    private readonly IUsuarioService _usuarios;

    public RolesController(IUsuarioService usuarios)
    {
        _usuarios = usuarios;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RolResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RolResponse>>> Get(CancellationToken cancellationToken)
        => Ok(await _usuarios.ListRolesAsync(cancellationToken));
}

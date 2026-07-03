using System.Security.Claims;
using Dynamics_Leads.Application.DTOs.Auth;
using Dynamics_Leads.Application.Services;
using Dynamics_Leads.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dynamics_Leads.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly JwtOptions _jwt;

    public AuthController(IAuthService auth, IOptions<JwtOptions> jwt)
    {
        _auth = auth;
        _jwt = jwt.Value;
    }

    /// <summary>Inicia sesión: valida credenciales y deja el JWT en una cookie httpOnly.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _auth.LoginAsync(request, cancellationToken);
        if (result is null)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
        }

        Response.Cookies.Append(_jwt.CookieName, result.Token, OpcionesCookie(result.ExpiraUtc));
        return Ok(new { usuario = result.Usuario, permisos = result.Permisos });
    }

    /// <summary>Cierra sesión: elimina la cookie de autenticación.</summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(_jwt.CookieName, OpcionesCookie(DateTime.UtcNow));
        return NoContent();
    }

    /// <summary>Devuelve el usuario autenticado, su rol y permisos.</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sub, out var userId))
        {
            return Unauthorized();
        }

        var me = await _auth.GetCurrentAsync(userId, cancellationToken);
        return me is null ? Unauthorized() : Ok(me);
    }

    private CookieOptions OpcionesCookie(DateTime expiraUtc) => new()
    {
        HttpOnly = true,
        Secure = _jwt.CookieSecure,
        SameSite = SameSiteMode.Lax,
        Expires = expiraUtc,
        Path = "/",
    };
}

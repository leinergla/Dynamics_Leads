using Dynamics_Leads.Application.DTOs.Auth;
using Dynamics_Leads.Application.Security;
using Dynamics_Leads.Domain.Repositories;

namespace Dynamics_Leads.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(IUsuarioRepository usuarios, IPasswordHasher hasher, IJwtTokenGenerator jwt)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var usuario = await _usuarios.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (usuario is null || !usuario.Activo || !_hasher.Verify(usuario.PasswordHash, request.Password))
        {
            return null;
        }

        var permisos = await _usuarios.GetPermisosByRolAsync(usuario.RolId, cancellationToken);
        var token = _jwt.Generar(usuario, permisos);

        return new LoginResult
        {
            Token = token.Token,
            ExpiraUtc = token.ExpiraUtc,
            Usuario = MapUsuario(usuario),
            Permisos = permisos,
        };
    }

    public async Task<CurrentUserResponse?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarios.GetByIdAsync(userId, cancellationToken);
        if (usuario is null)
        {
            return null;
        }

        var permisos = await _usuarios.GetPermisosByRolAsync(usuario.RolId, cancellationToken);
        return new CurrentUserResponse
        {
            Id = usuario.Id,
            Username = usuario.Username,
            Email = usuario.Email,
            Rol = usuario.RolNombre,
            Permisos = permisos,
        };
    }

    internal static UsuarioResponse MapUsuario(Domain.Entities.Usuario u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        RolId = u.RolId,
        RolNombre = u.RolNombre,
        Activo = u.Activo,
        FechaCreacion = u.FechaCreacion,
    };
}

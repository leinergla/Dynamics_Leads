using Dynamics_Leads.Application.DTOs.Auth;

namespace Dynamics_Leads.Application.Services;

public interface IAuthService
{
    /// <summary>Valida credenciales y genera el token. Devuelve null si son inválidas o el usuario está inactivo.</summary>
    Task<LoginResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Datos del usuario autenticado (para /me). Null si no existe.</summary>
    Task<CurrentUserResponse?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken = default);
}

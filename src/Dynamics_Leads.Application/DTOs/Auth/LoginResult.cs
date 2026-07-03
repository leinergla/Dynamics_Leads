namespace Dynamics_Leads.Application.DTOs.Auth;

/// <summary>Resultado de un login correcto: token a guardar en cookie + datos del usuario.</summary>
public sealed class LoginResult
{
    public required string Token { get; init; }
    public DateTime ExpiraUtc { get; init; }
    public required UsuarioResponse Usuario { get; init; }
    public IReadOnlyList<string> Permisos { get; init; } = [];
}

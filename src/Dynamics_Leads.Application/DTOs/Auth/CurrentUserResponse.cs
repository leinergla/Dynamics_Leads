namespace Dynamics_Leads.Application.DTOs.Auth;

/// <summary>Datos del usuario autenticado (endpoint /me).</summary>
public sealed class CurrentUserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Rol { get; init; } = string.Empty;
    public IReadOnlyList<string> Permisos { get; init; } = [];
}

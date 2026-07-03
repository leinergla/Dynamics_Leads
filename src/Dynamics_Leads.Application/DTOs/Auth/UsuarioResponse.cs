namespace Dynamics_Leads.Application.DTOs.Auth;

public sealed class UsuarioResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string? Email { get; init; }
    public Guid RolId { get; init; }
    public string RolNombre { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
}

namespace Dynamics_Leads.Domain.Entities;

/// <summary>
/// Usuario del sistema. Cada usuario pertenece a un único rol.
/// </summary>
public sealed class Usuario
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

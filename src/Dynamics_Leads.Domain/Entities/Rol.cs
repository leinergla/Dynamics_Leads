namespace Dynamics_Leads.Domain.Entities;

/// <summary>Rol al que pertenecen los usuarios; agrupa permisos.</summary>
public sealed class Rol
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

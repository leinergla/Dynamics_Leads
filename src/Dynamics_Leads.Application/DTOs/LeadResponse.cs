namespace Dynamics_Leads.Application.DTOs;

/// <summary>
/// Resultado devuelto tras crear un lead.
/// </summary>
public sealed class LeadResponse
{
    /// <summary>Identificador único generado para el lead.</summary>
    public Guid LeadId { get; init; }
}

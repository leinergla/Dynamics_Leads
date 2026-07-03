namespace Dynamics_Leads.Domain.Entities;

/// <summary>
/// Representa un lead tal como se almacena en la tabla public.leads.
/// </summary>
public sealed class Lead
{
    /// <summary>Identificador único del lead (generado por la base de datos).</summary>
    public Guid LeadId { get; set; }

    /// <summary>Nombre del formulario de origen del lead.</summary>
    public string Formulario { get; set; } = string.Empty;

    /// <summary>Campos del lead en formato JSON (columna jsonb): array de campos del formulario.</summary>
    public string Datos { get; set; } = "[]";

    /// <summary>Fecha de creación del registro.</summary>
    public DateTime FechaCreacion { get; set; }
}

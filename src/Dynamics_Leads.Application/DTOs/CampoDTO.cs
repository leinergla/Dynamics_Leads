namespace Dynamics_Leads.Application.DTOs;

/// <summary>
/// Representa un campo individual de un formulario dentro de los datos del lead.
/// </summary>
public sealed class CampoDTO
{
    /// <summary>Nombre del campo.</summary>
    public string? Nombre { get; set; }

    /// <summary>Valor capturado del campo.</summary>
    public string? Valor { get; set; }

    /// <summary>Orden de aparición del campo en el formulario.</summary>
    public int Orden { get; set; }

    /// <summary>Alias o nombre alternativo del campo.</summary>
    public string? Alias { get; set; }
}

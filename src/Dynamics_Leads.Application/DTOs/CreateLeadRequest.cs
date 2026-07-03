using System.ComponentModel.DataAnnotations;

namespace Dynamics_Leads.Application.DTOs;

/// <summary>
/// Datos de entrada para registrar un nuevo lead.
/// </summary>
public sealed class CreateLeadRequest
{
    /// <summary>Nombre del formulario de origen. Obligatorio, máximo 255 caracteres.</summary>
    [Required(ErrorMessage = "El campo 'formulario' es obligatorio.")]
    [MaxLength(255, ErrorMessage = "El campo 'formulario' no puede superar los 255 caracteres.")]
    public string Formulario { get; set; } = string.Empty;

    /// <summary>Lista de campos del formulario (se almacenan como jsonb).</summary>
    [Required(ErrorMessage = "El campo 'datos' es obligatorio.")]
    [MinLength(1, ErrorMessage = "El campo 'datos' debe contener al menos un elemento.")]
    public List<CampoDTO> Datos { get; set; } = [];

    /// <summary>Archivos adjuntos del formulario (opcional). Se guardan en disco; en BD solo la ruta.</summary>
    public List<ArchivoDTO> Archivos { get; set; } = [];
}

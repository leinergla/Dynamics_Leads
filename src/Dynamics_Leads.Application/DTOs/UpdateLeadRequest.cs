using System.ComponentModel.DataAnnotations;

namespace Dynamics_Leads.Application.DTOs;

/// <summary>
/// Datos de entrada para actualizar un lead existente (formulario y campos).
/// Los archivos se gestionan mediante sus propios endpoints.
/// </summary>
public sealed class UpdateLeadRequest
{
    /// <summary>Nombre del formulario de origen. Obligatorio, máximo 255 caracteres.</summary>
    [Required(ErrorMessage = "El campo 'formulario' es obligatorio.")]
    [MaxLength(255, ErrorMessage = "El campo 'formulario' no puede superar los 255 caracteres.")]
    public string Formulario { get; set; } = string.Empty;

    /// <summary>Lista de campos del formulario (reemplaza por completo el contenido de 'datos').</summary>
    [Required(ErrorMessage = "El campo 'datos' es obligatorio.")]
    [MinLength(1, ErrorMessage = "El campo 'datos' debe contener al menos un elemento.")]
    public List<CampoDTO> Datos { get; set; } = [];
}

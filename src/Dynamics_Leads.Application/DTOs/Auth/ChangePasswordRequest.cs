using System.ComponentModel.DataAnnotations;

namespace Dynamics_Leads.Application.DTOs.Auth;

/// <summary>Restablece la contraseña de un usuario (solo la nueva contraseña).</summary>
public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

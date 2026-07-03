using System.ComponentModel.DataAnnotations;

namespace Dynamics_Leads.Application.DTOs.Auth;

public sealed class UpdateUsuarioRequest
{
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public Guid RolId { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>Opcional: si se indica, restablece la contraseña.</summary>
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string? Password { get; set; }
}

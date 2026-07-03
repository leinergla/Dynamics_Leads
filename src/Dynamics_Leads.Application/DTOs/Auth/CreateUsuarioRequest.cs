using System.ComponentModel.DataAnnotations;

namespace Dynamics_Leads.Application.DTOs.Auth;

public sealed class CreateUsuarioRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public Guid RolId { get; set; }
}

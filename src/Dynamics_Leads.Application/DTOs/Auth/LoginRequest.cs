using System.ComponentModel.DataAnnotations;

namespace Dynamics_Leads.Application.DTOs.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

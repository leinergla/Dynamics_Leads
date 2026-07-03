using Dynamics_Leads.Domain.Entities;

namespace Dynamics_Leads.Application.Security;

/// <summary>Token JWT generado: el valor y su expiración.</summary>
public sealed record TokenGenerado(string Token, DateTime ExpiraUtc);

/// <summary>Genera el JWT del usuario con sus claims (rol y permisos).</summary>
public interface IJwtTokenGenerator
{
    TokenGenerado Generar(Usuario usuario, IReadOnlyList<string> permisos);
}

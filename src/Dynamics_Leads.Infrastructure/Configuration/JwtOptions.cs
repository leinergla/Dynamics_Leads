namespace Dynamics_Leads.Infrastructure.Configuration;

/// <summary>Opciones de JWT y de la cookie de autenticación (sección "Jwt").</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Clave de firma simétrica (HS256). Mínimo 32 caracteres.</summary>
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "dynamics_leads";
    public string Audience { get; set; } = "dynamics_leads";
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>Nombre de la cookie httpOnly que transporta el token.</summary>
    public string CookieName { get; set; } = "access_token";

    /// <summary>Si la cookie debe marcarse Secure (true en producción/HTTPS).</summary>
    public bool CookieSecure { get; set; }
}

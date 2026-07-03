namespace Dynamics_Leads.Application.Security;

/// <summary>Hash y verificación de contraseñas. Implementación en infraestructura (PBKDF2).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}

using System.Security.Cryptography;
using Dynamics_Leads.Application.Security;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Dynamics_Leads.Infrastructure.Security;

/// <summary>
/// Hash de contraseñas con PBKDF2 (HMAC-SHA256). Formato almacenado: "iteraciones.saltBase64.hashBase64".
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iteraciones = 100_000;
    private const int TamanoSalt = 16;
    private const int TamanoHash = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanoSalt);
        var hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iteraciones, TamanoHash);
        return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string hash, string password)
    {
        var partes = hash.Split('.');
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones))
        {
            return false;
        }

        byte[] salt, esperado;
        try
        {
            salt = Convert.FromBase64String(partes[1]);
            esperado = Convert.FromBase64String(partes[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, iteraciones, esperado.Length);
        return CryptographicOperations.FixedTimeEquals(actual, esperado);
    }
}

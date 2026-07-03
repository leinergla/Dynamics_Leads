using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dynamics_Leads.Application.Auth;
using Dynamics_Leads.Application.Security;
using Dynamics_Leads.Domain.Entities;
using Dynamics_Leads.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dynamics_Leads.Infrastructure.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TokenGenerado Generar(Usuario usuario, IReadOnlyList<string> permisos)
    {
        var expira = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, usuario.Username),
            new(ClaimTypes.Role, usuario.RolNombre),
        };

        if (!string.IsNullOrWhiteSpace(usuario.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, usuario.Email));
        }

        foreach (var permiso in permisos)
        {
            claims.Add(new Claim(Permisos.ClaimType, permiso));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: creds);

        return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}

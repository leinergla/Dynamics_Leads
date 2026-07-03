using Dapper;
using Dynamics_Leads.Domain.Entities;
using Dynamics_Leads.Domain.Repositories;
using Dynamics_Leads.Infrastructure.Persistence;

namespace Dynamics_Leads.Infrastructure.Repositories;

/// <summary>Acceso a datos de roles con Dapper; solo rutinas de BD.</summary>
public sealed class RolRepository : IRolRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RolRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Rol>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var roles = await connection.QueryAsync<Rol>(new CommandDefinition(
            "SELECT id, nombre FROM public.fn_list_roles()", cancellationToken: cancellationToken));
        return roles.ToList();
    }

    public async Task<Rol?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default)
    {
        var roles = await ListAsync(cancellationToken);
        return roles.FirstOrDefault(r => string.Equals(r.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
    }
}

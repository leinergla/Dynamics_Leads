using System.Data;
using Dynamics_Leads.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Dynamics_Leads.Infrastructure.Persistence;

/// <summary>
/// Implementación de <see cref="IDbConnectionFactory"/> para PostgreSQL usando Npgsql.
/// </summary>
public sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

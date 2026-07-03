using System.Data;

namespace Dynamics_Leads.Infrastructure.Persistence;

/// <summary>
/// Fábrica de conexiones a la base de datos. Abstrae la creación de
/// <see cref="IDbConnection"/> para facilitar pruebas e Inversión de Control.
/// </summary>
public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}

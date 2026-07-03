using Dynamics_Leads.Domain.Entities;

namespace Dynamics_Leads.Domain.Repositories;

/// <summary>Acceso a datos de roles. La implementación usa rutinas de BD.</summary>
public interface IRolRepository
{
    Task<IReadOnlyList<Rol>> ListAsync(CancellationToken cancellationToken = default);
    Task<Rol?> GetByNombreAsync(string nombre, CancellationToken cancellationToken = default);
}

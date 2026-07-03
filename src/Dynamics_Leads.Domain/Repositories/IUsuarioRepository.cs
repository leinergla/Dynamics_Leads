using Dynamics_Leads.Domain.Entities;

namespace Dynamics_Leads.Domain.Repositories;

/// <summary>Acceso a datos de usuarios. La implementación usa rutinas de BD.</summary>
public interface IUsuarioRepository
{
    Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Usuario>> ListAsync(int offset, int limit, CancellationToken cancellationToken = default);
    Task<long> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Códigos de permiso asociados al rol indicado.</summary>
    Task<IReadOnlyList<string>> GetPermisosByRolAsync(Guid rolId, CancellationToken cancellationToken = default);

    Task<Guid> InsertAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, string? email, Guid rolId, bool activo, CancellationToken cancellationToken = default);
    Task<bool> UpdatePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

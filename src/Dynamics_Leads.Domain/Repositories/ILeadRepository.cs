using Dynamics_Leads.Domain.Entities;

namespace Dynamics_Leads.Domain.Repositories;

/// <summary>
/// Contrato de acceso a datos para leads y sus archivos.
/// La implementación reside en la capa de infraestructura (Inversión de Control).
/// </summary>
public interface ILeadRepository
{
    /// <summary>
    /// Inserta un lead y sus archivos de forma transaccional mediante procedimientos almacenados.
    /// </summary>
    /// <returns>El <see cref="Guid"/> generado para el nuevo lead.</returns>
    Task<Guid> InsertAsync(Lead lead, IReadOnlyList<Archivo> archivos, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve una página de leads, opcionalmente filtrados por formulario, ordenados por fecha descendente.
    /// </summary>
    Task<IReadOnlyList<Lead>> ListAsync(string? formulario, int offset, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta los leads que coinciden con el filtro de formulario.
    /// </summary>
    Task<long> CountAsync(string? formulario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve los nombres de formulario distintos existentes, ordenados alfabéticamente.
    /// </summary>
    Task<IReadOnlyList<string>> ListFormulariosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un lead por su identificador, o null si no existe.
    /// </summary>
    Task<Lead?> GetByIdAsync(Guid leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los archivos pertenecientes a los leads indicados.
    /// </summary>
    Task<IReadOnlyList<Archivo>> GetArchivosByLeadIdsAsync(IReadOnlyList<Guid> leadIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un archivo por su identificador, o null si no existe.
    /// </summary>
    Task<Archivo?> GetArchivoByIdAsync(Guid archivoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el formulario y los datos (campos) de un lead. Devuelve true si existía.
    /// </summary>
    Task<bool> UpdateAsync(Lead lead, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un lead (y, en cascada, sus archivos en BD). Devuelve true si existía.
    /// </summary>
    Task<bool> DeleteAsync(Guid leadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un archivo concreto de la BD. Devuelve true si existía.
    /// </summary>
    Task<bool> DeleteArchivoAsync(Guid archivoId, CancellationToken cancellationToken = default);
}

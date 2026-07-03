using Dynamics_Leads.Application.DTOs;

namespace Dynamics_Leads.Application.Services;

/// <summary>
/// Lógica de negocio para la gestión de leads.
/// Las consultas devuelven cada lead como un objeto dinámico (campos del formulario al nivel raíz),
/// ya que cada formulario puede tener campos distintos.
/// </summary>
public interface ILeadService
{
    /// <summary>Crea un nuevo lead, guardando sus archivos y persistiendo solo las referencias.</summary>
    Task<LeadResponse> CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lista leads paginados (objetos dinámicos), opcionalmente filtrados por formulario.</summary>
    Task<PagedResult<IDictionary<string, object?>>> GetAsync(string? formulario, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Lista los nombres de formulario distintos existentes.</summary>
    Task<IReadOnlyList<string>> GetFormulariosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Exporta a Excel (.xlsx) todos los leads (objetos dinámicos), opcionalmente filtrados por formulario.
    /// Devuelve el contenido binario del archivo.
    /// </summary>
    Task<byte[]> ExportarLeadsAsync(string? formulario, CancellationToken cancellationToken = default);

    /// <summary>Obtiene un lead como objeto dinámico, o null si no existe.</summary>
    Task<IDictionary<string, object?>?> GetByIdAsync(Guid leadId, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los archivos de un lead (vacío si no tiene o si el lead no existe).</summary>
    Task<IReadOnlyList<ArchivoResponse>> GetArchivosByLeadAsync(Guid leadId, CancellationToken cancellationToken = default);

    /// <summary>Obtiene los campos crudos de un lead (con nombre, valor, orden y alias), o null si no existe. Útil para edición fiel.</summary>
    Task<IReadOnlyList<CampoDTO>?> GetCamposByLeadAsync(Guid leadId, CancellationToken cancellationToken = default);

    /// <summary>Actualiza un lead (formulario y campos). Devuelve el lead actualizado, o null si no existe.</summary>
    Task<IDictionary<string, object?>?> UpdateAsync(Guid leadId, UpdateLeadRequest request, CancellationToken cancellationToken = default);

    /// <summary>Obtiene el contenido de un archivo para su descarga. Lanza <see cref="KeyNotFoundException"/> si no existe.</summary>
    Task<ArchivoContenido> GetArchivoContenidoAsync(Guid archivoId, CancellationToken cancellationToken = default);

    /// <summary>Elimina un lead y los binarios de sus archivos en disco. Devuelve true si existía.</summary>
    Task<bool> DeleteAsync(Guid leadId, CancellationToken cancellationToken = default);

    /// <summary>Elimina un archivo concreto (fila en BD y binario en disco). Devuelve true si existía.</summary>
    Task<bool> DeleteArchivoAsync(Guid archivoId, CancellationToken cancellationToken = default);
}

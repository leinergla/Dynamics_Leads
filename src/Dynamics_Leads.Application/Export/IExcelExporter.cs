namespace Dynamics_Leads.Application.Export;

/// <summary>
/// Genera un archivo Excel (.xlsx) a partir de leads dinámicos.
/// La implementación reside en infraestructura (IoC).
/// </summary>
public interface IExcelExporter
{
    /// <summary>
    /// Construye un .xlsx con una fila por lead. Las columnas se calculan dinámicamente
    /// (metadatos primero, luego los campos de cada formulario).
    /// </summary>
    byte[] ExportarLeads(IReadOnlyList<IDictionary<string, object?>> leads);
}

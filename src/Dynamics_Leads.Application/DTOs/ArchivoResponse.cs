namespace Dynamics_Leads.Application.DTOs;

/// <summary>
/// Representación de un archivo en las respuestas de lectura. Expone una URL de descarga,
/// nunca la ruta física del servidor.
/// </summary>
public sealed class ArchivoResponse
{
    public Guid Id { get; init; }
    public string NombreArchivo { get; init; } = string.Empty;
    public string? NombreCampo { get; init; }
    public string? ContentType { get; init; }
    public long Tamano { get; init; }

    /// <summary>URL relativa para descargar el archivo (p. ej. /api/leads/archivos/{id}).</summary>
    public string Url { get; init; } = string.Empty;
}

namespace Dynamics_Leads.Application.DTOs;

/// <summary>
/// Contenido de un archivo para su descarga.
/// </summary>
public sealed class ArchivoContenido
{
    public required Stream Contenido { get; init; }
    public required string ContentType { get; init; }
    public required string NombreArchivo { get; init; }
}

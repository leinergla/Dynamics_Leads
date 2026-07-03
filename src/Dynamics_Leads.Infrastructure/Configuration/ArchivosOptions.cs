namespace Dynamics_Leads.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para el almacenamiento de archivos.
/// Se enlaza desde la sección "Archivos" de appsettings.json.
/// </summary>
public sealed class ArchivosOptions
{
    public const string SectionName = "Archivos";

    /// <summary>Carpeta base donde se guardan los archivos. Puede ser relativa o absoluta.</summary>
    public string BasePath { get; set; } = "Archivos";

    /// <summary>Tamaño máximo permitido por archivo, en bytes. Por defecto 10 MB.</summary>
    public long TamanoMaximoBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Extensiones permitidas (con punto, p. ej. ".pdf"). Vacío = se permiten todas.
    /// </summary>
    public string[] ExtensionesPermitidas { get; set; } = [];
}

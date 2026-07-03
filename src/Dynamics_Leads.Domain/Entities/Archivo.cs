namespace Dynamics_Leads.Domain.Entities;

/// <summary>
/// Representa un archivo asociado a un lead, almacenado en la tabla public.lead_archivos.
/// En base de datos solo se guarda la referencia (storage_key), nunca el binario.
/// </summary>
public sealed class Archivo
{
    /// <summary>Identificador único del archivo.</summary>
    public Guid Id { get; set; }

    /// <summary>Identificador del lead al que pertenece.</summary>
    public Guid LeadId { get; set; }

    /// <summary>Nombre original del archivo (con extensión).</summary>
    public string NombreArchivo { get; set; } = string.Empty;

    /// <summary>Clave/ruta relativa del archivo en el almacenamiento.</summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>Nombre del campo del formulario al que pertenece el archivo.</summary>
    public string? NombreCampo { get; set; }

    /// <summary>Tipo MIME del archivo.</summary>
    public string? ContentType { get; set; }

    /// <summary>Tamaño del archivo en bytes.</summary>
    public long Tamano { get; set; }

    /// <summary>Fecha de creación del registro.</summary>
    public DateTime FechaCreacion { get; set; }
}

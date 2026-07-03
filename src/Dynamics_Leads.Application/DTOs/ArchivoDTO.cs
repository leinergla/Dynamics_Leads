namespace Dynamics_Leads.Application.DTOs;

/// <summary>
/// Archivo recibido al crear un lead. El contenido llega en Base64; el servidor lo guarda
/// en el almacenamiento y persiste solo la referencia en la tabla lead_archivos.
/// </summary>
public sealed class ArchivoDTO
{
    /// <summary>Nombre original del archivo (con extensión).</summary>
    public string? NombreArchivo { get; set; }

    /// <summary>Nombre del campo del formulario al que pertenece el archivo.</summary>
    public string? NombreCampo { get; set; }

    /// <summary>Tipo MIME del archivo (opcional). Si no se indica, se usa application/octet-stream.</summary>
    public string? ContentType { get; set; }

    /// <summary>Contenido del archivo en Base64 (obligatorio para subir el archivo).</summary>
    public string? ContenidoBase64 { get; set; }
}

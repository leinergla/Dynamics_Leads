namespace Dynamics_Leads.Application.Storage;

/// <summary>
/// Abstracción para el almacenamiento físico de archivos.
/// La implementación reside en la capa de infraestructura (IoC).
/// </summary>
public interface IArchivoStorage
{
    /// <summary>
    /// Guarda el contenido de un archivo y devuelve la clave relativa con la que recuperarlo.
    /// Valida tamaño y extensión según la configuración; lanza <see cref="ArgumentException"/> si no son válidos.
    /// </summary>
    Task<string> GuardarArchivoAsync(string nombreArchivo, byte[] contenido, CancellationToken cancellationToken = default);

    /// <summary>
    /// Abre el archivo identificado por su clave para lectura. Lanza <see cref="FileNotFoundException"/> si no existe.
    /// </summary>
    Task<Stream> AbrirArchivoAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina el archivo identificado por su clave. No falla si el archivo no existe.
    /// </summary>
    Task EliminarArchivoAsync(string storageKey, CancellationToken cancellationToken = default);
}

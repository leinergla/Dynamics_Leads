using Dynamics_Leads.Application.Storage;
using Dynamics_Leads.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Dynamics_Leads.Infrastructure.Storage;

/// <summary>
/// Implementación de <see cref="IArchivoStorage"/> que guarda los archivos en el sistema de archivos local,
/// dentro de la carpeta configurada (por defecto "Archivos"), organizados por año/mes.
/// La clave devuelta es relativa a esa carpeta; en BD nunca se guarda la ruta absoluta.
/// </summary>
public sealed class FileSystemArchivoStorage : IArchivoStorage
{
    private readonly string _basePath;
    private readonly long _tamanoMaximoBytes;
    private readonly HashSet<string> _extensionesPermitidas;

    public FileSystemArchivoStorage(IOptions<ArchivosOptions> options)
    {
        var opt = options.Value;
        // Resuelve a ruta absoluta (si es relativa, respecto al directorio de trabajo).
        _basePath = Path.GetFullPath(opt.BasePath);
        _tamanoMaximoBytes = opt.TamanoMaximoBytes;
        _extensionesPermitidas = new HashSet<string>(
            opt.ExtensionesPermitidas.Select(e => e.Trim().ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string> GuardarArchivoAsync(string nombreArchivo, byte[] contenido, CancellationToken cancellationToken = default)
    {
        if (contenido.LongLength == 0)
        {
            throw new ArgumentException($"El archivo '{nombreArchivo}' está vacío.");
        }

        if (contenido.LongLength > _tamanoMaximoBytes)
        {
            throw new ArgumentException(
                $"El archivo '{nombreArchivo}' supera el tamaño máximo permitido ({_tamanoMaximoBytes} bytes).");
        }

        var nombreSeguro = Path.GetFileName(nombreArchivo);
        if (string.IsNullOrWhiteSpace(nombreSeguro))
        {
            nombreSeguro = "archivo";
        }

        var extension = Path.GetExtension(nombreSeguro).ToLowerInvariant();
        if (_extensionesPermitidas.Count > 0 && !_extensionesPermitidas.Contains(extension))
        {
            throw new ArgumentException($"La extensión '{extension}' no está permitida.");
        }

        // Organiza por año/mes para evitar carpetas enormes. La clave usa '/' como separador.
        var ahora = DateTime.UtcNow;
        var subcarpeta = $"{ahora:yyyy}/{ahora:MM}";
        var nombreUnico = $"{Guid.NewGuid():N}_{nombreSeguro}";
        var storageKey = $"{subcarpeta}/{nombreUnico}";

        var rutaAbsoluta = ResolverRuta(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(rutaAbsoluta)!);
        await File.WriteAllBytesAsync(rutaAbsoluta, contenido, cancellationToken);

        return storageKey;
    }

    public Task<Stream> AbrirArchivoAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var rutaAbsoluta = ResolverRuta(storageKey);
        if (!File.Exists(rutaAbsoluta))
        {
            throw new FileNotFoundException($"No se encontró el archivo '{storageKey}'.", rutaAbsoluta);
        }

        Stream stream = new FileStream(rutaAbsoluta, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    public Task EliminarArchivoAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var rutaAbsoluta = ResolverRuta(storageKey);
        if (File.Exists(rutaAbsoluta))
        {
            File.Delete(rutaAbsoluta);
        }

        return Task.CompletedTask;
    }

    /// <summary>Resuelve la clave relativa a una ruta absoluta y evita el traspaso de directorios.</summary>
    private string ResolverRuta(string storageKey)
    {
        var relativo = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var rutaAbsoluta = Path.GetFullPath(Path.Combine(_basePath, relativo));

        var baseNormalizada = _basePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!rutaAbsoluta.StartsWith(baseNormalizada, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Clave de almacenamiento inválida: '{storageKey}'.");
        }

        return rutaAbsoluta;
    }
}

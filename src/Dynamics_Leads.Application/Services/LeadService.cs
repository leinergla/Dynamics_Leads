using System.Text.Json;
using Dynamics_Leads.Application.DTOs;
using Dynamics_Leads.Application.Export;
using Dynamics_Leads.Application.Storage;
using Dynamics_Leads.Domain.Entities;
using Dynamics_Leads.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Dynamics_Leads.Application.Services;

/// <summary>
/// Implementación de la lógica de negocio de leads.
/// Depende de las abstracciones <see cref="ILeadRepository"/> e <see cref="IArchivoStorage"/> (Inversión de Control).
/// </summary>
public sealed class LeadService : ILeadService
{
    private const string DefaultContentType = "application/octet-stream";

    /// <summary>Claves reservadas de cada lead; prevalecen sobre cualquier campo con el mismo nombre.</summary>
    private static readonly HashSet<string> ClavesReservadas =
        new(["leadId", "formulario", "fechaCreacion"], StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Tamaño de lote al recorrer todos los leads para exportar.</summary>
    private const int ExportBatchSize = 1000;

    private readonly ILeadRepository _leadRepository;
    private readonly IArchivoStorage _archivoStorage;
    private readonly IExcelExporter _excelExporter;
    private readonly ILogger<LeadService> _logger;

    public LeadService(ILeadRepository leadRepository, IArchivoStorage archivoStorage, IExcelExporter excelExporter, ILogger<LeadService> logger)
    {
        _leadRepository = leadRepository;
        _archivoStorage = archivoStorage;
        _excelExporter = excelExporter;
        _logger = logger;
    }

    public async Task<LeadResponse> CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Los campos del formulario se guardan en la columna jsonb 'datos'.
        var lead = new Lead
        {
            Formulario = request.Formulario.Trim(),
            Datos = JsonSerializer.Serialize(request.Datos, JsonOptions)
        };

        // Cada archivo se guarda en el almacenamiento; solo se persiste la referencia.
        var archivos = new List<Archivo>(request.Archivos.Count);
        foreach (var archivo in request.Archivos)
        {
            if (string.IsNullOrWhiteSpace(archivo.ContenidoBase64))
            {
                throw new ArgumentException(
                    $"El archivo '{archivo.NombreArchivo}' no incluye contenido (contenidoBase64).");
            }

            byte[] contenido;
            try
            {
                contenido = Convert.FromBase64String(archivo.ContenidoBase64);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(
                    $"El contenido en Base64 del archivo '{archivo.NombreArchivo}' no es válido.", ex);
            }

            var nombre = string.IsNullOrWhiteSpace(archivo.NombreArchivo) ? "archivo" : archivo.NombreArchivo;
            var storageKey = await _archivoStorage.GuardarArchivoAsync(nombre, contenido, cancellationToken);

            archivos.Add(new Archivo
            {
                NombreArchivo = nombre,
                StorageKey = storageKey,
                NombreCampo = archivo.NombreCampo,
                ContentType = string.IsNullOrWhiteSpace(archivo.ContentType) ? DefaultContentType : archivo.ContentType,
                Tamano = contenido.LongLength
            });
        }

        var leadId = await _leadRepository.InsertAsync(lead, archivos, cancellationToken);
        return new LeadResponse { LeadId = leadId };
    }

    public async Task<PagedResult<IDictionary<string, object?>>> GetAsync(string? formulario, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _leadRepository.CountAsync(formulario, cancellationToken);
        var leads = await _leadRepository.ListAsync(formulario, (page - 1) * pageSize, pageSize, cancellationToken);

        var items = leads.Select(MapLead).ToList();

        return new PagedResult<IDictionary<string, object?>>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public Task<IReadOnlyList<string>> GetFormulariosAsync(CancellationToken cancellationToken = default)
        => _leadRepository.ListFormulariosAsync(cancellationToken);

    public async Task<byte[]> ExportarLeadsAsync(string? formulario, CancellationToken cancellationToken = default)
    {
        // Se recorren todos los leads del filtro por lotes (sin paginación visible al cliente).
        var items = new List<IDictionary<string, object?>>();
        var offset = 0;
        while (true)
        {
            var lote = await _leadRepository.ListAsync(formulario, offset, ExportBatchSize, cancellationToken);
            if (lote.Count == 0)
            {
                break;
            }

            foreach (var lead in lote)
            {
                items.Add(MapLead(lead));
            }

            if (lote.Count < ExportBatchSize)
            {
                break;
            }

            offset += ExportBatchSize;
        }

        return _excelExporter.ExportarLeads(items);
    }

    public async Task<IDictionary<string, object?>?> GetByIdAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(leadId, cancellationToken);
        return lead is null ? null : MapLead(lead);
    }

    public async Task<IReadOnlyList<ArchivoResponse>> GetArchivosByLeadAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        var archivos = await _leadRepository.GetArchivosByLeadIdsAsync([leadId], cancellationToken);
        return archivos.Select(MapArchivo).ToList();
    }

    public async Task<IReadOnlyList<CampoDTO>?> GetCamposByLeadAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(leadId, cancellationToken);
        if (lead is null)
        {
            return null;
        }

        var campos = string.IsNullOrWhiteSpace(lead.Datos)
            ? []
            : JsonSerializer.Deserialize<List<CampoDTO>>(lead.Datos, JsonOptions) ?? [];

        return campos.OrderBy(c => c.Orden).ToList();
    }

    public async Task<IDictionary<string, object?>?> UpdateAsync(Guid leadId, UpdateLeadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var lead = new Lead
        {
            LeadId = leadId,
            Formulario = request.Formulario.Trim(),
            Datos = JsonSerializer.Serialize(request.Datos, JsonOptions)
        };

        var actualizado = await _leadRepository.UpdateAsync(lead, cancellationToken);
        if (!actualizado)
        {
            return null;
        }

        return await GetByIdAsync(leadId, cancellationToken);
    }

    public async Task<ArchivoContenido> GetArchivoContenidoAsync(Guid archivoId, CancellationToken cancellationToken = default)
    {
        var archivo = await _leadRepository.GetArchivoByIdAsync(archivoId, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el archivo con id '{archivoId}'.");

        var stream = await _archivoStorage.AbrirArchivoAsync(archivo.StorageKey, cancellationToken);

        return new ArchivoContenido
        {
            Contenido = stream,
            ContentType = string.IsNullOrWhiteSpace(archivo.ContentType) ? DefaultContentType : archivo.ContentType,
            NombreArchivo = archivo.NombreArchivo
        };
    }

    public async Task<bool> DeleteAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        // Se obtienen las claves antes de borrar, porque el CASCADE elimina las filas de archivos.
        var archivos = await _leadRepository.GetArchivosByLeadIdsAsync([leadId], cancellationToken);

        var eliminado = await _leadRepository.DeleteAsync(leadId, cancellationToken);
        if (!eliminado)
        {
            return false;
        }

        // Borrado best-effort de los binarios: un fallo aquí no revierte el borrado en BD.
        foreach (var archivo in archivos)
        {
            try
            {
                await _archivoStorage.EliminarArchivoAsync(archivo.StorageKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo eliminar el binario del archivo {ArchivoId} (clave {StorageKey}).",
                    archivo.Id, archivo.StorageKey);
            }
        }

        return true;
    }

    public async Task<bool> DeleteArchivoAsync(Guid archivoId, CancellationToken cancellationToken = default)
    {
        var archivo = await _leadRepository.GetArchivoByIdAsync(archivoId, cancellationToken);
        if (archivo is null)
        {
            return false;
        }

        var eliminado = await _leadRepository.DeleteArchivoAsync(archivoId, cancellationToken);
        if (!eliminado)
        {
            return false;
        }

        try
        {
            await _archivoStorage.EliminarArchivoAsync(archivo.StorageKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "No se pudo eliminar el binario del archivo {ArchivoId} (clave {StorageKey}).",
                archivo.Id, archivo.StorageKey);
        }

        return true;
    }

    /// <summary>
    /// Construye un objeto dinámico por lead: metadatos reservados + los campos del formulario
    /// "aplanados" al nivel raíz (clave = nombre del campo, valor = valor del campo).
    /// Los campos cuyo nombre coincida con una clave reservada se ignoran para no romper los metadatos.
    /// Los archivos NO se incluyen aquí; se consultan en GET /api/Leads/{id}/archivos.
    /// </summary>
    private static Dictionary<string, object?> MapLead(Lead lead)
    {
        var item = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["leadId"] = lead.LeadId,
            ["formulario"] = lead.Formulario,
            ["fechaCreacion"] = lead.FechaCreacion
        };

        var campos = string.IsNullOrWhiteSpace(lead.Datos)
            ? []
            : JsonSerializer.Deserialize<List<CampoDTO>>(lead.Datos, JsonOptions) ?? [];

        foreach (var campo in campos.OrderBy(c => c.Orden))
        {
            if (string.IsNullOrWhiteSpace(campo.Nombre) || ClavesReservadas.Contains(campo.Nombre))
            {
                continue;
            }

            item[campo.Nombre] = campo.Valor;
        }

        return item;
    }

    private static ArchivoResponse MapArchivo(Archivo a) => new()
    {
        Id = a.Id,
        NombreArchivo = a.NombreArchivo,
        NombreCampo = a.NombreCampo,
        ContentType = a.ContentType,
        Tamano = a.Tamano,
        Url = $"/api/leads/archivos/{a.Id}"
    };
}

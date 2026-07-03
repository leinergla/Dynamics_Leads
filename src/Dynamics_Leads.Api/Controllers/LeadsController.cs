using Dynamics_Leads.Application.Auth;
using Dynamics_Leads.Application.DTOs;
using Dynamics_Leads.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dynamics_Leads.Api.Controllers;

/// <summary>
/// Endpoints para la gestión de leads. Cada acción exige un permiso.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    /// <summary>Inserta un nuevo lead (con sus campos y archivos).</summary>
    [HttpPost]
    [Authorize(Policy = Permisos.LeadsCreate)]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _leadService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.LeadId }, result);
    }

    /// <summary>Lista leads paginados (objetos dinámicos), opcionalmente filtrados por formulario (orden por fecha descendente).</summary>
    [HttpGet]
    [Authorize(Policy = Permisos.LeadsRead)]
    [ProducesResponseType(typeof(PagedResult<Dictionary<string, object?>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<IDictionary<string, object?>>>> Get(
        [FromQuery] string? formulario,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _leadService.GetAsync(formulario, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>Lista los nombres de formulario distintos existentes (para poblar filtros/dropdowns).</summary>
    [HttpGet("formularios")]
    [Authorize(Policy = Permisos.LeadsRead)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetFormularios(CancellationToken cancellationToken)
    {
        var formularios = await _leadService.GetFormulariosAsync(cancellationToken);
        return Ok(formularios);
    }

    /// <summary>Exporta a Excel (.xlsx) todos los leads, opcionalmente filtrados por formulario.</summary>
    [HttpGet("export")]
    [Authorize(Policy = Permisos.LeadsRead)]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Export(
        [FromQuery] string? formulario,
        CancellationToken cancellationToken)
    {
        var contenido = await _leadService.ExportarLeadsAsync(formulario, cancellationToken);

        var sufijo = string.IsNullOrWhiteSpace(formulario) ? "todos" : formulario;
        var nombreArchivo = $"leads_{sufijo}_{DateTime.UtcNow:yyyy-MM-dd}.xlsx";

        return File(
            contenido,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombreArchivo);
    }

    /// <summary>Obtiene un lead (objeto dinámico) por su identificador.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permisos.LeadsRead)]
    [ProducesResponseType(typeof(Dictionary<string, object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IDictionary<string, object?>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var lead = await _leadService.GetByIdAsync(id, cancellationToken);
        return lead is null ? NotFound() : Ok(lead);
    }

    /// <summary>Lista los archivos asociados a un lead.</summary>
    [HttpGet("{id:guid}/archivos")]
    [Authorize(Policy = Permisos.LeadsRead)]
    [ProducesResponseType(typeof(IReadOnlyList<ArchivoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ArchivoResponse>>> GetArchivos(Guid id, CancellationToken cancellationToken)
    {
        var archivos = await _leadService.GetArchivosByLeadAsync(id, cancellationToken);
        return Ok(archivos);
    }

    /// <summary>Devuelve los campos crudos de un lead (nombre, valor, orden, alias). Útil para edición fiel.</summary>
    [HttpGet("{id:guid}/campos")]
    [Authorize(Policy = Permisos.LeadsRead)]
    [ProducesResponseType(typeof(IReadOnlyList<CampoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CampoDTO>>> GetCampos(Guid id, CancellationToken cancellationToken)
    {
        var campos = await _leadService.GetCamposByLeadAsync(id, cancellationToken);
        return campos is null ? NotFound() : Ok(campos);
    }

    /// <summary>Actualiza un lead (formulario y campos). Los archivos se gestionan aparte.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permisos.LeadsUpdate)]
    [ProducesResponseType(typeof(Dictionary<string, object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IDictionary<string, object?>>> Update(
        Guid id,
        [FromBody] UpdateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var actualizado = await _leadService.UpdateAsync(id, request, cancellationToken);
        return actualizado is null ? NotFound() : Ok(actualizado);
    }

    /// <summary>Elimina un lead, sus archivos en BD (cascada) y los binarios en disco.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permisos.LeadsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await _leadService.DeleteAsync(id, cancellationToken);
        return eliminado ? NoContent() : NotFound();
    }

    /// <summary>Descarga el contenido binario de un archivo asociado a un lead.</summary>
    [HttpGet("archivos/{archivoId:guid}")]
    [Authorize(Policy = Permisos.LeadsRead)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DescargarArchivo(Guid archivoId, CancellationToken cancellationToken)
    {
        var archivo = await _leadService.GetArchivoContenidoAsync(archivoId, cancellationToken);
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    /// <summary>Elimina un archivo individual (fila en BD y binario en disco).</summary>
    [HttpDelete("archivos/{archivoId:guid}")]
    [Authorize(Policy = Permisos.LeadsDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarArchivo(Guid archivoId, CancellationToken cancellationToken)
    {
        var eliminado = await _leadService.DeleteArchivoAsync(archivoId, cancellationToken);
        return eliminado ? NoContent() : NotFound();
    }
}

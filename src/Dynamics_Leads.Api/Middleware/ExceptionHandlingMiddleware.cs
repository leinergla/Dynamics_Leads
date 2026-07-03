using Microsoft.AspNetCore.Mvc;

namespace Dynamics_Leads.Api.Middleware;

/// <summary>
/// Convierte excepciones conocidas en respuestas ProblemDetails con el código HTTP adecuado.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                FileNotFoundException => (StatusCodes.Status404NotFound, "Archivo no encontrado"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(ex, "Error no controlado al procesar la solicitud.");
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError ? "Ocurrió un error inesperado." : ex.Message
            };

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}

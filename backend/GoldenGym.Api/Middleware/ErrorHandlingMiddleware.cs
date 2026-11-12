using System.Text.Json;
using GoldenGym.Application.DTOs;
using GoldenGym.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GoldenGym.Api.Middleware;

/// <summary>
/// Convierte las excepciones en respuestas JSON con un formato único (RNF-08):
/// { "status": 409, "mensaje": "...", "errores": null, "exito": false }
/// </summary>
public class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            var (status, mensaje) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                ConflictException => (StatusCodes.Status409Conflict, ex.Message),
                BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, ex.Message),
                ForbiddenException => (StatusCodes.Status403Forbidden, ex.Message),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, ex.Message),
                DbUpdateException => (StatusCodes.Status409Conflict,
                    "No se pudo guardar: los datos violan una restricción de la base de datos (registro duplicado o relacionado)."),
                _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado en el servidor.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);
            else
                _logger.LogInformation("Solicitud rechazada ({Status}): {Mensaje}", status, ex.Message);

            await EscribirErrorAsync(context, status, mensaje);
        }
    }

    public static async Task EscribirErrorAsync(HttpContext context, int status, string mensaje,
        Dictionary<string, string[]>? errores = null)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new ErrorResponse(status, mensaje, errores), Json));
    }
}

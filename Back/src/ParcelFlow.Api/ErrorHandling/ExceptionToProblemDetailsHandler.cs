using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ParcelFlow.BuildingBlocks.Application.Exceptions;
using ParcelFlow.BuildingBlocks.Domain;

namespace ParcelFlow.Api.ErrorHandling;

/// <summary>
/// Traduce las excepciones de nuestras capas a códigos HTTP, en UN solo lugar.
/// Gracias a esto los endpoints no tienen try/catch: solo describen el "camino feliz".
///
///   ArgumentException   (dato inválido)              → 400 Bad Request
///   NotFoundException   (no existe)                  → 404 Not Found
///   DomainException     (regla de negocio, estado)   → 409 Conflict
///   DbUpdateConcurrency (otro lo cambió antes)       → 409 Conflict
///   cualquier otra      (bug, BD caída...)           → 500 genérico, sin detalles internos
///
/// El cuerpo es un ProblemDetails con un campo extra "code" (ej. "shipment.already_cancelled")
/// para que el front decida qué hacer sin depender del texto del mensaje.
/// </summary>
internal sealed class ExceptionToProblemDetailsHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, code) = exception switch
        {
            // JSON mal formado o cuerpo que no se puede leer (lo lanza ASP.NET al hacer el binding).
            BadHttpRequestException bad => (bad.StatusCode, "Petición inválida", "request.invalid"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Datos inválidos", "validation.invalid"),
            NotFoundException notFound => (StatusCodes.Status404NotFound, "No encontrado", notFound.Code),
            DomainException domain => (StatusCodes.Status409Conflict, "Operación no permitida", domain.Code),
            // Otro usuario modificó lo mismo justo antes (concurrencia optimista con xmin).
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflicto de edición", "concurrency.conflict"),
            _ => (0, "", ""),
        };

        // false = "no es mío": el middleware responde 500 y registra el error en el log.
        if (status == 0)
            return false;

        httpContext.Response.StatusCode = status;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Extensions = { ["code"] = code },
            },
        };

        // Qué campo falló (ej. "Kilograms"): útil para que el front marque el input en rojo.
        if (exception is ArgumentException { ParamName: { } field })
            context.ProblemDetails.Extensions["field"] = field;

        return await problemDetailsService.TryWriteAsync(context);
    }
}

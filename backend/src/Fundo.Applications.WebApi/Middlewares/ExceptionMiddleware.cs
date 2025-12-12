using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Middlewares
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
 
                _logger.LogWarning(ex,
                    "Validation failed. {Method} {Path}. TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.TraceIdentifier);

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

                var payload = new
                {
                    title = "Validation failed",
                    status = context.Response.StatusCode,
                    traceId = context.TraceIdentifier,
                    errors
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
            catch (UnauthorizedAccessException ex)
            {

                _logger.LogWarning(ex,
                    "Unauthorized. {Method} {Path}. TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.TraceIdentifier);

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    title = "Unauthorized",
                    status = context.Response.StatusCode,
                    traceId = context.TraceIdentifier,
                    detail = ex.Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex,
                    "Business rule violation. {Method} {Path}. TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.TraceIdentifier);

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    title = "Business rule violation",
                    status = context.Response.StatusCode,
                    traceId = context.TraceIdentifier,
                    detail = ex.Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
            catch (Exception ex)
            {

                _logger.LogError(ex,
                    "Unhandled exception. {Method} {Path}. TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.TraceIdentifier);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    title = "Internal Server Error",
                    status = context.Response.StatusCode,
                    traceId = context.TraceIdentifier,
                    detail = _env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred."
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }
}

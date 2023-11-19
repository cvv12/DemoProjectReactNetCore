using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace webapi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var requestPath = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

                _logger.LogDebug($"Actual request path: {requestPath}");
                _logger.LogInformation($"Request {context.Request?.Method} {requestPath} processed in {stopwatch.ElapsedMilliseconds} ms");
            }
        }


    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace webapi.Middleware
{
    public class RateLimitingMiddleware //for demo purpose,only apply for update balance
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private const int REQUEST_LIMIT = 2; 
        private const int EXPIRATION_TIME_MINUTES = 1; 

        public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.EndsWith("/UpdateAccount", StringComparison.OrdinalIgnoreCase) &&
                context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                var key = $"RateLimit_{context.Request.Path}_{context.Connection.RemoteIpAddress}";
                var count = _cache.GetOrCreate(key, entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(EXPIRATION_TIME_MINUTES);
                    return 0;
                });

                if (count >= REQUEST_LIMIT)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Too often. Account balance can only be updated twice every minute.");
                    return;
                }

                _cache.Set(key, count + 1, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(EXPIRATION_TIME_MINUTES)
                });
            }

            await _next(context);
        }
    }
}

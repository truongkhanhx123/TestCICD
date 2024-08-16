using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using testbook.ConfigurationClasses;

namespace testbook.MiddleWare
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        public RateLimitMiddleware(RequestDelegate next, IDistributedCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var rateLimitAttribute = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();
            if (rateLimitAttribute == null)
            {
                await _next(context);
                return;
            }
            string cacheKey = $"rate_limit_{endpoint}";

            var value = await _cache.GetStringAsync(cacheKey);
            int requestCount = string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
            if (requestCount >= rateLimitAttribute.Limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync($"You have exceeded the allowed request limit, please try again later {rateLimitAttribute.WindowTime} seconds");
                string logg = $"This user has logged in {rateLimitAttribute.Limit} times exceeding the allowed limit and is allowed to log in again after {rateLimitAttribute.WindowTime} seconds.";
                Log.Warning(logg);
                return;
            }

            await _cache.SetStringAsync(cacheKey, (requestCount + 1).ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(rateLimitAttribute.WindowTime)
            });

            await _next(context);
        }
    }
}

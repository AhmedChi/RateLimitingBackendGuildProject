using RateLimitingBackendGuildProject.ApiService.Config;
using RateLimitingBackendGuildProject.ApiService.Data.Models;

namespace RateLimitingBackendGuildProject.ApiService.Middleware
{
    public class RateLimitMiddleware(RequestDelegate next, TokenBucketStore bucketStore)
    {
        public async Task InvokeAsync(HttpContext context, ClientStore clientStore)
        {
            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();

            if(apiKey == null) {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: API key is missing.");
                return;
            }

            var value = new RateLimitTokenBucket
            {
                LastRefillTime = DateTime.UtcNow
            };

            var buckets = bucketStore.Buckets.GetOrAdd(apiKey, value);

            lock (buckets)
            {
                var currentTime = DateTime.UtcNow;

                var elapsedTime = currentTime - buckets.LastRefillTime;

                var totalTokensGeneratedPerSecond = elapsedTime.TotalSeconds * buckets.RefillRatePerSecond;

                buckets.Tokens = Math.Min(buckets.Capacity, buckets.Tokens + (int)totalTokensGeneratedPerSecond);
                buckets.LastRefillTime = currentTime;
                
                buckets.Tokens = Math.Max(0, buckets.Tokens-1);
                
            }

            if (buckets.Tokens == 0)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too Many Requests: Rate limit exceeded.");
                return;
            }

            await next(context);
        }
    }
}

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

                var elapsedTime = currentTime - value.LastRefillTime;

                var totalTokensGeneratedPerSecond = elapsedTime.TotalSeconds * value.RefillRatePerSecond;

                value.Tokens = Math.Min(value.Capacity, value.Tokens + (int)totalTokensGeneratedPerSecond);
                value.LastRefillTime = currentTime;
            }

        }
    }
}

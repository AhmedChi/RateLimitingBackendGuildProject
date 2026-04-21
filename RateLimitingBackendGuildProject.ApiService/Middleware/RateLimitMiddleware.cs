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

            var shouldproceed = true;

            lock (buckets)
            {
                var currentTime = DateTime.UtcNow;

                var elapsedTime = currentTime - buckets.LastRefillTime;

                var totalTokensGeneratedPerSecond = elapsedTime.TotalSeconds * buckets.RefillRatePerSecond;

                buckets.Tokens = Math.Min(buckets.Capacity, buckets.Tokens + (int)totalTokensGeneratedPerSecond);
                buckets.LastRefillTime = currentTime;

                if (buckets.Tokens > 0)
                {
                    shouldproceed = true;
                    buckets.Tokens = Math.Max(0, buckets.Tokens - 1);
                }
                else
                {
                    shouldproceed = false;
                }
            }

            if (shouldproceed)
            {
                await next(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Too Many Requests: Rate limit exceeded.");
                return;
            }
        }
    }
}

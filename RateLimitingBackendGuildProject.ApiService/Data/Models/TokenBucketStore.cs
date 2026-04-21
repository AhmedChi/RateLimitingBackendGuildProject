using System.Collections.Concurrent;

namespace RateLimitingBackendGuildProject.ApiService.Data.Models
{
    public class TokenBucketStore
    {
        public ConcurrentDictionary<string, RateLimitTokenBucket> Buckets { get; set; } = new ConcurrentDictionary<string, RateLimitTokenBucket>();
    }
}

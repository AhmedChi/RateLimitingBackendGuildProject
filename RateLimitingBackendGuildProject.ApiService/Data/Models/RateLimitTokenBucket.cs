namespace RateLimitingBackendGuildProject.ApiService.Data.Models
{
    public class RateLimitTokenBucket
    {
        public int Capacity { get; set; } = 100;
        public int RateLimit { get; set; } = 100;
        public double Tokens { get; set; } = 100;
        public DateTime LastRefillTime { get; set; } = DateTime.UtcNow;
        public double RefillRatePerSecond => RateLimit / 60.0;
    }
}

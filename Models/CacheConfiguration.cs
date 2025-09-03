namespace WebApplication1.Models
{
    public class CacheConfiguration
    {
        public bool EnableCache { get; set; } = true;
        public int DefaultExpirationMinutes { get; set; } = 20;
        public int MaxCacheSize { get; set; } = 1000;
        public bool EnableStatistics { get; set; } = true;
        public bool EnableLogging { get; set; } = true;
        
        // Specific cache settings
        public CacheSettings Permissions { get; set; } = new() { ExpirationMinutes = 5 };
        public CacheSettings Configuration { get; set; } = new() { ExpirationMinutes = 30 };
        public CacheSettings Menu { get; set; } = new() { ExpirationMinutes = 10 };
        public CacheSettings User { get; set; } = new() { ExpirationMinutes = 15 };
        public CacheSettings Reports { get; set; } = new() { ExpirationMinutes = 60 };
    }

    public class CacheSettings
    {
        public bool Enabled { get; set; } = true;
        public int ExpirationMinutes { get; set; } = 20;
        public bool SlidingExpiration { get; set; } = false;
        public int MaxItems { get; set; } = 100;
    }
} 
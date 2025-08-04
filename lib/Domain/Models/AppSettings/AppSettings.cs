namespace Domain.Models.AppSettings;

public class AppSettings
{
    public Logging Logging { get; set; }
    public string AllowedHosts { get; set; }
    public RateLimit RateLimit { get; set; }
}

public class Logging
{
    public LogLevel LogLevel { get; set; }
}

public class LogLevel
{
    public string Default { get; set; }
    public string MicrosoftAspNetCore { get; set; }
}

public class RateLimit
{
    public bool EnableRateLimiting { get; set; }
    public int MaxRequests { get; set; }
    public double TimeWindowSeconds { get; set; }
}


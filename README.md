# Rate Limit Implementation Guide

## Overview

This service uses a custom `[RateLimit]` attribute to restrict the number of requests a client can make to specific endpoints within a given time window. This helps prevent abuse and ensures fair usage.

## How It Works

- The `[RateLimit]` attribute is applied to controller actions.
- When a request is made, the middleware checks if the client has exceeded the allowed number of requests.
- If the limit is exceeded, the service returns HTTP 429 (Too Many Requests).
- Otherwise, the request is processed normally.

## Example Usage

In `HomeController.cs`:

```csharp
[AllowAnonymous]
[HttpGet]
[RateLimit]
public IActionResult Get()
{
    return Ok(new
    {
        Status = StatusCodes.Status200OK,
        Message = "Welcome to the Rate Limit Service",
        Timestamp = DateTime.UtcNow
    });
}
```

## RateLimitAttribute Class

The `RateLimitAttribute` is a custom attribute that enforces rate limiting on controller actions.  
It uses `IMemoryCache` and configuration settings to track requests per client and endpoint.

### Example Declaration

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RateLimitAttribute : ActionFilterAttribute, IAsyncActionFilter
{
    // Constructor
    public RateLimitAttribute() { }

    // Main logic for rate limiting
    async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // ...rate limiting logic...
    }

    // Helper methods for configuration and IP address
    private static bool IsRateLimitExceededAsync(IConfiguration? configuration) { ... }
    private static string GetClientIpAddress(HttpContext context) { ... }
    private async Task<RateLimit> GetRateLimitAsync(IConfiguration configuration) { ... }
}
```

### How to Use

Apply `[RateLimit]` to any controller action you want to protect:

```csharp
[RateLimit]
public IActionResult Get()
{
    // Your endpoint logic
}
```

### How It Works

- Checks if rate limiting is enabled via configuration.
- Tracks requests per endpoint and client IP.
- Returns HTTP 429 if the limit is exceeded.
- Otherwise, increments the request count and allows the request.

### Configuration Example

Add to `appsettings.json`:

```json
"RateLimit": {
  "EnableRateLimiting": true,
  "MaxRequests": 5,
  "TimeWindowSeconds": 60
}
```

### Notes

- The attribute should be registered and used on controller actions.
- For distributed scenarios, replace `IMemoryCache` with a distributed cache.

## Steps to Implement

1. **Add the `[RateLimit]` attribute** to any controller action you want to protect.
2. **Configure the rate limit logic** in your middleware or attribute implementation (not shown here).
3. **Test the endpoint** by making repeated requests. After exceeding the limit, you should receive a 429 response.

## Customization

- You can adjust the rate limit window and request count in the middleware or attribute logic.
- For distributed scenarios, consider using Redis or another distributed cache.

## Troubleshooting

- If rate limiting does not work, ensure the middleware or attribute is registered and applied correctly.
- Check logs for errors related to rate limit enforcement.

## References

- https://github.com/edoprayogo/rate-limit-dotnet-service/tree/main

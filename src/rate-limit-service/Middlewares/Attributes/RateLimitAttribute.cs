using Domain.Models.AppSettings;
using Domain.Payloads.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace rate_limit_service.Middlewares.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RateLimitAttribute : ActionFilterAttribute, IAsyncActionFilter
{
    public RateLimitAttribute()
    {

    }

    async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var svc = context.HttpContext.RequestServices;
        var memoCache = svc.GetService<IMemoryCache>();
        var configuration = svc.GetService<IConfiguration>();

        if (memoCache == null || configuration == null)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        bool isRateLimitExceeded = IsRateLimitExceededAsync(configuration);

        if (isRateLimitExceeded)
        {
            string methodName = context.HttpContext.Request.Path.Value?.Split('/').LastOrDefault() ?? "Unknown";
            string ipAdressClient = GetClientIpAddress(context.HttpContext);
            var rateLimit = await GetRateLimitAsync(configuration);

            string keycacheKey = $"{methodName}:{ipAdressClient}";

            if (memoCache.TryGetValue(keycacheKey, out int requestCount))
            {
                if (requestCount > rateLimit.MaxRequests)
                {
                    var contextResult = new StatusResponse(
                        StatusCodes.Status429TooManyRequests,
                        $"Rate limit exceeded. You can only make {rateLimit.MaxRequests} requests in {rateLimit.TimeWindowSeconds} seconds."
                    );

                    context.Result = new ContentResult
                    {
                        Content = System.Text.Json.JsonSerializer.Serialize(contextResult),
                        ContentType = "application/json",
                        StatusCode = StatusCodes.Status429TooManyRequests
                    };
                    return;

                }
                else
                {
                    requestCount++;
                    memoCache.Set(keycacheKey, requestCount, TimeSpan.FromSeconds(rateLimit.TimeWindowSeconds));
                }
            }
            else
            {
                memoCache.Set(keycacheKey, 1, TimeSpan.FromSeconds(rateLimit.TimeWindowSeconds));
            }

        }
        await next();

    }

    private static bool IsRateLimitExceededAsync(IConfiguration? configuration)
    {
        bool isExceeded = false;
        try
        {
            if (configuration != null)
            {
                isExceeded = configuration.GetValue<bool>("RateLimit:EnableRateLimiting");
            }
        }
        catch (Exception)
        {
            isExceeded = false;
        }
        return isExceeded;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        if (context.Connection.RemoteIpAddress != null)
        {
            return context.Connection.RemoteIpAddress.ToString();
        }
        else
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? "Unknown";
            }
        }
        return "Unknown";
    }
    
    private async Task<RateLimit> GetRateLimitAsync(IConfiguration configuration)
    {
        RateLimit rateLimit = new RateLimit();
        try
        {
            rateLimit.EnableRateLimiting = configuration.GetValue<bool>("RateLimit:EnableRateLimiting");
            rateLimit.MaxRequests = configuration.GetValue<int>("RateLimit:MaxRequests");
            rateLimit.TimeWindowSeconds = configuration.GetValue<double>("RateLimit:TimeWindowSeconds");
        }
        catch (Exception)
        {
            // Handle exceptions or log them as needed
        }
        return await Task.FromResult(rateLimit);
    }

}

# 🚀 ASP.NET Core 8 – API Versioning & Rate Limiting

This project demonstrates:

- ✅ **API Versioning** → enabling endpoints with multiple versions (`/api/v1/...`, `/api/v2/...`)
- ✅ **Rate Limiting (Fixed Window)** → limiting the number of client requests within a given time window

---

## 📂 Project Structure

rate-limit-dotnet-service/
|
├── lib/
│ └── Domain/ # Domain layer (entities, business logic, etc.)
|
└── src/
└── rate-limit-service/
├── Controllers/
│ ├── v1/ # API version 1
│ │ └── SampleController.cs
│ └── v2/ # API version 2
│ └── SampleController.cs
│
├── Middlewares/
│ ├── Attributes/ # Custom attributes (if any)
│ ├── Extensions/ # Extension methods (RateLimiter, Versioning)
│ │ ├── RateLimiterExtensions.cs
│ │ └── VersioningExtensions.cs
│ └── Options/ # Additional configuration
│
├── appsettings.json
├── Program.cs
└── rate-limit-service.http # Request testing file


---

## ⚡ API Versioning

### 🔧 Configuration (`VersioningExtensions.cs`)

```csharp
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

public static class VersioningExtensions
{
    public static IServiceCollection AddCustomVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version")
            );
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
```

➡️ Register in Program.cs:
```csharp
builder.Services.AddCustomVersioning();
```

📡 Controllers
🔹 API v1

File: Controllers/v1/SampleController.cs
```csharp
using Microsoft.AspNetCore.Mvc;

namespace rate_limit_service.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet]
    public IActionResult GetV1() => Ok("Hello from API v1");
}

```
📌 Endpoint:

GET /api/v1/sample
🔹 API v2

File: Controllers/v2/SampleController.cs

using Microsoft.AspNetCore.Mvc;

namespace rate_limit_service.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet]
    public IActionResult GetV2() => Ok("Hello from API v2 🚀");
}


📌 Endpoint:

GET /api/v2/sample

🚦 Rate Limiting (Fixed Window)
🔧 Configuration (RateLimiterExtensions.cs)
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;                     // Max 5 requests
                limiterOptions.Window = TimeSpan.FromMinutes(1);    // Per 1 minute
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;                      // No queue
            });

            // Custom response when rate limit is exceeded
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    "{\\\"error\\\": \\\"Rate limit exceeded. Try again later.\\\"}", token);
            };
        });

        return services;
    }
}


➡️ Register in Program.cs:

builder.Services.AddCustomRateLimiter();
app.UseRateLimiter();

🧪 Testing

Run the application:

dotnet run --project src/rate-limit-service

✅ Test API Versioning

API v1 → GET /api/v1/sample → returns Hello from API v1

API v2 → GET /api/v2/sample → returns Hello from API v2 🚀

✅ Test Rate Limiting

Send 6 quick requests to /api/v1/sample (or /api/v2/sample):

Requests 1–5 → 200 OK

Request 6 → 429 Too Many Requests

📖 Summary

With this setup, you get:

Separate API versions (v1 & v2) for backward compatibility.

Fixed window rate limiting to prevent abuse.

Clean architecture & extensible structure for future features.


---



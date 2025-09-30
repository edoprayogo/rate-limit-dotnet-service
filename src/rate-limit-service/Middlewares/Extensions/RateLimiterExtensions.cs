using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace rate_limit_service.Middlewares.Extensions
{
    public static class RateLimiterExtensions
    {
        public static IServiceCollection AddCustomRateLimiters(this IServiceCollection services)
        {
            // Fixed Window
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                // Custom response
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        "{\"error\": \"Rate limit exceeded. Try again later.\"}", token);
                };
            });

            // Sliding Window
            services.AddRateLimiter(options =>
                options.AddSlidingWindowLimiter("sliding", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.SegmentsPerWindow = 10;
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 5;
                }));

            // Token Bucket
            services.AddRateLimiter(options =>
                options.AddTokenBucketLimiter("token", limiterOptions =>
                {
                    limiterOptions.TokenLimit = 100;
                    limiterOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
                    limiterOptions.TokensPerPeriod = 10;
                    limiterOptions.AutoReplenishment = true;
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 5;
                }));

            // Concurrency
            services.AddRateLimiter(options =>
                options.AddConcurrencyLimiter("concurrency", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 5;
                }));

            // Global Chained Limiter
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
                        return RateLimitPartition.GetFixedWindowLimiter(
                            userAgent, _ => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 4,
                                Window = TimeSpan.FromSeconds(2)
                            });
                    }),
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        var clientIP = httpContext.Connection.RemoteIpAddress!.ToString();
                        return RateLimitPartition.GetFixedWindowLimiter(
                            clientIP, _ => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 20,
                                Window = TimeSpan.FromSeconds(30)
                            });
                    }));
            });

            return services;
        }
    }

}

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using rate_limit_service.Middlewares.Attributes;

namespace rate_limit_service.Controllers.v2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {

        [AllowAnonymous]
        [HttpGet("limit")]
        [EnableRateLimiting("fixed")]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = StatusCodes.Status200OK,
                Message = "Welcome to the Rate Limit Service",
                Timestamp = DateTime.UtcNow
            });
        }

        [DisableRateLimiting]
        [AllowAnonymous]
        [HttpGet("no-limit")]
        public IActionResult GetNoRateLimit()
        {
            return Ok(new
            {
                Status = StatusCodes.Status200OK,
                Message = "Welcome to the Rate Limit Service",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

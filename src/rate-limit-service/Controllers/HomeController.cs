using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rate_limit_service.Middlewares.Attributes;

namespace rate_limit_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {

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
    }
}

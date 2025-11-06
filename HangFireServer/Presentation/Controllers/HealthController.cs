using Microsoft.AspNetCore.Mvc;

namespace HangFireServer.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Endpoint para verificar si la API está levantada
        /// </summary>
        /// <returns>200 OK con mensaje</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                message = "API is running",
                timestamp = DateTime.UtcNow
            });
        }
    }
}

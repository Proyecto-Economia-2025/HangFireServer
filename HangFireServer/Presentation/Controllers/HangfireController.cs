using HangFireServer.Core.Absttractions;
using HangFireServer.Core.DTOs; 
using HangFireServer.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HangFireServer.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HangfireController : ControllerBase
    {
        private readonly ITopProductsService _topProductsService;

        public HangfireController(ITopProductsService topProductsService)
        {
            _topProductsService = topProductsService;
        }

        [HttpPost("top-products")]
        public async Task<IActionResult> ProcessTopProducts([FromBody] TopProductsRequest request)
        {
            try
            {
                var result = await _topProductsService.JobsProcessTopProducts(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace LongRunningTasks.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;

        public TaskController(
            ILogger<TaskController> logger)
        {
            _logger = logger;
        }

        [HttpGet("version")]
        public IActionResult GetVesion()
        {
            return Ok("v3");
        }
    }
}
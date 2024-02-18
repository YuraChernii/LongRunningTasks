using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LongRunningTasks.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TaskController> _logger;

        public TaskController(
            IMediator mediator,
            ILogger<TaskController> logger
            )
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("version")]
        public IActionResult GetVesion()
        {
            return Ok("v3");
        }
    }
}
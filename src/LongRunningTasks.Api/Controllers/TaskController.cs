using LongRunningTasks.Application.Commands.TestINotification;
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

        [HttpPost("notification")]
        public async Task<IActionResult> TestINotification(TestINotification testINotification)
        {
            await _mediator.Publish(testINotification);

            return Ok();
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVesion()
        {
            return Ok("v2");
        }
    }
}
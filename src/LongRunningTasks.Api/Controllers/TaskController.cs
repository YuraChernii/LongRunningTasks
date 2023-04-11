using LongRunningTasks.Application.Commands.TestBackgroundService;
using LongRunningTasks.Application.Commands.TestHangFire;
using LongRunningTasks.Application.Commands.TestINotification;
using LongRunningTasks.Application.Commands.TestObservable;
using LongRunningTasks.Application.Commands.TestRabbitMQ;
using LongRunningTasks.Application.DTOs;
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

        [HttpPost("version")]
        public async Task<IActionResult> GetVersion(TestBackgroundService setEmailReminder)
        {
            return Ok("statging");
        }

        
    }
}
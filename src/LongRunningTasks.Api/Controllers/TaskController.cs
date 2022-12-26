using LongRunningTasks.Application.Commands.TestBackgroundService;
using LongRunningTasks.Application.Commands.TestHangFire;
using LongRunningTasks.Application.Commands.TestObservable;
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

        [HttpPost("background-service")]
        public async Task<IActionResult> TestBackgroundService(TestBackgroundService setEmailReminder)
        {
            var result = await _mediator.Send(setEmailReminder);

            return Ok(result);
        }

        [HttpPost("hangfire")]
        public async Task<IActionResult> TestHangFire(TestHangFire testHangFire)
        {
            var result = await _mediator.Send(testHangFire);

            return Ok(result);
        }

        [HttpPost("observable")]
        public async Task<IActionResult> TestObservable(TestObservable testObservable)
        {
            var result = await _mediator.Send(testObservable);

            return Ok(result);
        }

    }
}
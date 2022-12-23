using LongRunningTasks.Application.Commands.EnqueueTask;
using LongRunningTasks.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LongRunningTasks.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReminderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ReminderController> _logger;

        public ReminderController(
            IMediator mediator,
            ILogger<ReminderController> logger
            )
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> SetReminder(EnqueueTask setEmailReminder)
        {
            var result = await _mediator.Send(setEmailReminder);

            return Ok(result);
        }

    }
}
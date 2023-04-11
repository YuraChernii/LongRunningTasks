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
        private readonly ILogger<TaskController> _logger;

        public TaskController(
            ILogger<TaskController> logger
            )
        {
            _logger = logger;
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVersion(TestBackgroundService setEmailReminder)
        {
            return Ok("prod");
        }

        
    }
}
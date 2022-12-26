using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Commands.TestBackgroundService
{
    internal class TestBackgroundServiceCommand : IRequestHandler<TestBackgroundService, TestPipeDTO>
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<TestBackgroundServiceCommand> _logger;
        public TestBackgroundServiceCommand(IBackgroundTaskQueue taskQueue, ILogger<TestBackgroundServiceCommand> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        public async Task<TestPipeDTO> Handle(TestBackgroundService request, CancellationToken cancellationToken)
        {
            await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItem);

            return new TestPipeDTO();
        }

        private async Task BuildWorkItem(CancellationToken token)
        {
            var id = Guid.NewGuid();

            _logger.LogInformation("Queued Background Task with {id} has started.", id);

            await Task.Delay(TimeSpan.FromSeconds(5), token);

            _logger.LogInformation("Queued Background Task with {id} has completed.", id);
        }
    }
}

using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Commands.EnqueueTask
{
    internal class EnqueueTaskCommand : IRequestHandler<EnqueueTask, EnqueueTaskDTO>
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<EnqueueTaskCommand> _logger;
        public EnqueueTaskCommand(IBackgroundTaskQueue taskQueue, ILogger<EnqueueTaskCommand> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        public async Task<EnqueueTaskDTO> Handle(EnqueueTask request, CancellationToken cancellationToken)
        {
            await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItem);

            return new EnqueueTaskDTO();
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

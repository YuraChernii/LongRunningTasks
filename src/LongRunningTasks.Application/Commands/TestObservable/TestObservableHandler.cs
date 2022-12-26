using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Commands.TestObservable
{
    internal class TestObservableHandler : IRequestHandler<TestObservable, TestObservableQueueDTO>
    {
        private readonly ILogger<TestObservableHandler> _logger;
        private readonly IPipeService _pipeService;

        public TestObservableHandler(ILogger<TestObservableHandler> logger, IPipeService pipeService)
        {
            _logger = logger;
            _pipeService = pipeService;
        }

        public async Task<TestObservableQueueDTO> Handle(TestObservable request, CancellationToken cancellationToken)
        {
            var func = new Func<CancellationToken, Task>(async (CancellationToken token) =>
            {
                _logger.LogInformation($"Observable task was started.");
                await Task.Delay(5000);
                _logger.LogInformation($"Observable task has been completed.");
            });

            _pipeService.Submit(func);

            return new TestObservableQueueDTO();
        }
    }
}

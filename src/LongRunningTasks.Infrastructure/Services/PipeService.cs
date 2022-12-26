using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class PipeService : IPipeService, IDisposable
    {
        private readonly ILogger<PipeService> _logger;
        private readonly PipeConfig _pipeConfig;
        private readonly ISubject<Func<CancellationToken, Task>> _pipe;
        private readonly IDisposable _sub;


        public PipeService(ILogger<PipeService> logger, IOptions<PipeConfig> pipeConfig)
        {
            _logger = logger;
            _pipeConfig = pipeConfig.Value;

            var pipe = new Subject<Func<CancellationToken, Task>>();

            _sub = pipe.ObserveOn(TaskPoolScheduler.Default)
                .Buffer(TimeSpan.FromSeconds(_pipeConfig.BufferSeconds))
                .Where(list => list.Any())
                .SelectMany(list => list)
                .Select(func => Observable.FromAsync(token => func(token)))
                .Merge(_pipeConfig.NumberOfParallelFlows)
                .Subscribe(
                _ =>
                {
                    _logger.LogInformation("onNext func was called.");
                },
                err => _logger.LogError(err, "Error in pipe:{0}", Environment.NewLine),
                () => _logger.LogInformation("Pipe was completed.")
                );

            _pipe = Subject.Synchronize(pipe);
        }

        public void Submit(Func<CancellationToken, Task> func)
        {
            _pipe.OnNext(func);
        }

        public void Dispose()
        {
            _pipe.OnCompleted();
            _sub.Dispose();
        }

    }
}

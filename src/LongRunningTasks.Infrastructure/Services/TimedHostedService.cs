

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LongRunningTasks.Infrastructure.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private Timer? _timer = null;

        public TimedHostedService(ILogger<TimedHostedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            System.Timers.Timer timer = new(interval: 3000);
            timer.Elapsed += async (sender, e) => await DoWork();
            timer.Start();

            // we must return Task.CompletedTask otherwise
            // other services and booting will wait until this method returns Task.CompletedTask.
            // That is why we do not use here await keyword!!!
            // It is usefull when we wanna wait for example for doing some migrations in database
            return Task.CompletedTask;
        }

        private async Task DoWork()
        {
            var count = Interlocked.Increment(ref executionCount);
            var currentThreadId = Thread.CurrentThread.ManagedThreadId;
            var currentProcessId = Process.GetCurrentProcess().Id;

            await Task.Delay(5000); //do smth long time running

            _logger.LogInformation(
                "Timed Hosted Service is working. " +
                "Count: {Count}. " +
                "Thread with id: {id}. " +
                "Process with id: {id}", 
                count, currentThreadId, currentProcessId);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

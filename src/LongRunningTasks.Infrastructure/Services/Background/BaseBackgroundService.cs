using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal abstract class BaseBackgroundService<T> : BackgroundService
    {
        protected readonly ILogger<T> _logger;

        public BaseBackgroundService(ILogger<T> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await TryExecuteAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Empty);
                }
            }
        }

        protected abstract Task TryExecuteAsync(CancellationToken cancellationToken);
    }
}
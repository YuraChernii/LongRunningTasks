using LongRunningTasks.Application.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal abstract class BaseBackgroundService<T> : BackgroundService
    {
        protected readonly ILogger<T> _logger;
        protected readonly IExceptionTelegramService _exceptionTelegramService;

        public BaseBackgroundService(
            ILogger<T> logger,
            IExceptionTelegramService exceptionTelegramService)
        {
            _logger = logger;
            _exceptionTelegramService = exceptionTelegramService;
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
                    await CatchAsync();
                    await _exceptionTelegramService.QueueExceptionNotification(ex);
                }
            }
        }

        protected abstract Task TryExecuteAsync(CancellationToken cancellationToken);

        protected virtual Task CatchAsync() => Task.CompletedTask;
    }
}
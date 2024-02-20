using LongRunningTasks.Application.Services;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class RetryService : IRetryService
    {
        private readonly Func<Exception, Task> _defaultCatcher = (Exception ex) => Task.CompletedTask;


        public async Task RetryAsync(Func<Task> action, int maxRetries = 5, int delayMilliseconds = 2000, Func<Exception, Task>? catchAsync = default)
        {
            catchAsync ??= _defaultCatcher;
            int attempt = 0;
            do
            {
                try
                {
                    await action();

                    return;
                }
                catch (Exception ex)
                {
                    if (++attempt >= maxRetries)
                    {
                        throw;
                    }

                    await Task.Delay(delayMilliseconds);
                    await catchAsync(ex);
                }
            } while (true);
        }
    }
}
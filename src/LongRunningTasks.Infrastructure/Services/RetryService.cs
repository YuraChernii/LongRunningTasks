using LongRunningTasks.Application.Services;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class RetryService : IRetryService
    {
        public async Task RetryAsync(Func<Task> action, int maxRetries = 5, int delayMilliseconds = 1000)
        {
            int attempt = 0;
            do
            {
                try
                {
                    await action();

                    return;
                }
                catch (Exception)
                {
                    if (++attempt >= maxRetries)
                    {
                        throw;
                    }

                    await Task.Delay(delayMilliseconds);
                }
            } while (true);
        }
    }
}
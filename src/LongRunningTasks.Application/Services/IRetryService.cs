namespace LongRunningTasks.Application.Services
{
    public interface IRetryService
    {
        public Task RetryAsync(Func<Task> action, int maxRetries = 5, int delayMilliseconds = 1000);
    }
}

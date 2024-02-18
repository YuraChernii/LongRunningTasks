namespace LongRunningTasks.Application.Services
{
    public interface IChannelService<T>
    {
        Task QueueAsync(T item);
        Task<T> DequeueAsync(CancellationToken cancellationToken);
    }
}
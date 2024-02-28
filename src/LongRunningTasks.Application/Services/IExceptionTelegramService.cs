namespace LongRunningTasks.Application.Services
{
    public interface IExceptionTelegramService
    {
        Task QueueExceptionNotification(Exception exception);
    }
}
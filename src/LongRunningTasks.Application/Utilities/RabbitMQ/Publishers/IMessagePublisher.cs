using LongRunningTasks.Application.Messages;

namespace LongRunningTasks.Application.Utilities
{
    public interface IMessagePublisher
    {
        public Task PublishAsync<TMessage>(string exchange, string routingKey, TMessage message) where TMessage: class, IMessage;
    }
}

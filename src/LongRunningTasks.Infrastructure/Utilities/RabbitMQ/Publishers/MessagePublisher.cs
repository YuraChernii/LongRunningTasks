using LongRunningTasks.Application.Utilities;
using LongRunningTasks.Application.Utilities.RabbitMQ.Connections;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LongRunningTasks.Infrastructure.Utilities.RabbitMQ.Publishers
{
    internal class MessagePublisher : IMessagePublisher
    {
        private readonly IModel _channel;

        public MessagePublisher(IChannelFactory channelFactory) => _channel = channelFactory.Create();

        Task IMessagePublisher.PublishAsync<TMessage>(string exchange, string routingKey, TMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();

            _channel.ExchangeDeclare(exchange, "topic", durable: false, autoDelete: false);
            _channel.BasicPublish(exchange, routingKey, properties, body);

            return Task.CompletedTask;
        }
    }
}

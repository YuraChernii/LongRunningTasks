using LongRunningTasks.Application.Utilities.RabbitMQ.Connections;
using LongRunningTasks.Application.Utilities.RabbitMQ.Subscribers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace LongRunningTasks.Infrastructure.Utilities.RabbitMQ.Subscribers
{
    internal sealed class MessageSubscriber : IMessageSubscriber
    {
        private readonly IModel _channel;
        public MessageSubscriber(IChannelFactory channelFactory) => _channel = channelFactory.Create();
        IMessageSubscriber IMessageSubscriber.SubscribeMessage<TMessage>(string queue, string routingKey, string exchange, Func<TMessage, BasicDeliverEventArgs, Task> handle)
        {
            _channel.ExchangeDeclare(exchange, "topic", durable: false, autoDelete: false);
            _channel.QueueDeclare(queue, durable: false, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue, exchange, routingKey);
            //_channel.BasicQos(1);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, eventArg) =>
            {
                var body = eventArg.Body.ToArray();
                var message = JsonSerializer.Deserialize<TMessage>(Encoding.UTF8.GetString(body));

                await handle(message, eventArg);
            };

            _channel.BasicConsume(queue, autoAck: true, consumer: consumer);

            return this;

        }
    }
}

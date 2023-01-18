using LongRunningTasks.Application.Messages;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Application.Utilities.RabbitMQ.Connections;
using LongRunningTasks.Application.Utilities.RabbitMQ.Subscribers;
using LongRunningTasks.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class MessagingBackgroundService : BackgroundService
    {
        private readonly IMessageSubscriber _messageSubscriber;
        private readonly ILogger<MessagingBackgroundService> _logger;
        private readonly IChannelFactory _channelFactory;
        public MessagingBackgroundService(
            IMessageSubscriber messageSubscriber,
            ILogger<MessagingBackgroundService> logger,
            IChannelFactory channelFactory)
        {
            _messageSubscriber = messageSubscriber;
            _logger = logger;
            _channelFactory = channelFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var _channel = _channelFactory.Create();
            _channel.ExchangeDeclare("Items", "topic", durable: false, autoDelete: false);

            _messageSubscriber
                .SubscribeMessage<ItemMessages>("multiple-item-message", "EU.#", "Items", (msg, args) =>
                {
                    _logger.LogInformation($"Received Item: {msg.ItemName}.");

                    return Task.CompletedTask;
                })
                .SubscribeMessage<ItemMessages>("single-item-message", "EU.*", "Items", (msg, args) =>
                {
                    _logger.LogInformation($"Received Item: {msg.ItemName}.");

                    return Task.CompletedTask;
                });

            _logger.LogInformation("RabbitMQ consumers have been launched.");

            return Task.CompletedTask;
        }


    }


}

using LongRunningTasks.Application.Services;
using LongRunningTasks.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class RabbitMQConsumerService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private IModel _channel;
        public RabbitMQConsumerService(IServiceProvider provider, ILogger<RabbitMQConsumerService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //using var scope = _provider.CreateScope();
            //var itemRepository = scope.ServiceProvider.GetService<IItemRepository>();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "user",
                Password = "user",
                VirtualHost = "/"
            };

            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();

            _channel.QueueDeclare("send-items", exclusive: false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, eventArgs) =>
            {
                _logger.LogInformation("Item has been received!");

                var body = eventArgs.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Item has been received!");
            };


            _channel.BasicConsume("send-items", autoAck:true, consumer);

            _logger.LogInformation("RabbitMQ consumer has been launched.");

            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            _channel.Dispose();
        }

    }


}

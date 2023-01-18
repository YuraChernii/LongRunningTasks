using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Messages;
using LongRunningTasks.Application.Utilities;
using LongRunningTasks.Application.Utilities.RabbitMQ.Connections;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Commands.TestRabbitMQ
{
    internal class TestRabbitMQHandler : IRequestHandler<TestRabbitMQ, TestRabbitMQDTO>
    {
        private readonly ILogger<TestRabbitMQHandler> _logger;
        private readonly IMessagePublisher _messagePublisher;


        public TestRabbitMQHandler(
            ILogger<TestRabbitMQHandler> logger,
            IMessagePublisher messagePublisher)
        {
            _logger = logger;
            _messagePublisher = messagePublisher;
        }

        public async Task<TestRabbitMQDTO> Handle(TestRabbitMQ request, CancellationToken cancellationToken)
        {
            var message = new ItemMessages(request.ItemName);

            await _messagePublisher.PublishAsync("Items", $"EU.{request.Country}", message);

            _logger.LogInformation("Item was sent with correlation id: ...");

            return new TestRabbitMQDTO();
        }
    }
}

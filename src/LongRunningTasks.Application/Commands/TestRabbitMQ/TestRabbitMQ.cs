using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Messages;
using MediatR;

namespace LongRunningTasks.Application.Commands.TestRabbitMQ
{
    public class TestRabbitMQ : IRequest<TestRabbitMQDTO>
    {
        public string ItemName { get; set; }
        public string Country { get; set; }

    }

}

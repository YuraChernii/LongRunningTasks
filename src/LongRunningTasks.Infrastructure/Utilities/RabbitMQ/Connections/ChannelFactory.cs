using LongRunningTasks.Application.Utilities.RabbitMQ.Connections;
using RabbitMQ.Client;

namespace LongRunningTasks.Infrastructure.Utilities.RabbitMQ.Connections
{
    public class ChannelFactory : IChannelFactory
    {
        private readonly IConnection _connection;
        private readonly ChannelAccessor _channelAccessor;

        public ChannelFactory(IConnection connection, ChannelAccessor channelAccessor)
        {
            _connection = connection;
            _channelAccessor = channelAccessor;
        }

        public IModel Create() => _channelAccessor.Channel ?? (_channelAccessor.Channel = _connection.CreateModel());
    }
}

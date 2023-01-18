using LongRunningTasks.Application.Messages;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Utilities.RabbitMQ.Subscribers
{
    public interface IMessageSubscriber
    {
        public IMessageSubscriber SubscribeMessage<TMessage>(string queue, string routingKey, string exchange,
            Func<TMessage, BasicDeliverEventArgs, Task> handle) where TMessage : class, IMessage;
    }
}

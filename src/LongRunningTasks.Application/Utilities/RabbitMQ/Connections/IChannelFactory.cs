using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Utilities.RabbitMQ.Connections
{
    public interface IChannelFactory
    {
        IModel Create();
    }
}

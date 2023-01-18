using Microsoft.AspNetCore.Http;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Utilities.RabbitMQ.Connections
{
    public class ChannelAccessor
    {
        private static ThreadLocal<ChannelHolder> _holder = new();

        public IModel Channel
        {
            get => _holder.Value?.Channel;

            set
            {
                var holder = _holder.Value;
                if (holder is not null)
                {
                    holder.Channel = null;
                }

                if (value is not null)
                {
                    _holder.Value = new ChannelHolder { Channel = value };
                }
            }
        }

        private class ChannelHolder
        {
            public IModel Channel;
        }

    }
}

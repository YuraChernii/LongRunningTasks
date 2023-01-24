using LongRunningTasks.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Utilities
{
    public class BackgroundTaskQueue<T> : IBackgroundTaskQueue<T>
    {
        private readonly Channel<T> _queue;

        public BackgroundTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<T>(options);
        }

        public async Task QueueBackgroundWorkItemAsync(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            await _queue.Writer.WriteAsync(item);
        }

        public async Task<IEnumerable<T>> DequeueAsync(
            CancellationToken cancellationToken, int amount = 1)
        {
            if (amount > 10 || amount < 1)
            {
                throw new Exception("Amount of dequeueing items is out of range.");
            }

            var items = new List<T>();

            for (int i = 0; i < amount; i++)
            {
                var item = await _queue.Reader.ReadAsync(cancellationToken);

                items.Add(item);

                if (_queue.Reader.Count == 0)
                {
                    break;
                }
            }

            return items;
        }

    }
}

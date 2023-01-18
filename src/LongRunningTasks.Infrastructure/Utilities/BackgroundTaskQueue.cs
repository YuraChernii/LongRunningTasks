using LongRunningTasks.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Utilities
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue;

        public BackgroundTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
        }

        public async Task QueueBackgroundWorkItemAsync(
            Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _queue.Writer.WriteAsync(workItem);
        }

        public async Task<IEnumerable<Func<CancellationToken, Task>>> DequeueAsync(
            CancellationToken cancellationToken, int amount = 1)
        {
            if (amount > 10 || amount < 1)
            {
                throw new Exception("Amount of dequeueing items is out of range.");
            }

            var workItems = new List<Func<CancellationToken, Task>>();

            for (int i = 0; i < amount; i++)
            {
                var workItem = await _queue.Reader.ReadAsync(cancellationToken);

                workItems.Add(workItem);

                if (_queue.Reader.Count == 0)
                {
                    break;
                }
            }

            return workItems;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Services
{
    public interface IBackgroundTaskQueue<T>
    {
        Task QueueBackgroundWorkItemAsync(T item);

        Task<IEnumerable<T>> DequeueAsync(
            CancellationToken cancellationToken, int amount = 1);
    }
}

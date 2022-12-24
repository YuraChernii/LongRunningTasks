using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Services
{
    public interface IBackgroundTaskQueue
    {
        Task QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);

        Task<IEnumerable<Func<CancellationToken, Task>>> DequeueAsync(
            CancellationToken cancellationToken, int amount = 1);
    }
}

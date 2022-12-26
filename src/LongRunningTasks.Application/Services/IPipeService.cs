using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Services
{
    public interface IPipeService
    {
        void Submit(Func<CancellationToken, Task> func);

    }
}

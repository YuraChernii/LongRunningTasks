using LongRunningTasks.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Commands.TestINotification
{
    internal class TestINotification_1Handler : INotificationHandler<TestINotification>
    {
        public async Task Handle(TestINotification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(5000);

            Console.WriteLine("TestINotification_1Handler");
        }
    }
}

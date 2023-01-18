using MediatR;

namespace LongRunningTasks.Application.Commands.TestINotification
{
    internal class TestINotification_2Handler : INotificationHandler<TestINotification>
    {
        public async Task Handle(TestINotification notification, CancellationToken cancellationToken)
        {
            await Task.Delay(5000);

            Console.WriteLine("TestINotification_2Handler");
        }
    }
}

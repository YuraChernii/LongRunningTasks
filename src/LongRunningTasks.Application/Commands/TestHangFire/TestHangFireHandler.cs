using Hangfire;
using LongRunningTasks.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Commands.TestHangFire
{
    internal class TestHangFireHandler : IRequestHandler<TestHangFire, TestHangFireDTO>
    {
        public async Task<TestHangFireDTO> Handle(TestHangFire request, CancellationToken cancellationToken)
        {
            //Fire - and - Forget Job - this job is executed only once
            var fireJobId = BackgroundJob.Enqueue(() => Console.WriteLine("Execute now."));


            //Delayed Job - this job executed only once but not immedietly after some time.
            var delayedJobId = BackgroundJob.Schedule(() => Console.WriteLine("Shedule after 20 seconds."), TimeSpan.FromSeconds(20));


            //Continuations Job - this job executed when its parent job is executed.
            BackgroundJob.ContinueJobWith(fireJobId, () => Console.WriteLine($"Execute after jod: {fireJobId}."));


            //Recurring Job - this job is executed many times on the specified cron schedule
            RecurringJob.AddOrUpdate(() => Console.WriteLine("Minute sending!"), Cron.Minutely);


            return new TestHangFireDTO();
        }
    }   
}

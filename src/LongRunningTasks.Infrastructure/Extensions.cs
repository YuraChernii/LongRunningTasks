
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Config;
using LongRunningTasks.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LongRunningTasks.Infrastructure
{
    public static class Extensions
    {
        public static void AddInfrastructure(this WebApplicationBuilder builder)
        {

            builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailConfig"));

            builder.Services.AddHostedService<TimedHostedService>();

            builder.Services.AddHostedService<QueuedHostedService>();

            builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
            {
                if (!int.TryParse(builder.Configuration["Queue:Capacity"], out var queueCapacity))
                    queueCapacity = 100;
                return new BackgroundTaskQueue(queueCapacity);
            });

        }
    }
}

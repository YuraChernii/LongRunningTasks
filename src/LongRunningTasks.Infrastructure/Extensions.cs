
using AutoMapper;
using Hangfire;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Core.Repositories;
using LongRunningTasks.Infrastructure.Config;
using LongRunningTasks.Infrastructure.Databases.RabbitDB;
using LongRunningTasks.Infrastructure.Databases.RabbitDB.Repositories;
using LongRunningTasks.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LongRunningTasks.Infrastructure
{
    public static class Extensions
    {
        public static void AddInfrastructure(this WebApplicationBuilder builder)
        {
            builder.AddConfigMapping();

            builder.AddBackgroundServices();

            builder.Services.AddDbContext<RabbitDBContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("RabbitDB")));

            builder.AddRepositories();
        }

        public static void AddConfigMapping(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailConfig"));
            builder.Services.Configure<PipeConfig>(builder.Configuration.GetSection("PipeConfig"));
        }

        public static void AddBackgroundServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddHostedService<TimedHostedService>();

            builder.Services.AddHostedService<QueuedHostedService>();

            builder.Services.AddHostedService<RabbitMQConsumerService>();

            builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
            {
                if (!int.TryParse(builder.Configuration["Queue:Capacity"], out var queueCapacity))
                    queueCapacity = 100;
                return new BackgroundTaskQueue(queueCapacity);
            });

            builder.Services.AddSingleton<IPipeService, PipeService>();

            builder.AddHangFireServices();
        }

        public static void AddHangFireServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangFireDB")));
            builder.Services.AddHangfireServer();
        }

        public static void AddRepositories(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IItemRepository, ItemRepository>();
        }

        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseHangfireDashboard();

            Databases.RabbitDB.Tables.Extensions.Configure(app.ApplicationServices.GetService<IMapper>());

            Services.Mapping.Extensions.Configure(app.ApplicationServices.GetService<IMapper>());

        }
    }
}

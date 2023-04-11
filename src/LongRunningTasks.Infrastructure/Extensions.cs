
using AutoMapper;
using Hangfire;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Application.Utilities;
using LongRunningTasks.Application.Utilities.RabbitMQ.Connections;
using LongRunningTasks.Application.Utilities.RabbitMQ.Subscribers;
using LongRunningTasks.Core.Repositories;
using LongRunningTasks.Infrastructure.Config;
using LongRunningTasks.Infrastructure.Databases.RabbitDB;
using LongRunningTasks.Infrastructure.Databases.RabbitDB.Repositories;
using LongRunningTasks.Infrastructure.Services;
using LongRunningTasks.Infrastructure.Utilities;
using LongRunningTasks.Infrastructure.Utilities.RabbitMQ.Connections;
using LongRunningTasks.Infrastructure.Utilities.RabbitMQ.Publishers;
using LongRunningTasks.Infrastructure.Utilities.RabbitMQ.Subscribers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace LongRunningTasks.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            //services.AddConfigMapping(config);

            //services.AddBackgroundServices(config);

            //services.AddDbContext<RabbitDBContext>(options =>
            //    options.UseSqlServer(config.GetConnectionString("RabbitDB")));

            //services.AddRepositories(config);

            //services.AddRabbitMQMessaging(config);

            return services;
        }

        public static IServiceCollection AddConfigMapping(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailConfig>(config.GetSection("EmailConfig"));
            services.Configure<PipeConfig> (config.GetSection("PipeConfig"));

            return services;
        }

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddHostedService<UkrNetParsingBackgroundService>();

            services.AddHostedService<TimedHostedService>();

            services.AddHostedService<QueuedHostedService>();

            services.AddSingleton<IBackgroundTaskQueue>(ctx =>
            {
                if (!int.TryParse(config["Queue:Capacity"], out var queueCapacity))
                    queueCapacity = 100;
                return new BackgroundTaskQueue(queueCapacity);
            });

            services.AddSingleton<IPipeService, PipeService>();

            services.AddHangFireServices(config);

            return services;
        }

        public static IServiceCollection AddHangFireServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddHangfire(x => x.UseSqlServerStorage(config.GetConnectionString("HangFireDB")));
            services.AddHangfireServer();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IItemRepository, ItemRepository>();

            return services;
        }

        public static IServiceCollection AddRabbitMQMessaging(this IServiceCollection services, IConfiguration config)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "user",
                Password = "user",
                VirtualHost = "/"
            };

            var connection = factory.CreateConnection();

            services.AddSingleton(connection);
            services.AddSingleton<ChannelAccessor>();
            services.AddSingleton<IChannelFactory, ChannelFactory>();
            services.AddSingleton<IMessagePublisher, MessagePublisher>();
            services.AddSingleton<IMessageSubscriber, MessageSubscriber>();

            services.AddHostedService<MessagingBackgroundService>();

            return services;
        }

        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseHangfireDashboard();

            Databases.RabbitDB.Tables.Extensions.Configure(app.ApplicationServices.GetService<IMapper>());

            Utilities.Mapping.Extensions.Configure(app.ApplicationServices.GetService<IMapper>());

        }
    }
}


using AutoMapper;
using Hangfire;
using LongRunningTasks.Application.DTOs;
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
            services.AddConfigMapping(config);

            services.AddBackgroundServices(config);

            services.AddDbContext<RabbitDBContext>(options =>
                options.UseSqlServer(config.GetConnectionString("RabbitDB")));

            services.AddRepositories(config);

            return services;
        }

        public static IServiceCollection AddConfigMapping(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailConfig>(config.GetSection("EmailConfig"));
            services.Configure<PipeConfig>(config.GetSection("PipeConfig"));

            return services;
        }

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration config)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            services.AddSingleton<IBackgroundTaskQueue<SendEmailInQueueDTO>>(ctx =>
            {
                if (!int.TryParse(config["Queue:Capacity"], out var queueCapacity))
                    queueCapacity = 100;
                return new BackgroundTaskQueue<SendEmailInQueueDTO>(queueCapacity);
            });

            services.AddHostedService<UkrNetParsingBackgroundService>();
            services.AddHostedService<QueuedEmailsBackgroundService>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IItemRepository, ItemRepository>();

            return services;
        }

        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            Databases.RabbitDB.Tables.Extensions.Configure(app.ApplicationServices.GetService<IMapper>());

            Utilities.Mapping.Extensions.Configure(app.ApplicationServices.GetService<IMapper>());

        }
    }
}

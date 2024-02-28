using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Configs;
using LongRunningTasks.Infrastructure.Services;
using LongRunningTasks.Infrastructure.Services.Background;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace LongRunningTasks.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddConfigMapping(config);
            services.AddServices(config);

            return services;
        }

        private static IServiceCollection AddConfigMapping(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<GoogleDriveConfig>(config.GetSection("GoogleDrive"));
            services.Configure<TelegramConfig>(config.GetSection("Telegram"));
            services.Configure<UkrnetConfig>(config.GetSection("Ukrnet"));

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config) => services
            .AddTransient<IRetryService, RetryService>()
            .AddSingleton<IExceptionTelegramService, ExceptionTelegramService>()
            .AddChannelService<UkrnetMailDTO>(config)
            .AddChannelService<TelegramMessageDTO>(config)
            .AddSingleton<ITelegramBotClient>(new TelegramBotClient(config["Telegram:BotToken"]!))
            .AddHostedServices();

        private static IServiceCollection AddChannelService<T>(this IServiceCollection services, IConfiguration config) => services
            .AddSingleton<IChannelService<T>>(ctx =>
            {
                if (!int.TryParse(config["Queue:Capacity"], out int queueCapacity))
                {
                    queueCapacity = 100;
                }

                return new ChannelService<T>(queueCapacity);
            });

        private static IServiceCollection AddHostedServices(this IServiceCollection services) => services
            .AddHostedService<UrknetMailParserBackgroundService>()
            .AddHostedService<UrknetMailProcessorBackgroundService>()
            .AddHostedService<TelegramChatMessageSenderBackgroundService>();
    }
}
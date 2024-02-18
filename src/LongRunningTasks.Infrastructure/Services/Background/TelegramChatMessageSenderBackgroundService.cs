using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Core.Enums;
using LongRunningTasks.Infrastructure.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal class TelegramChatMessageSenderBackgroundService : BaseBackgroundService<UrknetMailParserBackgroundService>
    {
        private readonly IChannelService<PrintMailDTO> _channelService;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly TelegramConfig _telegramConfig;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TelegramChatMessageSenderBackgroundService(
            ILogger<UrknetMailParserBackgroundService> logger,
            IChannelService<PrintMailDTO> channelService,
            ITelegramBotClient telegramBotClient,
            IOptions<TelegramConfig> telegramConfig,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger)
        {
            _channelService = channelService;
            _telegramBotClient = telegramBotClient;
            _telegramConfig = telegramConfig.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected async override Task TryExecuteAsync(CancellationToken cancellationToken)
        {
            await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
            IRetryService retryService = scope.ServiceProvider.GetRequiredService<IRetryService>();

            PrintMailDTO mail = await _channelService.DequeueAsync(cancellationToken);

            await retryService.RetryAsync(async () => 
                await _telegramBotClient.SendTextMessageAsync(
                    GetChatId(mail.MessageType),
                    mail.Message,
                    cancellationToken: cancellationToken
                )
            );
        }

        private string GetChatId(MailMessageType messageType) => messageType switch
        {
            MailMessageType.Sformovana => _telegramConfig.ChatIds.Sformovana,
            MailMessageType.Opracovana => _telegramConfig.ChatIds.Opracovana,
            MailMessageType.Vnesenyzmin => _telegramConfig.ChatIds.Vnesenyzmin,
            MailMessageType.VnesenyzminPaid => _telegramConfig.ChatIds.VnesenyzminPaid,
            MailMessageType.OpracovanaVnesenyzmin => _telegramConfig.ChatIds.OpracovanaVnesenyzmin,
            MailMessageType.OpracovanaVnesenyzminPaid => _telegramConfig.ChatIds.OpracovanaVnesenyzminPaid,
            MailMessageType.Undefined => _telegramConfig.ChatIds.Errors,
            _ => throw new ArgumentOutOfRangeException(nameof(messageType))
        };
    }
}
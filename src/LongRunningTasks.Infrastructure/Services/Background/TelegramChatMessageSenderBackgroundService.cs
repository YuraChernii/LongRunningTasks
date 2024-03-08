using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Core.Enums;
using LongRunningTasks.Core.Models;
using LongRunningTasks.Infrastructure.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal class TelegramChatMessageSenderBackgroundService : BaseBackgroundService<TelegramChatMessageSenderBackgroundService>
    {
        private readonly IChannelService<TelegramMessageDTO> _telegramMessageChannel;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly TelegramConfig _telegramConfig;
        private readonly IRetryService _retryService;
        private readonly LinkedList<ProcessedMessage> _processedMessages = new();

        public TelegramChatMessageSenderBackgroundService(
            ILogger<TelegramChatMessageSenderBackgroundService> logger,
            IExceptionTelegramService exceptionTelegramService,
            IChannelService<TelegramMessageDTO> telegramMessageChannel,
            ITelegramBotClient telegramBotClient,
            IOptions<TelegramConfig> telegramConfig,
            IRetryService retryService)
            : base(logger, exceptionTelegramService)
        {
            _telegramMessageChannel = telegramMessageChannel;
            _telegramBotClient = telegramBotClient;
            _telegramConfig = telegramConfig.Value;
            _retryService = retryService;
        }

        protected async override Task TryExecuteAsync(CancellationToken cancellationToken)
        {
            TelegramMessageDTO mail = await _telegramMessageChannel.DequeueAsync(cancellationToken);

            if (!EnsureUniqueMessage(mail))
            {
                return;
            }

            await _retryService.RetryAsync(async () =>
                await _telegramBotClient.SendTextMessageAsync(
                    GetChatId(mail.MessageType),
                    mail.Message,
                    cancellationToken: cancellationToken
                ),
                int.MaxValue,
                catchAsync: async (Exception ex) =>
                {
                    await _exceptionTelegramService.QueueExceptionNotification(ex);
                }
            );
        }

        private bool EnsureUniqueMessage(TelegramMessageDTO message)
        {
            if (_processedMessages.Any(x => x.Id == message.Id && x.MailAction == message.MailAction))
            {
                return false;
            }

            _processedMessages.AddLast(new ProcessedMessage()
            {
                Id = message.Id,
                MailAction = message.MailAction
            });
            if (_processedMessages.Count > 20)
            {
                _processedMessages.RemoveFirst();
            }

            return true;
        }

        private string GetChatId(MailMessageType messageType) => messageType switch
        {
            MailMessageType.Sformovana => _telegramConfig.ChatIds.Sformovana,
            MailMessageType.Opracovana => _telegramConfig.ChatIds.Opracovana,
            MailMessageType.Vnesenyzmin => _telegramConfig.ChatIds.Vnesenyzmin,
            MailMessageType.VnesenyzminPaid => _telegramConfig.ChatIds.VnesenyzminPaid,
            MailMessageType.OpracovanaVnesenyzmin => _telegramConfig.ChatIds.OpracovanaVnesenyzmin,
            MailMessageType.OpracovanaVnesenyzminPaid => _telegramConfig.ChatIds.OpracovanaVnesenyzminPaid,
            MailMessageType.Undefined or MailMessageType.Error => _telegramConfig.ChatIds.Errors,
            _ => throw new ArgumentOutOfRangeException(nameof(messageType))
        };
    }
}
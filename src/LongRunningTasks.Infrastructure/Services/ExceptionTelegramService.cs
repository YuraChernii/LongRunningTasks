using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.ExtensionsN;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Core.Enums;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class ExceptionTelegramService : IExceptionTelegramService
    {
        private readonly IChannelService<TelegramMessageDTO> _telegramMessageChannel;

        public ExceptionTelegramService(IChannelService<TelegramMessageDTO> telegramMessageChannel)
        {
            _telegramMessageChannel = telegramMessageChannel;
        }

        public async Task QueueExceptionNotification(Exception exception)
        {
            if (!IsFrequentlyThrowed(exception))
            {
                await _telegramMessageChannel.QueueAsync(new()
                {
                    MessageType = MailMessageType.Error,
                    Message = exception.GetFullMessage()
                });
            }
        }

        private bool IsFrequentlyThrowed(Exception exception) =>
            exception.Message.StartsWith("The IMAP server has unexpectedly disconnected.") 
            || exception.Message.StartsWith("timeout");
    }
}
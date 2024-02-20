using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.ExtensionsN;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Core.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal abstract class BaseBackgroundService<T> : BackgroundService
    {
        protected readonly ILogger<T> _logger;
        private readonly IChannelService<TelegramMessageDTO> _telegramMessageChannel;

        public BaseBackgroundService(
            ILogger<T> logger,
            IChannelService<TelegramMessageDTO> telegramMessageChannel)
        {
            _logger = logger;
            _telegramMessageChannel = telegramMessageChannel;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await TryExecuteAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Empty);
                    await _telegramMessageChannel.QueueAsync(new()
                    {
                        MessageType = MailMessageType.Error,
                        Message = ex.GetFullMessage()
                    });
                }
            }
        }

        protected abstract Task TryExecuteAsync(CancellationToken cancellationToken);
    }
}
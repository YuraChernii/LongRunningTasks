using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Constants;
using LongRunningTasks.Infrastructure.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Logging;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal class UrknetMailParserBackgroundService : BaseBackgroundService<UrknetMailParserBackgroundService>
    {
        private readonly IChannelService<UkrnetMailDTO> _urknetMailChannel;
        private readonly ImapClient _client;

        public UrknetMailParserBackgroundService(
            ILogger<UrknetMailParserBackgroundService> logger,
            IChannelService<UkrnetMailDTO> urknetMailChannel,
            IChannelService<TelegramMessageDTO> telegramMessageChannel)
            : base(logger, telegramMessageChannel)
        {
            _urknetMailChannel = urknetMailChannel;
            _client = UkrNetUtility.CreateClient();
        }

        protected async override Task TryExecuteAsync(CancellationToken cancellationToken)
        {
            await _client.SignInAsync();
            IMailFolder trashFolder = await _client.GetFolder(_client.PersonalNamespaces[0])
                                                .GetSubfolderAsync(MailFolders.Trash, cancellationToken);
            await trashFolder.OpenAsync(FolderAccess.ReadOnly);

            trashFolder.CountChanged += async (sender, e) =>
            {
                await _urknetMailChannel.QueueAsync(new());
            };

            await _client.IdleAsync(cancellationToken);
        }

        protected async override Task CatchAsync()
        {
            await _client.SignOutAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.SignOutAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}
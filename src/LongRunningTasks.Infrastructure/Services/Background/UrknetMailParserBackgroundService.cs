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

        public UrknetMailParserBackgroundService(
            ILogger<UrknetMailParserBackgroundService> logger,
            IChannelService<UkrnetMailDTO> urknetMailChannel,
            IChannelService<TelegramMessageDTO> telegramMessageChannel)
            : base(logger, telegramMessageChannel)
        {
            _urknetMailChannel = urknetMailChannel;
        }

        protected async override Task TryExecuteAsync(CancellationToken cancellationToken)
        {
            ImapClient client = UkrNetUtility.CreateClient();
            await client.SignInAsync();
            IMailFolder trashFolder = await client.GetFolder(client.PersonalNamespaces[0])
                                                .GetSubfolderAsync(MailFolders.Trash, cancellationToken);
            await trashFolder.OpenAsync(FolderAccess.ReadOnly);

            trashFolder.CountChanged += async (sender, e) =>
            {
                await _urknetMailChannel.QueueAsync(new());
            };

            await client.IdleAsync(cancellationToken);
        }
    }
}
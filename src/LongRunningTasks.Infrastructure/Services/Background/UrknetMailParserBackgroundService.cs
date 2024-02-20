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
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
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
            (ImapClient Client, IMailFolder Folder) openRusult1 = await OpenTrashFolder(cancellationToken);
            (ImapClient Client, IMailFolder Folder) openRusult2 = await OpenTrashFolder(cancellationToken);

            openRusult1.Folder.CountChanged += async (sender, e) =>
            {
                await Lock(async () =>
                {
                    List<UniqueId> uIds = await GetAllMailUniqueIds(openRusult2.Folder);
                    await _urknetMailChannel.QueueAsync(new UkrnetMailDTO()
                    {
                        UIds = uIds
                    });
                });
            };

            await openRusult1.Client.IdleAsync(cancellationToken);
        }

        private async Task Lock(Func<Task> action)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                await action();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task<(ImapClient, IMailFolder)> OpenTrashFolder(CancellationToken cancellationToken)
        {
            ImapClient client = UkrNetUtility.CreateClient();
            await client.SignInAsync();
            IMailFolder trashFolder = await client.GetFolder(client.PersonalNamespaces[0])
                                                .GetSubfolderAsync(MailFolders.Trash, cancellationToken);
            await trashFolder.OpenAsync(FolderAccess.ReadOnly);

            return (client, trashFolder);
        }

        private async Task<List<UniqueId>> GetAllMailUniqueIds(IMailFolder folder)
        {
            IList<IMessageSummary> summaries = await folder.FetchAsync(0, -1, MessageSummaryItems.UniqueId);

            return summaries.Select(_ => _.UniqueId).ToList();
        }
    }
}
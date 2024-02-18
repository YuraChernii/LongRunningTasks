using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Constants;
using LongRunningTasks.Infrastructure.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal class UrknetMailParserBackgroundService : BackgroundService
    {
        private readonly ImapClient _client;
        private readonly ILogger<UrknetMailParserBackgroundService> _logger;
        private readonly IChannelService<ProcessMailDTO> _processMailChannel;

        public UrknetMailParserBackgroundService(
            ILogger<UrknetMailParserBackgroundService> logger,
            IChannelService<ProcessMailDTO> processMailChannel)
        {
            _logger = logger;
            _processMailChannel = processMailChannel;
            _client = UkrNetUtility.CreateClient();
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEmailsAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, string.Empty);
                }
            }
        }

        private async Task ProcessEmailsAsync(CancellationToken cancellationToken)
        {
            await _client.SignInAsync();
            IMailFolder trashFolder = await _client.GetFolder(_client.PersonalNamespaces[0])
                                                .GetSubfolderAsync(MailFolders.Trash, cancellationToken);
            await trashFolder.OpenAsync(FolderAccess.ReadOnly);
            trashFolder.CountChanged += (sender, e) =>
            {
                ImapFolder? folder = (ImapFolder?)sender;
                UniqueId? uId = folder?.UidNext;
                if (uId.HasValue)
                {
                    _processMailChannel.QueueAsync(new ProcessMailDTO()
                    {
                        UId = uId.Value
                    });
                }
            };
            await _client.IdleAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.SignOutAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}



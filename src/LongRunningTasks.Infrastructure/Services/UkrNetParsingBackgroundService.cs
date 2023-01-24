using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class UkrNetParsingBackgroundService : BackgroundService
    {
        private readonly ImapClient _client;
        private readonly ILogger<UkrNetParsingBackgroundService> _logger;
        private readonly IBackgroundTaskQueue<SendEmailInQueueDTO> _queue;


        public UkrNetParsingBackgroundService(
            ILogger<UkrNetParsingBackgroundService> logger,
            IBackgroundTaskQueue<SendEmailInQueueDTO> queue
            )
        {
            _logger = logger;
            _queue = queue;
            _client = UkrNetUtility.CreateClient();
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await ProcessEmailsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
            finally
            {
                await _client.SignOutAsync();

                await ExecuteAsync(stoppingToken);
            }
        }

        private async Task ProcessEmailsAsync(CancellationToken stoppingToken)
        {
            await _client.SignInAsync();

            var trashFolder = await _client.GetFolder(_client.PersonalNamespaces[0])
                                    .GetSubfolderAsync("Trash", stoppingToken);

            await trashFolder.OpenAsync(FolderAccess.ReadOnly);

            trashFolder.CountChanged += (sender, e) =>
            {
                var folder = (ImapFolder?)sender;
                _queue.QueueBackgroundWorkItemAsync(new SendEmailInQueueDTO()
                {
                    UId = folder.UidNext.Value
                });
            };

            await _client.IdleAsync(stoppingToken);
        }



        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.SignOutAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}



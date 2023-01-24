using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace LongRunningTasks.Infrastructure.Services
{
    // BackgroundService implements IHostedService
    internal class QueuedEmailsBackgroundService : BackgroundService
    {
        private readonly ILogger<QueuedEmailsBackgroundService> _logger;
        private readonly ImapClient _client;
        private static LinkedList<UniqueId> _uniqueIds = new LinkedList<UniqueId>();

        public QueuedEmailsBackgroundService(IBackgroundTaskQueue<SendEmailInQueueDTO> taskQueue,
            ILogger<QueuedEmailsBackgroundService> logger)
        {
            TaskQueue = taskQueue;
            _logger = logger;
            _client = UkrNetUtility.CreateClient();
        }

        public IBackgroundTaskQueue<SendEmailInQueueDTO> TaskQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _client.SignInAsync();

                await ProcessQueue(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred executing....");
            }
            finally
            {
                await _client.SignOutAsync();

                await ExecuteAsync(stoppingToken);
            }

        }

        private async Task ProcessQueue(CancellationToken stoppingToken)
        {
            var trashFolder = await _client.GetFolder(_client.PersonalNamespaces[0])
                                    .GetSubfolderAsync("Trash", stoppingToken);

            await trashFolder.OpenAsync(FolderAccess.ReadOnly);

            while (!stoppingToken.IsCancellationRequested)
            {
                var item =
                    await TaskQueue.DequeueAsync(stoppingToken);

                var emailUId = item.FirstOrDefault()?.UId;

                if (!emailUId.HasValue)
                {
                    _logger.LogError(new Exception("No Uid!!!"), "Incorrect logic.");
                }

                await ProcessEmail((UniqueId)emailUId, trashFolder);
            }
        }

        private async Task ProcessEmail(UniqueId uId, IMailFolder folder)
        {

            var summaries = await folder.FetchAsync(0, -1, MessageSummaryItems.UniqueId);

            var allCurrentUniqueIds = summaries.Select(_ => _.UniqueId).ToList();


            var deletedEmails = new List<UniqueId>();
            var startingIndex = 0;
            foreach (var uniqueId in _uniqueIds)
            {
                var tempIndex = allCurrentUniqueIds.FindLastIndex(id => id == uniqueId);
                if (tempIndex == -1)
                {
                    deletedEmails.Add(uniqueId);
                }
                else
                {
                    startingIndex = tempIndex;
                }
            }

            var newEmails = new LinkedList<UniqueId>();
            for (int i = startingIndex; i < allCurrentUniqueIds.Count; i++)
            {
                var exists = _uniqueIds.Contains(allCurrentUniqueIds[i]);

                if (!exists)
                {
                    _uniqueIds.AddLast(allCurrentUniqueIds[i]);
                    newEmails.AddLast(allCurrentUniqueIds[i]);
                    if (_uniqueIds.Count > 200)
                    {
                        _uniqueIds.RemoveFirst();
                    }
                    if (newEmails.Count > 50)
                    {
                        newEmails.RemoveFirst();
                    }
                }
            }

            var bot = new TelegramBotClient("5813736223:AAGuIRqDOSgVYMD_CJ62hJLjNrgmpZcpdMY");
            foreach (var email in newEmails)
            {
                var message = await folder.GetMessageAsync(email);
                var text = message.TextBody.ToString();
                var address = message.From.Mailboxes.FirstOrDefault()?.Address ?? "";
                if (message.From.Mailboxes.Any(_ => _.Address.Contains("e-noreply@land.gov.ua")) && !text.Contains("опрацьована"))
                {
                    var s = await bot.SendTextMessageAsync("@derefeefef", ".");

                }
            }





            await bot.CloseAsync();
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}

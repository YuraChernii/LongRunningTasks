﻿using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Telegram.Bot;
using TL;

namespace LongRunningTasks.Infrastructure.Services
{
    // BackgroundService implements IHostedService
    internal class QueuedEmailsBackgroundService : BackgroundService
    {
        private readonly ILogger<QueuedEmailsBackgroundService> _logger;
        private readonly ImapClient _client;
        private LinkedList<Item> _uniqueIds = new LinkedList<Item>();
        private bool previousProcessingCompleted = true;
        string fileName = "WeatherForecast.json";

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
            await _client.SignInAsync();

            var trashFolder = await _client.GetFolder(_client.PersonalNamespaces[0])
                                    .GetSubfolderAsync("Trash", stoppingToken);

            await trashFolder.OpenAsync(FolderAccess.ReadOnly);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (previousProcessingCompleted)
                {
                    await TaskQueue.DequeueAsync(stoppingToken);
                }

                previousProcessingCompleted = false;

                await ProcessEmail(trashFolder);
            }
        }

        private async Task ProcessEmail(IMailFolder folder)
        {
            string databaseText = System.IO.File.ReadAllText(fileName);
            if (!string.IsNullOrEmpty(databaseText))
            {
                _uniqueIds = JsonSerializer.Deserialize<LinkedList<Item>>(databaseText) ?? _uniqueIds;
            }

            var summaries = await folder.FetchAsync(0, -1, MessageSummaryItems.UniqueId);

            var allCurrentUniqueIds = summaries.Select(_ => _.UniqueId).ToList();

            // Get list of deleted emails.
            // Find last index of newest common (between cached and server data) message.
            var deletedEmails = new List<Item>();
            var startingIndex = 0;
            bool stopCount = false;
            foreach (var uniqueId in _uniqueIds)
            {
                var tempIndex = allCurrentUniqueIds.FindLastIndex(id => id.Id == uniqueId.Id);
                if (tempIndex == -1)
                {
                    deletedEmails.Add(uniqueId);
                }
                else
                {
                    if (!stopCount)
                        startingIndex = tempIndex;

                    if (!uniqueId.Processed)
                        stopCount = true;
                }
            }

            // Get all newly arrived emails.
            for (int i = startingIndex < 0 ? 0 : startingIndex; i < allCurrentUniqueIds.Count; i++)
            {
                var exists = _uniqueIds.Contains(new Item() { Id = allCurrentUniqueIds[i].Id, Processed = true });

                if (!exists)
                {
                    if (!_uniqueIds.Contains(new Item() { Id = allCurrentUniqueIds[i].Id, Processed = false }))
                        _uniqueIds.AddLast(new Item() { Id = allCurrentUniqueIds[i].Id, Processed = false });

                    if (_uniqueIds.Count > 200)
                    {
                        _uniqueIds.RemoveFirst();
                    }
                }
            }

            var bot = new TelegramBotClient("5813736223:AAGuIRqDOSgVYMD_CJ62hJLjNrgmpZcpdMY");

            foreach (var emailId in _uniqueIds.Where(x => x.Processed == false).Select(x => x.Id))
            {
                var message = await folder.GetMessageAsync(new UniqueId(emailId));
                var text = message.TextBody?.ToString();
                var address = message.From.Mailboxes.FirstOrDefault()?.Address ?? "";

                var elem = _uniqueIds.Find(new Item() { Id = emailId, Processed = false })?.Value;

                if (message.From.Mailboxes.Any(_ => _.Address.Contains("e-noreply@land.gov.ua")) &&
                                                    text != null &&
                                                    text.ToLower().Contains("щодо державної реєстрації земельної ділянки сформована")
                                              )
                {
                    int index = text.IndexOf("\\r\\n") - 1;
                    var textToSend = text;
                    if (index >= 0)
                    {
                        textToSend = text.Substring(0, index);
                        textToSend = textToSend.Replace("Вітаємо, шановний(а) ", "");
                    }
                    _logger.LogInformation("textToSend: " + textToSend);
                    var s = await bot.SendTextMessageAsync("@derefeefef", textToSend);

                    SetMessageText(new UniqueId(emailId), textToSend);
                }

                if (elem != null)
                    elem.Processed = true;
            }

            foreach (var item in deletedEmails)
            {
                if (item.Text != null)
                {

                    var text = "Було видалено: " + item.Text;
                    await bot.SendTextMessageAsync("@derefeefef", text);

                }

                _uniqueIds.Remove(item);
            }

            previousProcessingCompleted = true;

            System.IO.File.WriteAllText(fileName, "");
            using FileStream createStream = File.OpenWrite(fileName);
            await JsonSerializer.SerializeAsync(createStream, _uniqueIds);
            await createStream.DisposeAsync();
        }

        private void SetMessageText(UniqueId id, string text)
        {
            var item = _uniqueIds.FirstOrDefault(x => x.Id == id.Id);
            if (item != null)
            {
                item.Text = text;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }

    class Item : IEquatable<Item>
    {
        public uint Id { get; set; }
        public bool Processed { get; set; }

        public string Text { get; set; }

        public bool Equals(Item? item)
        {
            return this.Processed == item?.Processed && this.Id == item?.Id;
        }
    }
}

using Google.Apis.Drive.v3;
using LongRunningTasks.Application.DTOs;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Core.Enums;
using LongRunningTasks.Core.Models;
using LongRunningTasks.Infrastructure.Configs;
using LongRunningTasks.Infrastructure.Constants;
using LongRunningTasks.Infrastructure.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.Json;
using File = Google.Apis.Drive.v3.Data.File;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal class UrknetMailProcessorBackgroundService : BaseBackgroundService<UrknetMailProcessorBackgroundService>
    {
        private bool previousProcessingCompleted = true;
        private readonly GoogleDriveConfig _googleDriveConfig;
        private readonly UkrnetConfig _ukrnetConfig;
        private readonly IChannelService<UkrnetMailDTO> _processMailChannel;
        private readonly IRetryService _retryService;
        private readonly IChannelService<TelegramMessageDTO> _telegramMessageChannel;
        private readonly ImapClient _client;

        public UrknetMailProcessorBackgroundService(
            IOptions<GoogleDriveConfig> googleDriveConfig,
            IOptions<UkrnetConfig> ukrnetConfig,
            IChannelService<UkrnetMailDTO> processMailChannel,
            ILogger<UrknetMailProcessorBackgroundService> logger,
            IChannelService<TelegramMessageDTO> telegramMessageChannel,
            IRetryService retryService,
            IExceptionTelegramService exceptionTelegramService)
            : base(logger, exceptionTelegramService)
        {
            _googleDriveConfig = googleDriveConfig.Value;
            _ukrnetConfig = ukrnetConfig.Value;
            _processMailChannel = processMailChannel;
            _client = UkrNetUtility.CreateClient();
            _retryService = retryService;
            _telegramMessageChannel = telegramMessageChannel;
        }

        protected override async Task TryExecuteAsync(CancellationToken cancellationToken)
        {
            IMailFolder trashFolder = await OpenTrashFolder(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (previousProcessingCompleted)
                {
                    await _processMailChannel.DequeueAsync(cancellationToken);
                }
                previousProcessingCompleted = false;
                await ProcessMailsInFolder(trashFolder);
            }
        }

        private async Task<IMailFolder> OpenTrashFolder(CancellationToken cancellationToken)
        {
            await _client.SignInAsync();
            IMailFolder trashFolder = await _client.GetFolder(_client.PersonalNamespaces[0])
                                                .GetSubfolderAsync(MailFolders.Trash, cancellationToken);
            await trashFolder.OpenAsync(FolderAccess.ReadOnly);

            return trashFolder;
        }

        private async Task ProcessMailsInFolder(IMailFolder folder)
        {
            List<UniqueId> allMailUniqueIds = await GetAllMailUniqueIds(folder);

            DriveService driveService = DriveServiceFactory.GetService(_googleDriveConfig);
            File? file = await driveService.FindFileByNameAsync(_googleDriveConfig.FileName);
            LinkedList<MailModel> savedMails = await GetSavedMailsAsync(file, driveService);

            try
            {
                IEnumerable<MailModel> deletedEmails = GetDeletedEmails(allMailUniqueIds, savedMails, out int indexToStartProcessFrom);

                for (int i = indexToStartProcessFrom; i < allMailUniqueIds.Count; i++)
                {
                    if (!savedMails.Any(x => x.Id == allMailUniqueIds[i].Id))
                    {
                        savedMails.AddLast(new MailModel() { Id = allMailUniqueIds[i].Id });

                        if (savedMails.Count > 200)
                        {
                            savedMails.RemoveFirst();
                        }
                    }
                }

                await _retryService.RetryAsync(
                    async () =>
                    {
                        await SendUnProcessedMailsToPrint(folder, savedMails, deletedEmails);
                        await SendDeletedMailsToPrint(deletedEmails, savedMails);
                    },
                    int.MaxValue,
                    catchAsync: async (Exception ex) =>
                    {
                        await _client.SignOutAsync();
                        folder = await OpenTrashFolder(CancellationToken.None);
                        await _exceptionTelegramService.QueueExceptionNotification(ex);
                    }
                );
                previousProcessingCompleted = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                await SaveMailsToGoogleDrive(savedMails, file, driveService);
            }
        }

        private async Task<List<UniqueId>> GetAllMailUniqueIds(IMailFolder folder)
        {
            IList<IMessageSummary> summaries = await folder.FetchAsync(0, -1, MessageSummaryItems.UniqueId);

            return summaries.Select(_ => _.UniqueId).ToList();
        }

        private TelegramMessageDTO ProcessMail(MailModel mailToProcess, string text, List<string> prefixsToRemove, string cutOffMarker, MailMessageType messageType)
        {
            int index = text.IndexOf(cutOffMarker) - (cutOffMarker == "\\r\\n" ? 1 : 0);
            string textToPrint = index >= 0 ? text.Substring(0, index) : text;
            prefixsToRemove.ForEach(prefix => textToPrint = textToPrint.Replace(prefix, string.Empty));
            mailToProcess.Message = textToPrint;
            mailToProcess.MessageType = messageType;

            return new()
            {
                Id = mailToProcess.Id,
                Message = textToPrint,
                MessageType = messageType
            };
        }

        private async Task<LinkedList<MailModel>> GetSavedMailsAsync(File? file, DriveService driveService)
        {
            if (file == null)
            {
                return new();
            }

            Stream? stream = await driveService.GetFileStreamAsync(file);
            stream!.Position = 0;
            StreamReader reader = new(stream);
            string? allFileText = reader.ReadToEnd();

            return JsonSerializer.Deserialize<LinkedList<MailModel>>(allFileText) ?? new();
        }

        private IEnumerable<MailModel> GetDeletedEmails(
            List<UniqueId> allMailUniqueIds, IEnumerable<MailModel> savedMails, out int indexToStartProcessFrom)
        {
            indexToStartProcessFrom = 0;
            bool stopCount = false;
            List<MailModel> deletedMails = new();
            foreach (MailModel savedMail in savedMails)
            {
                int tempIndex = allMailUniqueIds.FindIndex(id => id.Id == savedMail.Id);
                if (tempIndex == -1)
                {
                    deletedMails.Add(savedMail);
                    savedMail.Processed = true;
                }
                else
                {
                    if (!stopCount)
                    {
                        indexToStartProcessFrom = tempIndex;
                    }

                    if (!savedMail.Processed)
                    {
                        stopCount = true;
                    }
                }
            }

            return deletedMails;
        }

        private async Task SendUnProcessedMailsToPrint(IMailFolder folder, IEnumerable<MailModel> savedMails, IEnumerable<MailModel> deletedEmails)
        {
            foreach (MailModel mailToProcess in savedMails.Where(x => !x.Processed))
            {
                MimeMessage message = await folder.GetMessageAsync(new UniqueId(mailToProcess.Id));
                if (message == null)
                {
                    deletedEmails.Append(mailToProcess);
                    mailToProcess.Processed = true;
                    continue;
                }

                string? text = message!.TextBody?.ToString();
                string address = message.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty;

                TelegramMessageDTO? printMailDTO = null;
                if (!message.From.Mailboxes.Any(_ => _.Address.Contains(_ukrnetConfig.SentFrom)) || text == null)
                {
                    mailToProcess.Processed = true;
                    continue;
                }

                if (text.Contains("щодо державної реєстрації земельної ділянки сформована")
                    || text.Contains("щодо державної реєстрації земельної ділянки створена"))
                {
                    printMailDTO = ProcessMail(
                        mailToProcess,
                        text,
                        new() { "Вітаємо, шановний(а) " },
                        "\\r\\n",
                        MailMessageType.Sformovana
                    );
                }
                else if (text.Contains("щодо державної реєстрації земельної ділянки опрацьовано"))
                {
                    printMailDTO = ProcessMail(
                        mailToProcess,
                        text,
                        new() { "Шановний(а) " },
                        ", заяву ",
                        MailMessageType.Opracovana
                    );
                }
                else if (text.ToLower().Contains("про внесення відомостей (змін до них) до державного земельного кадастру за кадастровим номером"))
                {
                    printMailDTO = ProcessMail(
                        mailToProcess,
                        text,
                        new() { "Вітаємо, шановний(а) " },
                        "\\r\\n",
                        MailMessageType.Vnesenyzmin
                    );
                }
                else if (text.ToLower().Contains("про внесення виправлених відомостей до державного земельного кадастру"))
                {
                    printMailDTO = ProcessMail(
                        mailToProcess,
                        text,
                        new() { "Вітаємо, шановний(а) " },
                        "\\r\\n",
                        MailMessageType.VnesenyzminPaid
                    );
                }
                else if (text.Contains(", Заява/Повідомлення") && text.Contains("опрацьована."))
                {
                    if (text.Contains("За результатами опрацювання"))
                    {
                        printMailDTO = ProcessMail(
                            mailToProcess,
                            text,
                            new() { "Шановний(а) ", "Шановний" },
                            ", Заява/Повідомлення",
                            MailMessageType.OpracovanaVnesenyzminPaid
                        );
                    }
                    else
                    {
                        printMailDTO = ProcessMail(
                            mailToProcess,
                            text,
                            new() { "Шановний(а) ", "Шановний" },
                            ", Заява/Повідомлення",
                            MailMessageType.OpracovanaVnesenyzmin
                        );
                    }
                }

                if (printMailDTO != null)
                {
                    await _telegramMessageChannel.QueueAsync(printMailDTO);
                }

                mailToProcess.Processed = true;
            }
        }

        private async Task SendDeletedMailsToPrint(IEnumerable<MailModel> deletedEmails, LinkedList<MailModel> savedMails)
        {
            foreach (MailModel deletedMail in deletedEmails)
            {
                string message = deletedMail.Message != null
                    ? "Було видалено: " + deletedMail.Message.TrimStart() + "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"
                    : $"Було видалено email з id: {deletedMail.Id}";
                await _telegramMessageChannel.QueueAsync(new()
                {
                    Message = message,
                    MessageType = deletedMail.MessageType
                });
                savedMails.Remove(deletedMail);
            }
        }

        private async Task SaveMailsToGoogleDrive(IEnumerable<MailModel> mails, File? file, DriveService driveService)
        {
            if (!mails.Any())
            {
                return;
            }

            using Stream memoryStream = new MemoryStream();
            JsonSerializer.Serialize(memoryStream, mails);
            if (file == null)
            {
                await driveService.CreateFileAsync(memoryStream, _googleDriveConfig.FileName, _googleDriveConfig.FileMime);
            }
            else
            {
                await driveService.UpdateFileAsync(file, memoryStream, _googleDriveConfig.FileName, _googleDriveConfig.FileMime);
            }
        }

        protected async override Task CatchAsync()
        {
            await _client.SignOutAsync();
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _client.SignOutAsync();

            await base.StopAsync(stoppingToken);
        }
    }
}
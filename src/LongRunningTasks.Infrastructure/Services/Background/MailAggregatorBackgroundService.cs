using LongRunningTasks.Application.Services;
using LongRunningTasks.Core.Enums;
using LongRunningTasks.Core.Models;
using LongRunningTasks.Infrastructure.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using File = Google.Apis.Drive.v3.Data.File;

namespace LongRunningTasks.Infrastructure.Services.Background
{
    internal class MailAggregatorBackgroundService : BaseBackgroundService<MailAggregatorBackgroundService>
    {
        private readonly GoogleDriveConfig _googleDriveConfig;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MailAggregatorBackgroundService(
            ILogger<MailAggregatorBackgroundService> logger,
            IExceptionTelegramService exceptionTelegramService,
            IOptions<GoogleDriveConfig> googleDriveConfig,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, exceptionTelegramService)
        {
            _googleDriveConfig = googleDriveConfig.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected async override Task TryExecuteAsync(CancellationToken cancellationToken)
        {
            using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
            IGoogleDriveService googleDriveService = scope.ServiceProvider.GetRequiredService<IGoogleDriveService>();
            IGoogleSheetsService googleSheetsService = scope.ServiceProvider.GetRequiredService<IGoogleSheetsService>();

            File? baseFile = await googleDriveService.FindFileByNameAsync(_googleDriveConfig.FileNames.Base);
            List<MailModel> savedMails = await googleDriveService.GetDataAsync<List<MailModel>>(baseFile);

            File? groupedFile = await googleDriveService.FindFileByNameAsync(_googleDriveConfig.FileNames.Grouped);
            List<MailGroup> savedGroups = await googleDriveService.GetDataAsync<List<MailGroup>>(groupedFile);

            UpdateGroupsCollection(savedGroups, savedMails);

            List<IList<object>> excelData = FormExcelData(savedGroups);
            googleSheetsService.UpdateSheet(excelData);

            CleanUpGroupsCollection(savedGroups);

            await SaveGroupsToGoogleDrive(savedGroups, groupedFile, googleDriveService);

            await Task.Delay(60 * 60 * 1000);
        }

        private void UpdateGroupsCollection(List<MailGroup> savedGroups, List<MailModel> savedMails)
        {
            DateTime now = DateTime.UtcNow;
            foreach (MailModel savedMail in savedMails.Where(x => x.MessageType == MailMessageType.Sformovana))
            {
                List<MailGroup> groups = savedGroups.Where(x => x.Key == savedMail.Message).ToList();
                bool mailIsInGroup = groups.Where(group => group.SformovanaIds.Contains(savedMail.Id)).Any();
                if (!mailIsInGroup)
                {
                    MailGroup? lastGroup = groups.LastOrDefault();
                    if (lastGroup != null && lastGroup.Created.AddDays(1) < now)
                    {
                        lastGroup.SformovanaIds.Add(savedMail.Id);
                    }
                    else
                    {
                        savedGroups.Add(new()
                        {
                            Key = savedMail.Message,
                            SformovanaIds = new() { savedMail.Id },
                            Created = now
                        });
                    }
                }
            }

            foreach (MailModel savedMail in savedMails.Where(x => x.MessageType == MailMessageType.Opracovana))
            {
                MailGroup? group = savedGroups.LastOrDefault(x => x.Key == savedMail.Message);
                if (group != null)
                {
                    if (!group.OpracovanaIds.Contains(savedMail.Id))
                    {
                        group.OpracovanaIds.Add(savedMail.Id);
                    }
                }
            }
        }

        private List<IList<object>> FormExcelData(List<MailGroup> savedGroups)
        {
            List<IList<object>> excelData = new List<IList<object>>
            {
                new List<object>()
                {
                    "ФІП",
                    "Кількість сформованих",
                    "Кількість опрацьованих",
                    "Створений"
                }
            };
            foreach (MailGroup savedGroup in savedGroups)
            {
                excelData.Add(new List<object>()
                {
                    savedGroup.Key,
                    savedGroup.SformovanaIds.Count().ToString(),
                    savedGroup.OpracovanaIds.Count().ToString(),
                    savedGroup.Created.ToString("dd-MM-yyyy HH:mm:ss")
                });
            }
            excelData.AddRange(Enumerable.Repeat(
                new List<object>()
                {
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty
                },
                20));

            return excelData;
        }

        private void CleanUpGroupsCollection(List<MailGroup> savedGroups)
        {
            int elem = savedGroups.Count - _googleDriveConfig.Capacity;
            foreach (MailGroup savedGroup in savedGroups.Take(Math.Max(elem, 0)))
            {
                if (savedGroup.OpracovanaIds.Count() >= savedGroup.SformovanaIds.Count()
                    || savedGroup.Created.AddMonths(3) < DateTime.UtcNow)
                {
                    savedGroups.Remove(savedGroup);
                }
            }
        }

        private async Task SaveGroupsToGoogleDrive(IEnumerable<MailGroup> groups, File? file, IGoogleDriveService driveService)
        {
            if (!groups.Any())
            {
                return;
            }

            using Stream memoryStream = new MemoryStream();
            JsonSerializer.Serialize(memoryStream, groups);
            if (file == null)
            {
                await driveService.CreateFileAsync(memoryStream, _googleDriveConfig.FileNames.Grouped, _googleDriveConfig.FileMime);
            }
            else
            {
                await driveService.UpdateFileAsync(file, memoryStream, _googleDriveConfig.FileNames.Grouped, _googleDriveConfig.FileMime);
            }
        }

        public class MailGroup
        {
            public string Key { get; set; }
            public List<uint> SformovanaIds { get; set; } = new();
            public List<uint> OpracovanaIds { get; set; } = new();
            public DateTime Created { get; set; }
        }
    }
}
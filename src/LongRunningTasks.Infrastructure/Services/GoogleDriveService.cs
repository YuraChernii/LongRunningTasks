using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Util.Store;
using LongRunningTasks.Application.Services;
using LongRunningTasks.Infrastructure.Configs;
using Microsoft.Extensions.Options;
using System.Text.Json;
using static Google.Apis.Drive.v3.DriveService;
using File = Google.Apis.Drive.v3.Data.File;

namespace LongRunningTasks.Infrastructure.Services
{
    internal class GoogleDriveService: IGoogleDriveService
    {
        private readonly DriveService _driveService;

        public GoogleDriveService(
            IOptions<GoogleDriveConfig> googleDriveConfig,
            IOptions<GoogleApplicationConfig> googleApplicationConfig)
        {
            _driveService = GetService(googleDriveConfig.Value, googleApplicationConfig.Value);
        }

        public async Task<File?> FindFileByNameAsync(string name)
        {
            FilesResource.ListRequest request = _driveService.Files.List();
            FileList fileMetaData = await request.ExecuteAsync();

            return fileMetaData.Files.FirstOrDefault(x => x.Name == name);
        }

        public async Task<string> UpdateFileAsync(
            File fileMetaData, Stream stream, string fileName, string contentType)
        {
            File updatedFile = new()
            {
                Name = fileName
            };
            FilesResource.UpdateMediaUpload updateMediaUpload = _driveService.Files
                .Update(updatedFile, fileMetaData.Id, stream, contentType);
            await updateMediaUpload.UploadAsync();

            return updateMediaUpload.ResponseBody.Id;
        }

        public async Task<string> CreateFileAsync(Stream stream, string fileName, string contentType)
        {
            File createdFile = new()
            {
                Name = fileName
            };
            FilesResource.CreateMediaUpload createRequest = _driveService.Files
                .Create(createdFile, stream, contentType);
            await createRequest.UploadAsync();

            return createRequest.ResponseBody.Id;
        }

        public async Task<T> GetDataAsync<T>(File? file) where T : new()
        {
            if (file == null)
            {
                return new();
            }

            Stream? stream = await GetFileStreamAsync(file);
            stream!.Position = 0;
            StreamReader reader = new(stream);
            string? allFileText = reader.ReadToEnd();

            return JsonSerializer.Deserialize<T>(allFileText) ?? new();
        }

        private async Task<Stream?> GetFileStreamAsync(File file)
        {
            FilesResource.GetRequest request = _driveService.Files.Get(file.Id);
            MemoryStream fileStream = new();
            await request.DownloadAsync(fileStream);

            return fileStream;
        }

        private DriveService GetService(GoogleDriveConfig googleDriveConfig, GoogleApplicationConfig googleApplicationConfig)
        {
            TokenResponse tokenResponse = new()
            {
                AccessToken = googleDriveConfig.AccessToken,
                RefreshToken = googleDriveConfig.RefreshToken
            };
            GoogleAuthorizationCodeFlow apiCodeFlow = new(new()
            {
                ClientSecrets = new()
                {
                    ClientId = googleApplicationConfig.ClientId,
                    ClientSecret = googleApplicationConfig.ClientSecret
                },
                Scopes = new[] { Scope.Drive },
                DataStore = new FileDataStore(googleApplicationConfig.Name)
            });
            UserCredential credential = new(apiCodeFlow, googleDriveConfig.Username, tokenResponse);
            DriveService service = new(new()
            {
                HttpClientInitializer = credential,
                ApplicationName = googleApplicationConfig.Name
            });

            return service;
        }
    }
}
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Util.Store;
using LongRunningTasks.Infrastructure.Configs;
using static Google.Apis.Drive.v3.DriveService;
using File = Google.Apis.Drive.v3.Data.File;

namespace LongRunningTasks.Infrastructure.Services
{
    internal static class DriveServiceFactory
    {
        public static DriveService GetService(GoogleDriveConfig config)
        {
            TokenResponse tokenResponse = new()
            {
                AccessToken = config.AccessToken,
                RefreshToken = config.RefreshToken
            };
            GoogleAuthorizationCodeFlow apiCodeFlow = new(new()
            {
                ClientSecrets = new()
                {
                    ClientId = config.ClientId,
                    ClientSecret = config.ClientSecret
                },
                Scopes = new[] { Scope.Drive },
                DataStore = new FileDataStore(config.ApplicationName)
            });
            UserCredential credential = new(apiCodeFlow, config.Username, tokenResponse);
            DriveService service = new(new()
            {
                HttpClientInitializer = credential,
                ApplicationName = config.ApplicationName
            });

            return service;
        }

        public static async Task<File?> FindFileByNameAsync(this DriveService service, string name)
        {
            FilesResource.ListRequest request = service.Files.List();
            FileList fileMetaData = await request.ExecuteAsync();

            return fileMetaData.Files.FirstOrDefault(x => x.Name.Contains(name));
        }

        public static async Task<Stream?> GetFileStreamAsync(this DriveService service, File file)
        {
            FilesResource.GetRequest request = service.Files.Get(file.Id);
            MemoryStream fileStream = new();
            await request.DownloadAsync(fileStream);

            return fileStream;
        }

        public static async Task<string> UpdateFileAsync(
            this DriveService service, File fileMetaData, Stream stream, string fileName, string contentType)
        {
            File updatedFile = new()
            {
                Name = fileName
            };
            FilesResource.UpdateMediaUpload updateMediaUpload = service.Files
                .Update(updatedFile, fileMetaData.Id, stream, contentType);
            await updateMediaUpload.UploadAsync();

            return updateMediaUpload.ResponseBody.Id;
        }

        public static async Task<string> CreateFileAsync(
            this DriveService service, Stream stream, string fileName, string contentType)
        {
            File createdFile = new()
            {
                Name = fileName
            };
            FilesResource.CreateMediaUpload createRequest = service.Files
                .Create(createdFile, stream, contentType);
            await createRequest.UploadAsync();

            return createRequest.ResponseBody.Id;
        }
    }
}
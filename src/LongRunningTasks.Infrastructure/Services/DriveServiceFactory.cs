using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using static Google.Apis.Drive.v3.DriveService;

namespace LongRunningTasks.Infrastructure.Services
{
    public static class DriveServiceFactory
    {
        public static DriveService GetService()
        {
            TokenResponse tokenResponse = new()
            {
                AccessToken = "ya29.a0AfB_byBHl7b4_UlxyYr7Xgkz5oMs6r31OQYDictTma6k-3oCxzffAa3QpOe5tquyhgriPz37Lpi1GJnGHy1zqwIA-9H2YlxVa0pqsuvHEX43u8s_BVHksRY8827XvHMGbogs6OPnhFb5PwA_LX-_4QIdb0r9b27KlYd2aCgYKAVkSARESFQHGX2MisDml3uydJpLuJlOWCij3qA0171",
                RefreshToken = "1//09RAIJVYyKsW-CgYIARAAGAkSNwF-L9Iru2WGEwxJ5bu4losn73QClCx2Qd3HeZDz0R6crhl8I8cFAcITAuNyAhq5KVRYu_j_qR4",
            };
            string applicationName = "EmailProcessor";
            string username = "mikeke373737@gmail.com";
            GoogleAuthorizationCodeFlow apiCodeFlow = new(new()
            {
                ClientSecrets = new()
                {
                    ClientId = "949383295896-gpu3auboojetdg31616t3oect3505qo0.apps.googleusercontent.com",
                    ClientSecret = "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"
                },
                Scopes = new[] { Scope.Drive },
                DataStore = new FileDataStore(applicationName)
            });
            UserCredential credential = new(apiCodeFlow, username, tokenResponse);
            DriveService service = new(new()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });

            return service;
        }

        public static async Task<Google.Apis.Drive.v3.Data.File?> FindFileByNameAsync(this DriveService service, string name)
        {
            var request = service.Files.List();
            var fileMetaData = await request.ExecuteAsync();

            return fileMetaData.Files.FirstOrDefault(x => x.Name.Contains(name));
        }

        public static async Task<Stream?> GetFileStreamAsync(this DriveService service, Google.Apis.Drive.v3.Data.File fileMetaData)
        {
            var request = service.Files.Get(fileMetaData.Id);

            var fileStream = new MemoryStream();
            await request.DownloadAsync(fileStream);

            return fileStream;
        }

        public static async Task<string> UpdateFileAsync(this DriveService service, Google.Apis.Drive.v3.Data.File fileMetaData, Stream stream, string fileName, string fileMime)
        {
            var updatedFileMetadata = new Google.Apis.Drive.v3.Data.File();
            updatedFileMetadata.Name = fileName;

            var updateRequest = service.Files.Update(updatedFileMetadata, fileMetaData.Id, stream, fileMime);
            await updateRequest.UploadAsync();
            var fileResult = updateRequest.ResponseBody;

            return fileResult.Id;
        }

        public static async Task<string> CreateFileAsync(this DriveService service, Stream stream, string fileName, string fileMime)
        {
            var createdFileMetadata = new Google.Apis.Drive.v3.Data.File();
            createdFileMetadata.Name = fileName;

            var createRequest = service.Files.Create(createdFileMetadata, stream, fileMime);
            await createRequest.UploadAsync();
            var file = createRequest.ResponseBody;

            return file.Id;
        }
    }
}

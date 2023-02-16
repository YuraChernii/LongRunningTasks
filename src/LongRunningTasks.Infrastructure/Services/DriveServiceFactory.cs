using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using static Google.Apis.Drive.v3.DriveService;

namespace LongRunningTasks.Infrastructure.Services
{
    public static class DriveServiceFactory
    {
        public static DriveService GetService()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "ya29.a0AVvZVsr7jxMzTMlbayJTUHzFVa99359t-qTbqXGrSOREtFD-ddfxxZzuBLUIbNXN768uhSs7HotUUOGWcqspenD0F0cJw2O3vkBw-1h_XKcSqCsXnRYIRtrXFmoQmNzrL2ttlGttSqhakgfuqdUgaLtDdR-_aCgYKAWkSARESFQGbdwaIUbMKhfq0huB-tA30CEH9Pg0163",
                RefreshToken = "1//04C6-Y9aw7E-HCgYIARAAGAQSNwF-L9Ir7AfDzvZeDzFLq1RGlXzXWJNaFHxawi2oXm0aw7QOtc9dMZsv43XSCJfIckUYtw8S8EI",
            };

            var applicationName = "EDC Portal";
            var username = "mikeke373737@gmail.com"; // Use your email

            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "710753423788-957qlqlr2eqr8u40l6t9fq1cp1sc0cf4.apps.googleusercontent.com",
                    ClientSecret = "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"
                },
                Scopes = new[] { Scope.Drive },
                DataStore = new FileDataStore(applicationName)
            });

            var credential = new UserCredential(apiCodeFlow, username, tokenResponse);

            var service = new DriveService(new BaseClientService.Initializer
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

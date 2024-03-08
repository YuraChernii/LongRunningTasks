using File = Google.Apis.Drive.v3.Data.File;

namespace LongRunningTasks.Application.Services
{
    public interface IGoogleDriveService
    {
        Task<File?> FindFileByNameAsync(string name);
        Task<string> UpdateFileAsync(File fileMetaData, Stream stream, string fileName, string contentType);
        Task<string> CreateFileAsync(Stream stream, string fileName, string contentType);
        Task<T> GetDataAsync<T>(File? file) where T : new();
    }
}
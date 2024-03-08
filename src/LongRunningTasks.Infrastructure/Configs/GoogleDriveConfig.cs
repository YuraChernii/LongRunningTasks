namespace LongRunningTasks.Infrastructure.Configs
{
    internal class GoogleDriveConfig
    {
        public FileNames FileNames { get; set; }
        public string FileMime { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Username { get; set; }
        public int Capacity { get; set; }
    }

    class FileNames
    {
        public string Base { get; set; }
        public string Grouped { get; set; }
    }
}
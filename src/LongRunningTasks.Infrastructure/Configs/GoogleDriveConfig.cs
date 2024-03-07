namespace LongRunningTasks.Infrastructure.Configs
{
    internal class GoogleDriveConfig
    {
        public string FileName { get; set; }
        public string FileMime { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ApplicationName { get; set; }
        public string Username { get; set; }
        public int Capacity { get; set; }
    }
}
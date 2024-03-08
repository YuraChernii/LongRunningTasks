namespace LongRunningTasks.Infrastructure.Configs
{
    internal class GoogleSheetsConfig
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Username { get; set; }
        public string SpreadsheetId { get; set; }
        public string Sheet { get; set; }
    }
}
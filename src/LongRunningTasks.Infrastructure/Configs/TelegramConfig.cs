namespace LongRunningTasks.Infrastructure.Configs
{
    internal class TelegramConfig
    {
        public string BotToken { get; set; }
        public ChatIds ChatIds { get; set; }
        
    }

    internal class ChatIds
    {
        public string Sformovana { get; set; }
        public string Opracovana { get; set; }
        public string Vnesenyzmin { get; set; }
        public string VnesenyzminPaid { get; set; }
        public string OpracovanaVnesenyzmin { get; set; }
        public string OpracovanaVnesenyzminPaid { get; set; }
        public string Errors { get; set; }
    }
}
using LongRunningTasks.Core.Enums;

namespace LongRunningTasks.Application.DTOs
{
    public class TelegramMessageDTO
    {
        public string Message { get; set; }
        public MailMessageType MessageType { get; set; }
    }
}
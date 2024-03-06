using LongRunningTasks.Core.Enums;

namespace LongRunningTasks.Application.DTOs
{
    public class TelegramMessageDTO
    {
        public uint? Id { get; set; }
        public string Message { get; set; }
        public MailMessageType MessageType { get; set; }
    }
}
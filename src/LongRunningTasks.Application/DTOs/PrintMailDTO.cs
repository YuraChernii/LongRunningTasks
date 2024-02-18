using LongRunningTasks.Core.Enums;

namespace LongRunningTasks.Application.DTOs
{
    public class PrintMailDTO
    {
        public string Message { get; set; }
        public MailMessageType MessageType { get; set; }
    }
}
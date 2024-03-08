using LongRunningTasks.Core.Enums;

namespace LongRunningTasks.Core.Models
{
    public class ProcessedMessage
    {
        public uint Id { get; set; }
        public MailActionType MailAction { get; set; }
    }
}
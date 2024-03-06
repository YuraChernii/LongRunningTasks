using LongRunningTasks.Core.Enums;

namespace LongRunningTasks.Core.Models
{
    public class MailModel : IEquatable<MailModel>
    {
        public uint Id { get; set; }
        public string Message { get; set; }
        public MailMessageType MessageType { get; set; }

        public bool Equals(MailModel? item)
        {
            return Id == item?.Id;
        }
    }
}
using MailKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.DTOs
{
    public class SendEmailInQueueDTO
    {
       public UniqueId UId { get; set; }
    }
}

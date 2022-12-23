using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Config
{
    public class EmailConfig
    {
        public string ApiKey { get; set; }
        public string FromEmail { get; set; }
        public string FromUserName { get; set; }
    }
}

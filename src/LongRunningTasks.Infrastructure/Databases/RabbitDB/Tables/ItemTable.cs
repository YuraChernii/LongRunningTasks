using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Databases.RabbitDB.Tables
{
    public class ItemTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

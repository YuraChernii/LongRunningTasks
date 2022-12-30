using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Core.Entities
{
    public class Item: IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

using AutoMapper;
using LongRunningTasks.Core.Entities;
using LongRunningTasks.Infrastructure.Databases.RabbitDB.Tables;

namespace LongRunningTasks.Infrastructure.Services.Mapping
{
    internal class ItemProfile: Profile
    {
        public ItemProfile()
        {
            CreateMap<Item, ItemTable>();
            CreateMap<ItemTable, Item>();
        }
    }
}

using LongRunningTasks.Core.Entities;
using LongRunningTasks.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace LongRunningTasks.Infrastructure.Databases.RabbitDB.Repositories
{
    internal class ItemRepository : GenericRepository<Item>, IItemRepository
    {
        public ItemRepository(RabbitDBContext context, ILogger<ItemRepository> logger) : base(context, logger)
        {
        }
    }
}

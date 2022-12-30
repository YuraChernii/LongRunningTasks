using LongRunningTasks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LongRunningTasks.Infrastructure.Databases.RabbitDB
{
    public class RabbitDBContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public RabbitDBContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //options.UseSqlServer(Configuration.GetConnectionString("RabbitDB"));
        }

        public DbSet<Item> Items { get; set; }
    }
}

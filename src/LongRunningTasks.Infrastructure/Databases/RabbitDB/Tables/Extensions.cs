using AutoMapper;
using LongRunningTasks.Core.Entities;

namespace LongRunningTasks.Infrastructure.Databases.RabbitDB.Tables
{
    public static class Extensions
    {
        private static IMapper _mapper;

        public static void Configure(IMapper mapper)
        {
            _mapper = mapper;
        }

        public static T AsTable<T>(this IEntity model) where T : ITable
        {
            return _mapper.Map<T>(model);
        }

        public static T AsModel<T>(this ITable table) where T : IEntity
        {
            return _mapper.Map<T>(table);
        }
    }
}

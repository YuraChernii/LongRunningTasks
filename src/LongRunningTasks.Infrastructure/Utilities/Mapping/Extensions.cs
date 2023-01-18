using AutoMapper;
using LongRunningTasks.Application.DTOs;

namespace LongRunningTasks.Infrastructure.Utilities.Mapping
{
    public static class Extensions
    {
        private static IMapper _mapper;
        public static void Configure(IMapper mapper)
        {
            _mapper = mapper;
        }

        public static T MapTo<T>(this IDTO dto)
        {
            return _mapper.Map<T>(dto);
        }
    }
}

using AutoMapper;
using LongRunningTasks.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Infrastructure.Services.Mapping
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

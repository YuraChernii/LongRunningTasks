using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LongRunningTasks.Application
{
    public static class Extensions
    {
        public static void AddApplication(this IServiceCollection services, IConfiguration config)
        {
            //services.AddMediatR(Assembly.GetExecutingAssembly());
        }
    }
}

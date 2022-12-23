using MediatR;
using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace LongRunningTasks.Application
{
    public static class Extensions
    {
        public static void AddApplication(this WebApplicationBuilder builder)
        {
            builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
        }
    }
}

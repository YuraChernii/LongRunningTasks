using LongRunningTasks.Application.DTOs;
using MediatR;

namespace LongRunningTasks.Application.Commands.EnqueueTask
{
    public class EnqueueTask: IRequest<EnqueueTaskDTO>
    {

    }
}

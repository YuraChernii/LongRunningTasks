using LongRunningTasks.Application.DTOs;
using MediatR;

namespace LongRunningTasks.Application.Commands.TestBackgroundService
{
    public class TestBackgroundService: IRequest<TestPipeDTO>
    {

    }
}

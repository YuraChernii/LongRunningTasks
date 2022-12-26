using LongRunningTasks.Application.DTOs;
using MediatR;

namespace LongRunningTasks.Application.Commands.TestHangFire
{
    public class TestHangFire: IRequest<TestHangFireDTO>
    {
    }
}

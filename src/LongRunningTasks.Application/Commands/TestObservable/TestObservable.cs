using LongRunningTasks.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Application.Commands.TestObservable
{
    public class TestObservable: IRequest<TestObservableQueueDTO>
    {
    }
}

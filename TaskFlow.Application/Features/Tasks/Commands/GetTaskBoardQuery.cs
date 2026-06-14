using MediatR;
using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.Features.Tasks.Commands
{
    public class GetTaskBoardQuery : IRequest<List<TaskBoardColumnDto>>
    {
    }
}

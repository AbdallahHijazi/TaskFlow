using MediatR;
using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.Features.Users.Commands;

public class GetUserTasksQuery : IRequest<List<TaskDto>>
{
    public Guid UserId { get; set; }

    public GetUserTasksQuery(Guid userId)
    {
        UserId = userId;
    }
}

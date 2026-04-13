using MediatR;
using TaskFlow.Application.DTOs.User;

namespace TaskFlow.Application.Features.Users.Commands;

public class GetUserTasksQuery : IRequest<UserTasksPagedResultDto>
{
    public Guid UserId { get; set; }
    public UserTasksQueryParametersDto Parameters { get; set; }

    public GetUserTasksQuery(Guid userId, UserTasksQueryParametersDto parameters)
    {
        UserId = userId;
        Parameters = parameters;
    }
}

using MediatR;
using TaskFlow.Application.DTOs.User;

namespace TaskFlow.Application.Features.Users.Commands;

public class GetUserProfileWithTasksQuery : IRequest<UserProfileWithTasksDto>
{
    public Guid UserId { get; set; }

    public GetUserProfileWithTasksQuery(Guid userId)
    {
        UserId = userId;
    }
}

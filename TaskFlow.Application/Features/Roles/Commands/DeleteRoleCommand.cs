using MediatR;

namespace TaskFlow.Application.Features.Roles.Commands;

public class DeleteRoleCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteRoleCommand(Guid id)
    {
        Id = id;
    }
}

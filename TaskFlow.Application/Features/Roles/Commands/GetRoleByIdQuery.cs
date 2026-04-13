using MediatR;
using TaskFlow.Application.DTOs.Role;

namespace TaskFlow.Application.Features.Roles.Commands;

public class GetRoleByIdQuery : IRequest<RoleDto>
{
    public Guid Id { get; set; }

    public GetRoleByIdQuery(Guid id)
    {
        Id = id;
    }
}

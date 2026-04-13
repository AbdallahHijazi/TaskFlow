using MediatR;
using TaskFlow.Application.DTOs.Role;

namespace TaskFlow.Application.Features.Roles.Commands;

public class UpdateRoleCommand : IRequest<RoleDto>
{
    public Guid Id { get; set; }
    public UpdateRoleDto Dto { get; set; }

    public UpdateRoleCommand(Guid id, UpdateRoleDto dto)
    {
        Id = id;
        Dto = dto;
    }
}

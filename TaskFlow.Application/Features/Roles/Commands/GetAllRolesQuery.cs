using MediatR;
using TaskFlow.Application.DTOs.Role;

namespace TaskFlow.Application.Features.Roles.Commands;

public class GetAllRolesQuery : IRequest<List<RoleDto>>
{
}

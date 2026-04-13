using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Role;
using TaskFlow.Application.Features.Roles.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Roles.Handlers;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto>
{
    private readonly IRepository<Role> _repository;

    public GetRoleByIdQueryHandler(IRepository<Role> repository)
    {
        _repository = repository;
    }

    public async Task<RoleDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _repository.GetAll().FirstOrDefaultAsync(r => r.RoleId == request.Id, cancellationToken);
        if (role == null)
            throw new NotFoundException("الدور", request.Id);

        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName ?? string.Empty
        };
    }
}

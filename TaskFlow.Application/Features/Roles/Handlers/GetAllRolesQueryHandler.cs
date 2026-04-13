using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Role;
using TaskFlow.Application.Features.Roles.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Roles.Handlers;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IRepository<Role> _repository;

    public GetAllRolesQueryHandler(IRepository<Role> repository)
    {
        _repository = repository;
    }

    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        return await _repository
            .GetAll()
            .Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName ?? string.Empty
            })
            .ToListAsync(cancellationToken);
    }
}

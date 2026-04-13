using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Role;
using TaskFlow.Application.Features.Roles.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Roles.Handlers;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private readonly IRepository<Role> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IRepository<Role> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _repository.GetAll().FirstOrDefaultAsync(r => r.RoleId == request.Id, cancellationToken);
        if (role == null)
            throw new NotFoundException("الدور", request.Id);

        if (string.IsNullOrWhiteSpace(request.Dto.RoleName))
            throw new BadRequestException("اسم الدور مطلوب");

        role.RoleName = request.Dto.RoleName.Trim();
        _repository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName
        };
    }
}

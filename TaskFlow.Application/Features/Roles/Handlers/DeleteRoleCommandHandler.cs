using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Roles.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Roles.Handlers;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IRepository<Role> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(IRepository<Role> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _repository.GetAll().FirstOrDefaultAsync(r => r.RoleId == request.Id, cancellationToken);
        if (role == null)
            throw new NotFoundException("الدور", request.Id);

        _repository.Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.TaskDependencies.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.TaskDependencies.Handlers;

public class DeleteTaskDependencyCommandHandler : IRequestHandler<DeleteTaskDependencyCommand, bool>
{
    private readonly IRepository<TaskDependency> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskDependencyCommandHandler(IRepository<TaskDependency> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetAll().FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("تبعية المهمة", request.Id);

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

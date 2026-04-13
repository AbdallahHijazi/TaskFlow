using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.DependencyTypes.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.DependencyTypes.Handlers;

public class DeleteDependencyTypeCommandHandler : IRequestHandler<DeleteDependencyTypeCommand, bool>
{
    private readonly IRepository<DependencyType> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDependencyTypeCommandHandler(IRepository<DependencyType> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteDependencyTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetAll().FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("نوع التبعية", request.Id);

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

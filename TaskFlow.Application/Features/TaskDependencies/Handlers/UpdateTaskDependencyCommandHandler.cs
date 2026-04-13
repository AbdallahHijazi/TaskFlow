using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.TaskDependency;
using TaskFlow.Application.Features.TaskDependencies.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.TaskDependencies.Handlers;

public class UpdateTaskDependencyCommandHandler : IRequestHandler<UpdateTaskDependencyCommand, TaskDependencyDto>
{
    private readonly IRepository<TaskDependency> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskDependencyCommandHandler(IRepository<TaskDependency> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDependencyDto> Handle(UpdateTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetAll().FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("تبعية المهمة", request.Id);

        if (request.Dto.PredecessorId == null || request.Dto.SuccessorId == null || request.Dto.DependencyTypeId == null)
            throw new BadRequestException("بيانات التبعية غير مكتملة");

        entity.DependencyTypeId = request.Dto.DependencyTypeId;
        entity.PredecessorId = request.Dto.PredecessorId;
        entity.SuccessorId = request.Dto.SuccessorId;

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaskDependencyDto
        {
            Id = entity.Id,
            DependencyTypeId = entity.DependencyTypeId,
            PredecessorId = entity.PredecessorId,
            SuccessorId = entity.SuccessorId
        };
    }
}

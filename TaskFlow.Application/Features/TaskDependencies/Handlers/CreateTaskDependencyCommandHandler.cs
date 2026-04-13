using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.TaskDependency;
using TaskFlow.Application.Features.TaskDependencies.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.TaskDependencies.Handlers;

public class CreateTaskDependencyCommandHandler : IRequestHandler<CreateTaskDependencyCommand, TaskDependencyDto>
{
    private readonly IRepository<TaskDependency> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskDependencyCommandHandler(IRepository<TaskDependency> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDependencyDto> Handle(CreateTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.PredecessorId == null || request.Dto.SuccessorId == null || request.Dto.DependencyTypeId == null)
            throw new BadRequestException("بيانات التبعية غير مكتملة");

        var entity = new TaskDependency
        {
            DependencyTypeId = request.Dto.DependencyTypeId,
            PredecessorId = request.Dto.PredecessorId,
            SuccessorId = request.Dto.SuccessorId
        };

        _repository.Add(entity);
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

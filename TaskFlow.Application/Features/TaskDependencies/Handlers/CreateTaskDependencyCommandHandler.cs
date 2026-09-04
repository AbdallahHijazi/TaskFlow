using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.TaskDependency;
using TaskFlow.Application.Features.TaskDependencies.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.TaskDependencies.Handlers;

public class CreateTaskDependencyCommandHandler : IRequestHandler<CreateTaskDependencyCommand, TaskDependencyDto>
{
    private readonly IRepository<TaskDependency> _repository;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IRepository<DependencyType> _typeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskDependencyCommandHandler(IRepository<TaskDependency> repository, IRepository<TaskItem> taskRepository,
        IRepository<DependencyType> typeRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _taskRepository = taskRepository;
        _typeRepository = typeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDependencyDto> Handle(CreateTaskDependencyCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.PredecessorId == null || request.Dto.SuccessorId == null || request.Dto.DependencyTypeId == null)
            throw new BadRequestException("Dependency type, predecessor, and successor are required.");

        var predecessorId = request.Dto.PredecessorId.Value;
        var successorId = request.Dto.SuccessorId.Value;
        var dependencyTypeId = request.Dto.DependencyTypeId.Value;
        if (predecessorId == successorId) throw new BadRequestException("A task cannot depend on itself.");

        var tasksExist = await _taskRepository.GetAll()
            .CountAsync(task => task.Id == predecessorId || task.Id == successorId, cancellationToken) == 2;
        if (!tasksExist) throw new BadRequestException("Both tasks must exist in the current workspace.");
        if (!await _typeRepository.GetAll().AnyAsync(type => type.Id == dependencyTypeId, cancellationToken))
            throw new BadRequestException("The selected dependency type does not exist.");

        var dependencies = await _repository.GetAll()
            .Select(item => new { item.PredecessorId, item.SuccessorId }).ToListAsync(cancellationToken);
        if (dependencies.Any(item => item.PredecessorId == predecessorId && item.SuccessorId == successorId))
            throw new BadRequestException("This dependency already exists.");

        var queue = new Queue<Guid>();
        var visited = new HashSet<Guid> { successorId };
        queue.Enqueue(successorId);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in dependencies.Where(item => item.PredecessorId == current && item.SuccessorId.HasValue)
                         .Select(item => item.SuccessorId!.Value))
            {
                if (next == predecessorId) throw new BadRequestException("This dependency would create a circular task relationship.");
                if (visited.Add(next)) queue.Enqueue(next);
            }
        }

        var entity = new TaskDependency { DependencyTypeId = dependencyTypeId, PredecessorId = predecessorId, SuccessorId = successorId };
        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new TaskDependencyDto { Id = entity.Id, DependencyTypeId = entity.DependencyTypeId,
            PredecessorId = entity.PredecessorId, SuccessorId = entity.SuccessorId };
    }
}

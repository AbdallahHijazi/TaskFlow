using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.TaskDependency;
using TaskFlow.Application.Features.TaskDependencies.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.TaskDependencies.Handlers;

public class GetTaskDependencyByIdQueryHandler : IRequestHandler<GetTaskDependencyByIdQuery, TaskDependencyDto>
{
    private readonly IRepository<TaskDependency> _repository;

    public GetTaskDependencyByIdQueryHandler(IRepository<TaskDependency> repository)
    {
        _repository = repository;
    }

    public async Task<TaskDependencyDto> Handle(GetTaskDependencyByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("تبعية المهمة", request.Id);

        return new TaskDependencyDto
        {
            Id = entity.Id,
            DependencyTypeId = entity.DependencyTypeId,
            PredecessorId = entity.PredecessorId,
            SuccessorId = entity.SuccessorId
        };
    }
}

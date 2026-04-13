using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.TaskDependency;
using TaskFlow.Application.Features.TaskDependencies.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.TaskDependencies.Handlers;

public class GetAllTaskDependenciesQueryHandler : IRequestHandler<GetAllTaskDependenciesQuery, List<TaskDependencyDto>>
{
    private readonly IRepository<TaskDependency> _repository;

    public GetAllTaskDependenciesQueryHandler(IRepository<TaskDependency> repository)
    {
        _repository = repository;
    }

    public async Task<List<TaskDependencyDto>> Handle(GetAllTaskDependenciesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAll()
            .AsNoTracking()
            .Select(d => new TaskDependencyDto
            {
                Id = d.Id,
                DependencyTypeId = d.DependencyTypeId,
                PredecessorId = d.PredecessorId,
                SuccessorId = d.SuccessorId
            })
            .ToListAsync(cancellationToken);
    }
}

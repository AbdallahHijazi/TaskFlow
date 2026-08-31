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
    private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

    public GetAllTaskDependenciesQueryHandler(
        IRepository<TaskDependency> repository,
        TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<List<TaskDependencyDto>> Handle(GetAllTaskDependenciesQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetAll();
        if (!_currentUser.IsAdmin)
        {
            var userId = _currentUser.UserId;
            query = query.Where(dependency => userId.HasValue &&
                ((dependency.Successor != null && dependency.Successor.AssignedToId == userId.Value)
                 || (dependency.Predecessor != null && dependency.Predecessor.AssignedToId == userId.Value)));
        }

        return await query
            .AsNoTracking()
            .Select(d => new TaskDependencyDto
            {
                Id = d.Id,
                DependencyTypeId = d.DependencyTypeId,
                PredecessorId = d.PredecessorId,
                SuccessorId = d.SuccessorId,
                DependencyTypeName = d.DependencyType == null ? null : d.DependencyType.Name,
                PredecessorName = d.Predecessor == null ? null : d.Predecessor.Name,
                SuccessorName = d.Successor == null ? null : d.Successor.Name
            })
            .ToListAsync(cancellationToken);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Users.Handlers;

public class GetUserTasksQueryHandler : IRequestHandler<GetUserTasksQuery, List<TaskDto>>
{
    private readonly IRepository<User> _usersRepository;
    private readonly IRepository<TaskItem> _tasksRepository;

    public GetUserTasksQueryHandler(IRepository<User> usersRepository, IRepository<TaskItem> tasksRepository)
    {
        _usersRepository = usersRepository;
        _tasksRepository = tasksRepository;
    }

    public async Task<List<TaskDto>> Handle(GetUserTasksQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _usersRepository.GetAll()
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
            throw new NotFoundException("المستخدم", request.UserId);

        var tasks = await _tasksRepository.GetAll()
            .AsNoTracking()
            .Where(t => t.AssignedToId == request.UserId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Name = t.Name ?? string.Empty,
                Description = t.Description,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Progress = t.Progress,
                StatusId = t.StatusId,
                InitiativeId = t.InitiativeId,
                AssignedToId = t.AssignedToId,
                CreatedById = t.CreatedBy ?? Guid.Empty,
                ImageId = t.ImageId,
                UpdatedAt = t.UpdatedAt,
                UpdatedById = t.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return tasks;
    }
}

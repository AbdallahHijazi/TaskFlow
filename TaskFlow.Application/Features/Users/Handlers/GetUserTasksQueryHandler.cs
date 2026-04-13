using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Users.Handlers;

public class GetUserTasksQueryHandler : IRequestHandler<GetUserTasksQuery, UserTasksPagedResultDto>
{
    private readonly IRepository<User> _usersRepository;
    private readonly IRepository<TaskItem> _tasksRepository;

    public GetUserTasksQueryHandler(IRepository<User> usersRepository, IRepository<TaskItem> tasksRepository)
    {
        _usersRepository = usersRepository;
        _tasksRepository = tasksRepository;
    }

    public async Task<UserTasksPagedResultDto> Handle(GetUserTasksQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _usersRepository.GetAll()
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
            throw new NotFoundException("المستخدم", request.UserId);

        var pageNumber = request.Parameters.PageNumber < 1 ? 1 : request.Parameters.PageNumber;
        var pageSize = request.Parameters.PageSize < 1 ? 20 : Math.Min(request.Parameters.PageSize, 100);

        var query = _tasksRepository.GetAll()
            .AsNoTracking()
            .Where(t => t.AssignedToId == request.UserId);

        if (request.Parameters.Status.HasValue)
            query = query.Where(t => t.StatusId == request.Parameters.Status.Value);

        if (request.Parameters.InitiativeId.HasValue)
            query = query.Where(t => t.InitiativeId == request.Parameters.InitiativeId.Value);

        if (request.Parameters.Priority.HasValue)
            query = query.Where(t => t.Priority == request.Parameters.Priority.Value);

        if (request.Parameters.FromDate.HasValue)
            query = query.Where(t => t.StartDate.HasValue && t.StartDate.Value.Date >= request.Parameters.FromDate.Value.Date);

        if (request.Parameters.ToDate.HasValue)
            query = query.Where(t => t.EndDate.HasValue && t.EndDate.Value.Date <= request.Parameters.ToDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(request.Parameters.Search))
        {
            var search = request.Parameters.Search.Trim().ToLower();
            query = query.Where(t =>
                (t.Name != null && t.Name.ToLower().Contains(search)) ||
                (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        var sortBy = request.Parameters.SortBy.Trim().ToLower();
        var sortDirection = request.Parameters.SortDirection.Trim().ToLower();
        var isDescending = sortDirection != "asc";

        query = sortBy switch
        {
            "duedate" => isDescending ? query.OrderByDescending(t => t.EndDate) : query.OrderBy(t => t.EndDate),
            "priority" => isDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            _ => isDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Name = t.Name ?? string.Empty,
                Description = t.Description,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Progress = t.Progress,
                Priority = t.Priority,
                StatusId = t.StatusId,
                InitiativeId = t.InitiativeId,
                AssignedToId = t.AssignedToId,
                CreatedById = t.CreatedBy ?? Guid.Empty,
                ImageId = t.ImageId,
                UpdatedAt = t.UpdatedAt,
                UpdatedById = t.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return new UserTasksPagedResultDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items
        };
    }
}

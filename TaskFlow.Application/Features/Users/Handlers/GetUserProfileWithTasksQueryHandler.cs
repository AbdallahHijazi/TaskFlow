using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Users.Handlers;

public class GetUserProfileWithTasksQueryHandler : IRequestHandler<GetUserProfileWithTasksQuery, UserProfileWithTasksDto>
{
    private readonly IRepository<User> _usersRepository;

    public GetUserProfileWithTasksQueryHandler(IRepository<User> usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<UserProfileWithTasksDto> Handle(GetUserProfileWithTasksQuery request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetAll()
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.AssignedTasks)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("المستخدم", request.UserId);

        var tasks = user.AssignedTasks
            .Select(t => new UserTaskSummaryDto
            {
                Id = t.Id,
                Name = t.Name ?? string.Empty,
                StatusId = t.StatusId,
                InitiativeId = t.InitiativeId,
                EndDate = t.EndDate,
                Progress = t.Progress
            })
            .ToList();

        return new UserProfileWithTasksDto
        {
            Id = user.Id,
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId ?? Guid.Empty,
            RoleName = user.Role?.RoleName ?? string.Empty,
            TotalTasksCount = tasks.Count,
            Tasks = tasks
        };
    }
}

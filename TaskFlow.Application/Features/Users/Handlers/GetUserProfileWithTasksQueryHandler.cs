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
    private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

    public GetUserProfileWithTasksQueryHandler(IRepository<User> usersRepository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
    {
        _usersRepository = usersRepository;
        _currentUser = currentUser;
    }

    public async Task<UserProfileWithTasksDto> Handle(GetUserProfileWithTasksQuery request, CancellationToken cancellationToken)
    {
        var clientId = _currentUser.ClientId ?? throw new UnauthorizedException("Your session does not contain a client. Please sign in again.");
        var user = await _usersRepository.GetAll()
            .AsNoTracking()
            .Include(u => u.Role)
            .Include(u => u.AssignedTasks)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.ClientId == clientId, cancellationToken);

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
            ImageId = user.ImageId ?? Guid.Empty,
            UpdatedAt = user.UpdatedAt,
            UpdatedById = user.UpdatedBy,
            CreatedAt = user.CreatedAt,
            CreatedById = user.CreatedBy ?? Guid.Empty,

            Tasks = tasks
        };
    }
}

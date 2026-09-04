using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Services;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Handlers;

public sealed class SaveGeneratedInitiativesBatchCommandHandler
    : IRequestHandler<SaveGeneratedInitiativesBatchCommand, SaveGeneratedInitiativesBatchResponse>
{
    private readonly IRepository<Initiative> _initiatives;
    private readonly IRepository<TaskItem> _tasks;
    private readonly IRepository<Status> _statuses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

    public SaveGeneratedInitiativesBatchCommandHandler(
        IRepository<Initiative> initiatives,
        IRepository<TaskItem> tasks,
        IRepository<Status> statuses,
        IUnitOfWork unitOfWork,
        TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
    {
        _initiatives = initiatives;
        _tasks = tasks;
        _statuses = statuses;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SaveGeneratedInitiativesBatchResponse> Handle(
        SaveGeneratedInitiativesBatchCommand command,
        CancellationToken cancellationToken)
    {
        var newStatus = await _statuses.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(status => status.Name != null && status.Name.ToLower() == "new", cancellationToken)
            ?? throw new InvalidOperationException("The default 'New' status is not configured for this workspace.");
        var currentUserId = _currentUser.UserId
            ?? throw new UnauthorizedException("Your session does not contain a user. Please sign in again.");

        var initiativeIds = new List<Guid>(command.Request.Initiatives.Count);
        var taskCount = 0;
        foreach (var request in command.Request.Initiatives)
        {
            var initiativeStyle = WorkItemStyleDefaults.ForInitiative(
                request.Name, request.Description, request.Color, request.Icon);
            var initiative = new Initiative
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Progress = 0,
                IsAISuggested = true,
                IsActive = true,
                Color = initiativeStyle.Color,
                Icon = initiativeStyle.Icon,
                StatusId = newStatus.Id,
                AssignedToId = currentUserId
            };
            _initiatives.Add(initiative);
            initiativeIds.Add(initiative.Id);

            foreach (var generatedTask in request.Tasks)
            {
                var taskStyle = WorkItemStyleDefaults.ForTask(
                    generatedTask.Name, generatedTask.Description, generatedTask.Color, generatedTask.Icon);
                _tasks.Add(new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Name = generatedTask.Name.Trim(),
                    Description = generatedTask.Description?.Trim(),
                    StartDate = generatedTask.StartDate,
                    EndDate = generatedTask.EndDate,
                    Progress = 0,
                    IsAISuggested = true,
                    IsActive = true,
                    Color = taskStyle.Color,
                    Icon = taskStyle.Icon,
                    InitiativeId = initiative.Id,
                    StatusId = newStatus.Id,
                    AssignedToId = currentUserId,
                    ParentId = null
                });
                taskCount++;
            }
        }

        // EF Core wraps this single SaveChanges call in a database transaction. Therefore the
        // complete batch succeeds or no initiative/task from it is committed.
        var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (affectedRows <= 0)
            throw new BadRequestException("لم يتم حفظ المبادرات والمهام.");

        return new SaveGeneratedInitiativesBatchResponse
        {
            InitiativeIds = initiativeIds,
            CreatedInitiativesCount = initiativeIds.Count,
            CreatedTasksCount = taskCount,
            Message = "تم إنشاء المبادرات والمهام بنجاح."
        };
    }
}

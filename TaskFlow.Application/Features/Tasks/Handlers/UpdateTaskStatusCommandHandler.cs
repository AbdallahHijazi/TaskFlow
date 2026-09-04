using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TaskDto>
    {
        private readonly IRepository<TaskItem> _taskRepository;
        private readonly IRepository<Status> _statusRepository;
        private readonly IRepository<TaskDependency> _dependencyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;
        private readonly IWorkEventService _workEvents;

        public UpdateTaskStatusCommandHandler(
            IRepository<TaskItem> taskRepository,
            IRepository<Status> statusRepository,
            IRepository<TaskDependency> dependencyRepository,
            IUnitOfWork unitOfWork,
            TaskFlow.Domain.Interfaces.ICurrentUserService currentUser,
            IWorkEventService workEvents)
        {
            _taskRepository = taskRepository;
            _statusRepository = statusRepository;
            _dependencyRepository = dependencyRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _workEvents = workEvents;
        }

        public async Task<TaskDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            var taskQuery = _taskRepository.GetAll();
            if (!_currentUser.IsAdmin)
            {
                var userId = _currentUser.UserId;
                taskQuery = taskQuery.Where(t => userId.HasValue && t.AssignedToId == userId.Value);
            }

            var task = await taskQuery
                .Where(t => t.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
                throw new NotFoundException("المهمة", request.Id);

            if (request.Dto.StatusId.HasValue)
            {
                var statusExists = await _statusRepository.GetAll()
                    .AnyAsync(s => s.Id == request.Dto.StatusId.Value, cancellationToken);

                if (!statusExists)
                    throw new NotFoundException("الحالة", request.Dto.StatusId.Value);
            }

            if (request.Dto.StatusId.HasValue)
            {
                var targetStatusName = await _statusRepository.GetAll()
                    .Where(status => status.Id == request.Dto.StatusId.Value)
                    .Select(status => status.Name)
                    .FirstOrDefaultAsync(cancellationToken);
                var completing = targetStatusName != null && new[] { "completed", "complete", "done", "closed" }
                    .Contains(targetStatusName.Trim().ToLowerInvariant());

                if (completing)
                {
                    var blockers = await _dependencyRepository.GetAll()
                        .Where(dependency => dependency.SuccessorId == task.Id && dependency.Predecessor != null)
                        .Select(dependency => new
                        {
                            Name = dependency.Predecessor!.Name,
                            Progress = dependency.Predecessor.Progress,
                            StatusName = dependency.Predecessor.Status == null ? null : dependency.Predecessor.Status.Name
                        })
                        .ToListAsync(cancellationToken);
                    var completedNames = new[] { "completed", "complete", "done", "closed" };
                    var incomplete = blockers.Where(blocker => blocker.Progress < 100 &&
                            !completedNames.Contains((blocker.StatusName ?? string.Empty).Trim().ToLowerInvariant()))
                        .Select(blocker => blocker.Name ?? "Unnamed task").ToList();

                    if (incomplete.Count > 0)
                        throw new BadRequestException($"This task is blocked by: {string.Join(", ", incomplete)}.");
                }
            }

            var oldStatusName = await _statusRepository.GetAll().Where(status => status.Id == task.StatusId)
                .Select(status => status.Name).FirstOrDefaultAsync(cancellationToken);
            task.StatusId = request.Dto.StatusId;

            _taskRepository.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var newStatusName = await _statusRepository.GetAll().Where(status => status.Id == task.StatusId)
                .Select(status => status.Name).FirstOrDefaultAsync(cancellationToken);
            if (!string.Equals(oldStatusName, newStatusName, StringComparison.OrdinalIgnoreCase))
                await _workEvents.RecordAsync(task.AssignedToId, task.Id, "status_changed", "Task status changed",
                    $"{task.Name} moved from {oldStatusName ?? "No status"} to {newStatusName ?? "No status"}.",
                    oldStatusName, newStatusName, true, cancellationToken);

            return await _taskRepository.GetAll()
                .Where(t => t.Id == task.Id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Progress = t.Progress,
                    StatusId = t.StatusId,
                    StatusName = t.Status == null || string.IsNullOrWhiteSpace(t.Status.Name) ? "Unknown Status" : t.Status.Name,
                    InitiativeId = t.InitiativeId,
                    InitiativeName = t.Initiative == null ? null : t.Initiative.Name,
                    AssignedToId = t.AssignedToId,
                    AssignedToName = t.AssignedTo == null ? null : t.AssignedTo.Name,
                    Icon = t.Icon,
                    Color = t.Color,
                    CreatedById = t.CreatedBy ?? Guid.Empty,
                    ImageId = t.ImageId,
                    ImageUrl = t.ImageId == null ? null : $"/api/Images/{t.ImageId}/file",
                    ThumbnailUrl = t.ImageId == null ? null : $"/api/Images/{t.ImageId}/thumbnail",
                    FilePath = null,
                    ImageFileName = t.Image == null ? null : t.Image.FileName,
                    ImageContentType = t.Image == null ? null : t.Image.MediaType,
                    ImageSizeInBytes = t.Image == null ? null : t.Image.SizeInBytes,
                    UpdatedAt = t.UpdatedAt,
                    UpdatedById = t.UpdatedBy,
                    IsAISuggested = t.IsAISuggested
                })
                .FirstAsync(cancellationToken);
        }
    }
}

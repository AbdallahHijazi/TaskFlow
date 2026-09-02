using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Application.Common.Services;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
    {
        private readonly IRepository<TaskItem> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IWorkEventService _workEvents;
        private readonly IRepository<Status> _statusRepository;

        public UpdateTaskCommandHandler(
            IRepository<TaskItem> repository,
            IUnitOfWork unitOfWork,
            IImageService imageService,
            IWorkEventService workEvents,
            IRepository<Status> statusRepository)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _workEvents = workEvents;
            _statusRepository = statusRepository;
        }

        public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _repository.GetAll()
                .Where(t => t.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
                throw new NotFoundException("المهمة", request.Id);

            var oldAssigneeId = task.AssignedToId;
            var oldEndDate = task.EndDate;
            var oldStatusId = task.StatusId;

            task.Name = request.Dto.Name.Trim();
            task.Description = request.Dto.Description?.Trim();
            task.StartDate = request.Dto.StartDate;
            task.EndDate = request.Dto.EndDate;
            task.Progress = request.Dto.Progress;
            task.StatusId = request.Dto.StatusId;
            task.InitiativeId = request.Dto.InitiativeId;
            task.AssignedToId = request.Dto.AssignedToId;
            var style = WorkItemStyleDefaults.ForTask(request.Dto.Name, request.Dto.Description, request.Dto.Color, request.Dto.Icon);
            task.Color = style.Color;
            task.Icon = style.Icon;

            if (request.Dto.Image != null && request.Dto.Image.Length > 0)
            {
                var imageId = await _imageService.SaveImageAsync(
                    request.Dto.Image,
                    cancellationToken);

                task.ImageId = imageId;
            }

            _repository.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (oldAssigneeId != task.AssignedToId)
                await _workEvents.RecordAsync(task.AssignedToId, task.Id, "task_assigned", "Task assigned to you",
                    $"You were assigned to task: {task.Name}.", oldAssigneeId?.ToString(), task.AssignedToId?.ToString(), true, cancellationToken);
            if (oldEndDate != task.EndDate)
                await _workEvents.RecordAsync(task.AssignedToId, task.Id, "due_date_changed", "Task due date changed",
                    $"The due date for {task.Name} changed to {task.EndDate:MMM d, yyyy}.", oldEndDate?.ToString("O"), task.EndDate?.ToString("O"), true, cancellationToken);
            if (oldStatusId != task.StatusId)
            {
                var statusIds = new[] { oldStatusId, task.StatusId }.Where(id => id.HasValue).Select(id => id!.Value).ToArray();
                var statusNames = await _statusRepository.GetAll().Where(status => statusIds.Contains(status.Id))
                    .ToDictionaryAsync(status => status.Id, status => status.Name, cancellationToken);
                var oldStatusName = oldStatusId.HasValue && statusNames.TryGetValue(oldStatusId.Value, out var oldName) ? oldName : "No status";
                var newStatusName = task.StatusId.HasValue && statusNames.TryGetValue(task.StatusId.Value, out var newName) ? newName : "No status";
                await _workEvents.RecordAsync(task.AssignedToId, task.Id, "status_changed", "Task status changed",
                    $"{task.Name} moved from {oldStatusName} to {newStatusName}.", oldStatusName, newStatusName, true, cancellationToken);
            }

            return await _repository.GetAll()
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
                    Color = t.Color,
                    Icon = t.Icon,
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

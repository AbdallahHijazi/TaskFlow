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
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTaskStatusCommandHandler(
            IRepository<TaskItem> taskRepository,
            IRepository<Status> statusRepository,
            IUnitOfWork unitOfWork)
        {
            _taskRepository = taskRepository;
            _statusRepository = statusRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TaskDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
        {
            var task = await _taskRepository.GetAll()
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

            task.StatusId = request.Dto.StatusId;

            _taskRepository.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

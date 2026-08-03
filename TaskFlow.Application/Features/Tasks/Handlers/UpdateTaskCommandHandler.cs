using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
    {
        private readonly IRepository<TaskItem> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public UpdateTaskCommandHandler(
            IRepository<TaskItem> repository,
            IUnitOfWork unitOfWork,
            IImageService imageService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _repository.GetAll()
                .Where(t => t.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
                throw new NotFoundException("المهمة", request.Id);

            task.Name = request.Dto.Name.Trim();
            task.Description = request.Dto.Description?.Trim();
            task.StartDate = request.Dto.StartDate;
            task.EndDate = request.Dto.EndDate;
            task.Progress = request.Dto.Progress;
            task.StatusId = request.Dto.StatusId;
            task.InitiativeId = request.Dto.InitiativeId;
            task.AssignedToId = request.Dto.AssignedToId;
            task.Color = request.Dto.Color;
            task.Icon = request.Dto.Icon;

            if (request.Dto.Image != null && request.Dto.Image.Length > 0)
            {
                var imageId = await _imageService.SaveImageAsync(
                    request.Dto.Image,
                    cancellationToken);

                task.ImageId = imageId;
            }

            _repository.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

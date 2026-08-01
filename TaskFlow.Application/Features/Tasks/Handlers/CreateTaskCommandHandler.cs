using MediatR;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
    {
        private readonly IRepository<TaskItem> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IRepository<Initiative> _initiativeRepository;

        public CreateTaskCommandHandler(IRepository<TaskItem> repository, IRepository<Initiative> initiativeRepository, IUnitOfWork unitOfWork,IImageService imageService)
        {
            _repository = repository;
            _initiativeRepository = initiativeRepository;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Guid? imageId = null;
                if (request.Dto.Image != null && request.Dto.Image.Length > 0)
                {
                    imageId = await _imageService.SaveImageAsync(request.Dto.Image, cancellationToken);
                }
                var initiativeExists = await _initiativeRepository
                    .GetAll()
                    .AnyAsync(
                                i => i.Id == request.Dto.InitiativeId,
                                cancellationToken
                    );

                if (!initiativeExists)
                {
                    throw new InvalidOperationException(
                        "المبادرة المحددة غير موجودة.");
                }
                var task = new TaskItem
                {
                    Name = request.Dto.Name.Trim(),
                    Description = request.Dto.Description?.Trim(),
                    StartDate = request.Dto.StartDate,
                    EndDate = request.Dto.EndDate,
                    Progress = request.Dto.Progress,
                    StatusId = request.Dto.StatusId,
                    InitiativeId = request.Dto.InitiativeId,
                    AssignedToId = request.Dto.AssignedToId,
                    Color = request.Dto.Color,
                    Icon = request.Dto.Icon,
                    ImageId = imageId,
                };

                _repository.Add(task);
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
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء إنشاء المهمة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}

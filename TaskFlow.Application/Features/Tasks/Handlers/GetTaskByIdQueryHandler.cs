using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
    {
        private readonly IRepository<TaskItem> _repository;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

        public GetTaskByIdQueryHandler(IRepository<TaskItem> repository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll();
            if (!_currentUser.IsAdmin)
            {
                var userId = _currentUser.UserId;
                query = query.Where(t => userId.HasValue && t.AssignedToId == userId.Value);
            }

            var task = await query
                .AsNoTracking()
                .Where(t => t.Id == request.Id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Progress = t.Progress,
                    //Priority = t.Priority,
                    StatusId = t.StatusId,
                    InitiativeId = t.InitiativeId,
                    AssignedToId = t.AssignedToId,
                    Color = t.Color,
                    Icon=t.Icon,
                    CreatedById = t.CreatedBy ?? Guid.Empty,
                    CreatedAt = t.CreatedAt,
                    ImageId = t.ImageId,
                    ImageUrl = t.ImageId == null ? null : $"/api/Images/{t.ImageId}/file",
                    ThumbnailUrl = t.ImageId == null ? null : $"/api/Images/{t.ImageId}/thumbnail",
                    FilePath = null,
                    ImageFileName = t.Image == null ? null : t.Image.FileName,
                    ImageContentType = t.Image == null ? null : t.Image.MediaType,
                    ImageSizeInBytes = t.Image == null ? null : t.Image.SizeInBytes,
                    UpdatedAt = t.UpdatedAt,
                    UpdatedById = t.UpdatedBy,
                    StatusName = t.Status == null || string.IsNullOrWhiteSpace(t.Status.Name) ? "Unknown Status" : t.Status.Name,
                    InitiativeName = t.Initiative == null ? null : t.Initiative.Name,
                    AssignedToName = t.AssignedTo == null ? null : t.AssignedTo.Name,
                    IsAISuggested = t.IsAISuggested,
                    
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
                throw new NotFoundException("المهمة", request.Id);

            return task;
        }
    }
}

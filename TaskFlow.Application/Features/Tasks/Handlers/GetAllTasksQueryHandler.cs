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
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, PagedResultDto<TaskDto>>
    {
        private readonly IRepository<TaskItem> _repository;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

        public GetAllTasksQueryHandler(IRepository<TaskItem> repository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<PagedResultDto<TaskDto>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll();
            if (!_currentUser.IsAdmin)
            {
                var userId = _currentUser.UserId;
                query = query.Where(t => userId.HasValue && t.AssignedToId == userId.Value);
            }

            if (request.AssignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == request.AssignedToId.Value);
            if (request.StatusId.HasValue)
                query = query.Where(t => t.StatusId == request.StatusId.Value);
            if (request.InitiativeId.HasValue)
                query = query.Where(t => t.InitiativeId == request.InitiativeId.Value);
            if (request.Search is not null)
            {
                var search = request.Search.ToLower();
                query = query.Where(t =>
                    (t.Name != null && t.Name.ToLower().Contains(search)) ||
                    (t.Description != null && t.Description.ToLower().Contains(search)) ||
                    (t.Status != null && t.Status.Name != null && t.Status.Name.ToLower().Contains(search)) ||
                    (t.Initiative != null && t.Initiative.Name != null && t.Initiative.Name.ToLower().Contains(search)) ||
                    (t.AssignedTo != null && t.AssignedTo.Name != null && t.AssignedTo.Name.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var tasks = await query
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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
                    StatusName = t.Status == null || string.IsNullOrWhiteSpace(t.Status.Name) ? "Unknown Status" : t.Status.Name,
                    InitiativeName = t.Initiative == null ? null : t.Initiative.Name,
                    IsAISuggested = t.IsAISuggested,
                    AssignedToName = t.AssignedTo == null ? null : t.AssignedTo.Name,
                })
                .ToListAsync(cancellationToken);

            return new PagedResultDto<TaskDto> { Items = tasks, PageNumber = request.PageNumber, PageSize = request.PageSize,
                TotalCount = totalCount, TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize) };
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Initiative;
using TaskFlow.Application.Features.Initiatives.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Initiatives.Handlers
{
    public class GetAllInitiativesQueryHandler : IRequestHandler<GetAllInitiativesQuery, PagedResultDto<InitiativeDto>>
    {
        private readonly IRepository<Initiative> _repository;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

        public GetAllInitiativesQueryHandler(IRepository<Initiative> repository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<PagedResultDto<InitiativeDto>> Handle(GetAllInitiativesQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll();
            if (!_currentUser.IsAdmin)
            {
                var userId = _currentUser.UserId;
                query = query.Where(i => userId.HasValue && (i.AssignedToId == userId.Value || i.Tasks.Any(t => t.AssignedToId == userId.Value)));
            }

            if (request.AssignedToId.HasValue)
                query = query.Where(i => i.AssignedToId == request.AssignedToId.Value);
            if (request.StatusId.HasValue)
                query = query.Where(i => i.StatusId == request.StatusId.Value);
            if (request.Search is not null)
            {
                var search = request.Search.ToLower();
                query = query.Where(i =>
                    (i.Name != null && i.Name.ToLower().Contains(search)) ||
                    (i.Description != null && i.Description.ToLower().Contains(search)) ||
                    (i.Status != null && i.Status.Name != null && i.Status.Name.ToLower().Contains(search)) ||
                    (i.AssignedTo != null && i.AssignedTo.Name != null && i.AssignedTo.Name.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var initiatives = await query
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new InitiativeDto
                {
                    Id = i.Id,
                    Name = i.Name ?? string.Empty,
                    Description = i.Description,
                    StartDate = i.StartDate,
                    EndDate = i.EndDate,
                    Progress = i.Tasks.Any()
                        ? i.Tasks.Average(task => task.Progress ?? 0)
                        : 0,
                    IsAISuggested = i.IsAISuggested,
                    ImageId = i.ImageId,
                    CreatedBy = i.CreatedBy,
                    UpdatedAt = i.UpdatedAt,
                    UpdatedBy = i.UpdatedBy,
                    StatusId = i.StatusId,
                    Color = i.Color!,
                    Icon = i.Icon!,
                    AssignedTo = i.AssignedToId,
                    AssignedToName = i.AssignedTo == null ? null : i.AssignedTo.Name,
                    TaskCount = i.Tasks.Count,
                })
                .ToListAsync(cancellationToken);

            return new PagedResultDto<InitiativeDto> { Items = initiatives, PageNumber = request.PageNumber, PageSize = request.PageSize,
                TotalCount = totalCount, TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize) };
        }
    }
}

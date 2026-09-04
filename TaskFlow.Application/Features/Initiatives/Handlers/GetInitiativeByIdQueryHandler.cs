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
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Initiatives.Handlers
{
    public class GetInitiativeByIdQueryHandler : IRequestHandler<GetInitiativeByIdQuery, InitiativeDto>
    {
        private readonly IRepository<Initiative> _repository;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

        public GetInitiativeByIdQueryHandler(IRepository<Initiative> repository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<InitiativeDto> Handle(GetInitiativeByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll();
            if (!_currentUser.IsAdmin)
            {
                var userId = _currentUser.UserId;
                query = query.Where(i => userId.HasValue && (i.AssignedToId == userId.Value || i.Tasks.Any(t => t.AssignedToId == userId.Value)));
            }

            var initiative = await query
                .AsNoTracking()
                .Where(i => i.Id == request.Id)
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
                    AssignedTo = i.AssignedToId,
                    Color = i.Color!,
                    Icon = i.Icon!,
                    StatusId = i.StatusId,
                    StatusName = i.Status == null ? null : i.Status.Name,
                    AssignedToName = i.AssignedTo == null ? null : i.AssignedTo.Name,
                    CreatedBy = i.CreatedBy,
                    CreatedAt = i.CreatedAt,
                    ImageUrl = i.ImageId == null ? null : $"/api/Images/{i.ImageId}/file",
                    ThumbnailUrl = i.ImageId == null ? null : $"/api/Images/{i.ImageId}/thumbnail",
                    ImageFileName = i.Image == null ? null : i.Image.FileName,
                    ImageSizeInBytes = i.Image == null ? null : i.Image.SizeInBytes,
                    TaskCount = i.Tasks.Count,
                    UpdatedAt = i.UpdatedAt,
                    UpdatedBy = i.UpdatedBy
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (initiative == null)
                throw new NotFoundException("المبادرة", request.Id);

            return initiative;
        }
    }
}

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

namespace TaskFlow.Application.Features.Initiatives.Handlers
{
    public class GetAllInitiativesQueryHandler : IRequestHandler<GetAllInitiativesQuery, List<InitiativeDto>>
    {
        private readonly IRepository<Initiative> _repository;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

        public GetAllInitiativesQueryHandler(IRepository<Initiative> repository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<List<InitiativeDto>> Handle(GetAllInitiativesQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll();
            if (!_currentUser.IsAdmin)
            {
                var userId = _currentUser.UserId;
                query = query.Where(i => userId.HasValue && (i.AssignedToId == userId.Value || i.Tasks.Any(t => t.AssignedToId == userId.Value)));
            }

            var initiatives = await query
                .AsNoTracking()
                .Select(i => new InitiativeDto
                {
                    Id = i.Id,
                    Name = i.Name ?? string.Empty,
                    Description = i.Description,
                    StartDate = i.StartDate,
                    EndDate = i.EndDate,
                    Progress = i.Progress,
                    IsAISuggested = i.IsAISuggested,
                    ImageId = i.ImageId,
                    CreatedBy = i.CreatedBy,
                    UpdatedAt = i.UpdatedAt,
                    UpdatedBy = i.UpdatedBy,
                    StatusId = i.StatusId,
                    Color = i.Color!,
                    Icon = i.Icon!,
                    AssignedTo = i.AssignedToId,
                })
                .ToListAsync(cancellationToken);

            return initiatives;
        }
    }
}

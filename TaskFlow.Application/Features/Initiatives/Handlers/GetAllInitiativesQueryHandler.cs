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

        public GetAllInitiativesQueryHandler(IRepository<Initiative> repository)
        {
            _repository = repository;
        }

        public async Task<List<InitiativeDto>> Handle(GetAllInitiativesQuery request, CancellationToken cancellationToken)
        {
            var initiatives = await _repository.GetAll()
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
                    UpdatedBy = i.UpdatedBy
                })
                .ToListAsync(cancellationToken);

            return initiatives;
        }
    }
}

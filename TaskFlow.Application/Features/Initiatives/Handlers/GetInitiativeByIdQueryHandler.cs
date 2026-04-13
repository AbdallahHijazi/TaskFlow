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

        public GetInitiativeByIdQueryHandler(IRepository<Initiative> repository)
        {
            _repository = repository;
        }

        public async Task<InitiativeDto> Handle(GetInitiativeByIdQuery request, CancellationToken cancellationToken)
        {
            var initiative = await _repository.GetAll()
                .AsNoTracking()
                .Where(i => i.Id == request.Id)
                .Select(i => new InitiativeDto
                {
                    Id = i.Id,
                    Name = i.Name,
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
                .FirstOrDefaultAsync(cancellationToken);

            if (initiative == null)
                throw new NotFoundException("المبادرة", request.Id);

            return initiative;
        }
    }
}

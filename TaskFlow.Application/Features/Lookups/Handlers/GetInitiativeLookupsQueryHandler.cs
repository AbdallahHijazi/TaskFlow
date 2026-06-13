using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Lookup;
using TaskFlow.Application.Features.Lookups.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Lookups.Handlers
{
    public class GetInitiativeLookupsQueryHandler:IRequestHandler<GetInitiativeLookupsQuery, List<LookupItemDto>>
    {
        private readonly IRepository<Initiative> repository;

        public GetInitiativeLookupsQueryHandler(IRepository<Initiative> repository)
        {
            this.repository = repository;
        }

        public async Task<List<LookupItemDto>> Handle(GetInitiativeLookupsQuery request, CancellationToken cancellationToken)
        {
            var initiatives = await repository.GetAll()
                .Select(i => new LookupItemDto
                {
                    Id = i.Id,
                    Name = i.Name ?? string.Empty
                })
                .ToListAsync(cancellationToken);
            return initiatives;
        }
    }
}

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
    public class GetStatusLookupsQueryHandler: IRequestHandler<GetStatusLookupsQuery, List<LookupItemDto>>
    {
        private readonly IRepository<Status> repository;

        public GetStatusLookupsQueryHandler(IRepository<Status> repository)
        {
            this.repository = repository;
        }

        public async Task<List<LookupItemDto>> Handle(GetStatusLookupsQuery request, CancellationToken cancellationToken)
        {
            var statuses = await repository.GetAll()
                .Select(s => new LookupItemDto
                {
                    Id = s.Id,
                    Name = s.Name ?? string.Empty
                })
                .ToListAsync(cancellationToken);
            return statuses;
        }
    }
}

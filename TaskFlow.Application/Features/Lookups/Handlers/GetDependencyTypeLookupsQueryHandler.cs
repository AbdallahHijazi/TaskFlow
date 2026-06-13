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
    public class GetDependencyTypeLookupsQueryHandler : IRequestHandler<GetDependencyTypeLookupsQuery, List<LookupItemDto>>
    {
        private readonly IRepository<DependencyType> repository;

        public GetDependencyTypeLookupsQueryHandler(IRepository<DependencyType> repository)
        {
            this.repository = repository;
        }

        public async Task<List<LookupItemDto>> Handle(GetDependencyTypeLookupsQuery request, CancellationToken cancellationToken)
        {
            var dependencyTypes = await repository.GetAll()
                .Select(dt => new LookupItemDto
                {
                    Id = dt.Id,
                    Name = dt.Name ?? string.Empty
                })
                .ToListAsync(cancellationToken);
            return dependencyTypes;
        }
    }
}

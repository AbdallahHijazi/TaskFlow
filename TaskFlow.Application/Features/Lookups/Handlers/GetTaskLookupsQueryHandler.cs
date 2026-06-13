using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Lookup;
using TaskFlow.Application.Features.Lookups.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Lookups.Handlers
{
    public class GetTaskLookupsQueryHandler : IRequestHandler<GetTaskLookupsQuery, List<LookupItemDto>>
    {
        private readonly IRepository<TaskItem> repository;

        public GetTaskLookupsQueryHandler(IRepository<TaskItem> repository)
        {
            this.repository = repository;
        }

        public async Task<List<LookupItemDto>> Handle(GetTaskLookupsQuery request, CancellationToken cancellationToken)
        {
            var tasks = await repository.GetAll()
                .Select(t => new LookupItemDto
                {
                    Id = t.Id,
                    Name = t.Name ?? string.Empty
                })
                .ToListAsync(cancellationToken);
            return tasks;
        }
    }
}

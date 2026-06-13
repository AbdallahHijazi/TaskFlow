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
    public class GetUserLookupsQueryHandler : IRequestHandler<GetUserLookupsQuery, List<LookupItemDto>>
    {
        private readonly IRepository<User> repository;

        public GetUserLookupsQueryHandler(IRepository<User> repository)
        {
            this.repository = repository;
        }

        public async Task<List<LookupItemDto>> Handle(GetUserLookupsQuery request, CancellationToken cancellationToken)
        {
            var users = await repository.GetAll()
                .Select(u => new LookupItemDto
                {
                    Id = u.Id,
                    Name = u.Name ?? string.Empty
                })
                .ToListAsync(cancellationToken);
            return users;
        }
    }
}

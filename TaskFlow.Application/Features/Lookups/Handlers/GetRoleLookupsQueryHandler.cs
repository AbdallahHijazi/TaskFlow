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
    public class GetRoleLookupsQueryHandler:IRequestHandler<GetRoleLookupsQuery, List<LookupItemDto>>
    {
        private readonly IRepository<Role> repository;

        public GetRoleLookupsQueryHandler(IRepository<Role> repository)
        {
            this.repository = repository;
        }

        public async Task<List<LookupItemDto>> Handle(GetRoleLookupsQuery request, CancellationToken cancellationToken)
        {
            var roles = await repository.GetAll()
                .Select(r => new LookupItemDto
                {
                    Id = r.RoleId,
                    Name = r.RoleName ?? string.Empty
                })
                .ToListAsync(cancellationToken);
            return roles;
        }
    }
}

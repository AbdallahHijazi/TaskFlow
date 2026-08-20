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
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService currentUser;

        public GetUserLookupsQueryHandler(IRepository<User> repository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            this.repository = repository;
            this.currentUser = currentUser;
        }

        public async Task<List<LookupItemDto>> Handle(GetUserLookupsQuery request, CancellationToken cancellationToken)
        {
            var clientId = currentUser.ClientId ?? throw new TaskFlow.Domain.Exceptions.UnauthorizedException("Your session does not contain a client. Please sign in again.");
            var users = await repository.GetAll().Where(u => u.ClientId == clientId)
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

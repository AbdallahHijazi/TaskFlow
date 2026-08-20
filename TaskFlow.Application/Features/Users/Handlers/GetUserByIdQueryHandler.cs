using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Users.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Users.Handlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly IRepository<User> _repository;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService currentUser;

        public GetUserByIdQueryHandler(IRepository<User> repository, TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            this.currentUser = currentUser;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var clientId = currentUser.ClientId ?? throw new TaskFlow.Domain.Exceptions.UnauthorizedException("Your session does not contain a client. Please sign in again.");
            var user = await _repository.GetAll()
                .AsNoTracking()
                .Where(u => u.Id == request.Id && u.ClientId == clientId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    RoleId = u.RoleId ?? Guid.Empty,
                    CreatedAt=u.CreatedAt,
                    CreatedById = u.CreatedBy ?? Guid.Empty,
                    ImageId = u.ImageId,
                    ClientId = u.ClientId,
                    ClientName = u.Client != null ? u.Client.Name : string.Empty,
                    RoleName= u.Role != null ? u.Role.RoleName : string.Empty,
                    UpdatedAt=u.UpdatedAt,
                    UpdatedById=u.UpdatedBy ?? Guid.Empty,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new NotFoundException("المستخدم", request.Id);

            return user;
        }
    }
}

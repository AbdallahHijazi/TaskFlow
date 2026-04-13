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

namespace TaskFlow.Application.Features.Users.Handlers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly IRepository<User> _repository;

        public GetAllUsersQueryHandler(IRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _repository.GetAll()
                    .AsNoTracking()
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Name = u.Name ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        PhoneNumber = u.PhoneNumber,
                        RoleId = u.RoleId ?? Guid.Empty
                    })
                    .ToListAsync(cancellationToken);

                return users;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء جلب قائمة المستخدمين. يرجى المحاولة مرة أخرى لاحقاً.");
            }
        }
    }
}

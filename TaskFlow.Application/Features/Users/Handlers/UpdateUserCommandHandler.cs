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
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IRepository<User> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandHandler(IRepository<User> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _repository.GetAll()
                    .Where(u => u.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                    throw new NotFoundException("المستخدم", request.Id);

                if (string.IsNullOrWhiteSpace(request.Dto.Name))
                    throw new InvalidOperationException("اسم المستخدم مطلوب");

                if (string.IsNullOrWhiteSpace(request.Dto.Email))
                    throw new InvalidOperationException("البريد الإلكتروني مطلوب");

                user.Name = request.Dto.Name.Trim();
                user.Email = request.Dto.Email.Trim().ToLower();
                user.PhoneNumber = request.Dto.PhoneNumber?.Trim();

                if (request.Dto.RoleId != Guid.Empty)
                    user.RoleId = request.Dto.RoleId;

                _repository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleId = user.RoleId ?? Guid.Empty
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء تحديث المستخدم. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}

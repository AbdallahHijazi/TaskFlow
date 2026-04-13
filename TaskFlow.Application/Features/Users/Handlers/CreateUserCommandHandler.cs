using MediatR;
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
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly IRepository<User> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserPasswordHasher _passwordHasher;

        public CreateUserCommandHandler(
            IRepository<User> repository,
            IUnitOfWork unitOfWork,
            IUserPasswordHasher passwordHasher)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Dto.Password != request.Dto.ConfirmPassword)
                {
                    throw new InvalidOperationException("كلمة المرور وتأكيدها غير متطابقين");
                }

                if (string.IsNullOrWhiteSpace(request.Dto.Name))
                    throw new InvalidOperationException("اسم المستخدم مطلوب");

                if (string.IsNullOrWhiteSpace(request.Dto.Email))
                    throw new InvalidOperationException("البريد الإلكتروني مطلوب");

                if (string.IsNullOrWhiteSpace(request.Dto.Password))
                    throw new InvalidOperationException("كلمة المرور مطلوبة");

                if (request.Dto.RoleId == Guid.Empty)
                    throw new InvalidOperationException("يجب اختيار دور للمستخدم");

                var user = new User
                {
                    Name = request.Dto.Name.Trim(),
                    Email = request.Dto.Email.Trim().ToLower(),
                    Password = _passwordHasher.HashPassword(request.Dto.Password),
                    PhoneNumber = request.Dto.PhoneNumber?.Trim(),
                    RoleId = request.Dto.RoleId
                };

                _repository.Add(user);
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
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء إنشاء المستخدم. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}

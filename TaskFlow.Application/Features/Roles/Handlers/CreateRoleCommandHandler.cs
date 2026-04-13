using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Role;
using TaskFlow.Application.Features.Roles.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Roles.Handlers
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
    {
        private readonly IRepository<Role> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRoleCommandHandler(IRepository<Role> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Dto.RoleName))
                    throw new InvalidOperationException("اسم الدور مطلوب");

                var role = new Role
                {
                    RoleName = request.Dto.RoleName.Trim()
                };

                _repository.Add(role);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new RoleDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName
                };
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء إنشاء الدور. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}

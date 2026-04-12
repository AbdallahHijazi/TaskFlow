using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Role;

namespace TaskFlow.Application.Features.Roles.Commands
{
    public class CreateRoleCommand : IRequest<RoleDto>
    {
        public CreateRoleDto Dto { get; set; }

        public CreateRoleCommand(CreateRoleDto dto)
        {
            Dto = dto;
        }
    }
}

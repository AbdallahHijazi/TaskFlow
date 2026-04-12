using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.User;

namespace TaskFlow.Application.Features.Users.Commands
{
    public class CreateUserCommand : IRequest<UserDto>
    {
        public CreateUserDto Dto { get; set; }

        public CreateUserCommand(CreateUserDto dto)
        {
            Dto = dto;
        }
    }
}

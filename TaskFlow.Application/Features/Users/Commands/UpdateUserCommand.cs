using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.User;

namespace TaskFlow.Application.Features.Users.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public Guid Id { get; set; }
        public UpdateUserDto Dto { get; set; }   

        public UpdateUserCommand(Guid id, UpdateUserDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}

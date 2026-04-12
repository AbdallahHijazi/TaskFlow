using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.User;

namespace TaskFlow.Application.Features.Users.Commands
{
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
    }
}

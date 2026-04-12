using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.Features.Tasks.Commands
{
    public class CreateTaskCommand : IRequest<TaskDto>
    {
        public CreateTaskDto Dto { get; set; }

        public CreateTaskCommand(CreateTaskDto dto)
        {
            Dto = dto;
        }
    }
}

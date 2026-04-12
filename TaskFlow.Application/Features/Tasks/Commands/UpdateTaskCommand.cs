using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.Features.Tasks.Commands
{
    public class UpdateTaskCommand : IRequest<TaskDto>
    {
        public Guid Id { get; set; }
        public CreateTaskDto Dto { get; set; }

        public UpdateTaskCommand(Guid id, CreateTaskDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}

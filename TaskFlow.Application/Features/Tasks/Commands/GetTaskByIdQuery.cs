using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.Features.Tasks.Commands
{
    public class GetTaskByIdQuery : IRequest<TaskDto>
    {
        public Guid Id { get; set; }

        public GetTaskByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}

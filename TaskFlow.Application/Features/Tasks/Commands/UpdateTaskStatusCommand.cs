using MediatR;
using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.Features.Tasks.Commands
{
    public class UpdateTaskStatusCommand : IRequest<TaskDto>
    {
        public Guid Id { get; set; }
        public UpdateTaskStatusDto Dto { get; set; }

        public UpdateTaskStatusCommand(Guid id, UpdateTaskStatusDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}

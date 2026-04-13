using MediatR;
using TaskFlow.Application.DTOs.TaskDependency;

namespace TaskFlow.Application.Features.TaskDependencies.Commands;

public class UpdateTaskDependencyCommand : IRequest<TaskDependencyDto>
{
    public Guid Id { get; set; }
    public CreateTaskDependencyDto Dto { get; set; }

    public UpdateTaskDependencyCommand(Guid id, CreateTaskDependencyDto dto)
    {
        Id = id;
        Dto = dto;
    }
}

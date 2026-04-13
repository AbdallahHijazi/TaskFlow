using MediatR;
using TaskFlow.Application.DTOs.TaskDependency;

namespace TaskFlow.Application.Features.TaskDependencies.Commands;

public class CreateTaskDependencyCommand : IRequest<TaskDependencyDto>
{
    public CreateTaskDependencyDto Dto { get; set; }

    public CreateTaskDependencyCommand(CreateTaskDependencyDto dto)
    {
        Dto = dto;
    }
}

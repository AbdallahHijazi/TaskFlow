using MediatR;
using TaskFlow.Application.DTOs.TaskDependency;

namespace TaskFlow.Application.Features.TaskDependencies.Commands;

public class GetTaskDependencyByIdQuery : IRequest<TaskDependencyDto>
{
    public Guid Id { get; set; }

    public GetTaskDependencyByIdQuery(Guid id)
    {
        Id = id;
    }
}

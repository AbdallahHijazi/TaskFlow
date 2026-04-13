using MediatR;

namespace TaskFlow.Application.Features.TaskDependencies.Commands;

public class DeleteTaskDependencyCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteTaskDependencyCommand(Guid id)
    {
        Id = id;
    }
}

using MediatR;

namespace TaskFlow.Application.Features.DependencyTypes.Commands;

public class DeleteDependencyTypeCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteDependencyTypeCommand(Guid id)
    {
        Id = id;
    }
}

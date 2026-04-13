using MediatR;
using TaskFlow.Application.DTOs.DependencyType;

namespace TaskFlow.Application.Features.DependencyTypes.Commands;

public class GetDependencyTypeByIdQuery : IRequest<DependencyTypeDto>
{
    public Guid Id { get; set; }

    public GetDependencyTypeByIdQuery(Guid id)
    {
        Id = id;
    }
}

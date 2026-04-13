using MediatR;
using TaskFlow.Application.DTOs.DependencyType;

namespace TaskFlow.Application.Features.DependencyTypes.Commands;

public class UpdateDependencyTypeCommand : IRequest<DependencyTypeDto>
{
    public Guid Id { get; set; }
    public CreateDependencyTypeDto Dto { get; set; }

    public UpdateDependencyTypeCommand(Guid id, CreateDependencyTypeDto dto)
    {
        Id = id;
        Dto = dto;
    }
}

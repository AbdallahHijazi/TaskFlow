using MediatR;
using TaskFlow.Application.DTOs.DependencyType;

namespace TaskFlow.Application.Features.DependencyTypes.Commands;

public class CreateDependencyTypeCommand : IRequest<DependencyTypeDto>
{
    public CreateDependencyTypeDto Dto { get; set; }

    public CreateDependencyTypeCommand(CreateDependencyTypeDto dto)
    {
        Dto = dto;
    }
}

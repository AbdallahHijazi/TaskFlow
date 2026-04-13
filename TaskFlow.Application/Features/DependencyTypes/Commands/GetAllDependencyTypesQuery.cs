using MediatR;
using TaskFlow.Application.DTOs.DependencyType;

namespace TaskFlow.Application.Features.DependencyTypes.Commands;

public class GetAllDependencyTypesQuery : IRequest<List<DependencyTypeDto>>
{
}

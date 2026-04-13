using MediatR;
using TaskFlow.Application.DTOs.TaskDependency;

namespace TaskFlow.Application.Features.TaskDependencies.Commands;

public class GetAllTaskDependenciesQuery : IRequest<List<TaskDependencyDto>>
{
}

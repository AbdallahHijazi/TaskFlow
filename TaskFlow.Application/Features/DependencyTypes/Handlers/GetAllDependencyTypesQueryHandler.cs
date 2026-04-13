using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.DependencyType;
using TaskFlow.Application.Features.DependencyTypes.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.DependencyTypes.Handlers;

public class GetAllDependencyTypesQueryHandler : IRequestHandler<GetAllDependencyTypesQuery, List<DependencyTypeDto>>
{
    private readonly IRepository<DependencyType> _repository;

    public GetAllDependencyTypesQueryHandler(IRepository<DependencyType> repository)
    {
        _repository = repository;
    }

    public async Task<List<DependencyTypeDto>> Handle(GetAllDependencyTypesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAll()
            .AsNoTracking()
            .Select(d => new DependencyTypeDto
            {
                Id = d.Id,
                Name = d.Name ?? string.Empty,
                Description = d.Description
            })
            .ToListAsync(cancellationToken);
    }
}

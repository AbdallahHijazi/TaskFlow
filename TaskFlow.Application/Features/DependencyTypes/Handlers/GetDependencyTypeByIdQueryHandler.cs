using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.DependencyType;
using TaskFlow.Application.Features.DependencyTypes.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.DependencyTypes.Handlers;

public class GetDependencyTypeByIdQueryHandler : IRequestHandler<GetDependencyTypeByIdQuery, DependencyTypeDto>
{
    private readonly IRepository<DependencyType> _repository;

    public GetDependencyTypeByIdQueryHandler(IRepository<DependencyType> repository)
    {
        _repository = repository;
    }

    public async Task<DependencyTypeDto> Handle(GetDependencyTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetAll().AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("نوع التبعية", request.Id);

        return new DependencyTypeDto
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Description = entity.Description
        };
    }
}

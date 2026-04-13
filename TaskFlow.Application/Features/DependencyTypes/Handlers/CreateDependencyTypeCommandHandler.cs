using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.DependencyType;
using TaskFlow.Application.Features.DependencyTypes.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.DependencyTypes.Handlers;

public class CreateDependencyTypeCommandHandler : IRequestHandler<CreateDependencyTypeCommand, DependencyTypeDto>
{
    private readonly IRepository<DependencyType> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDependencyTypeCommandHandler(IRepository<DependencyType> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DependencyTypeDto> Handle(CreateDependencyTypeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Dto.Name))
            throw new BadRequestException("اسم نوع التبعية مطلوب");

        var entity = new DependencyType
        {
            Name = request.Dto.Name.Trim(),
            Description = request.Dto.Description?.Trim()
        };

        _repository.Add(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DependencyTypeDto
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Description = entity.Description
        };
    }
}

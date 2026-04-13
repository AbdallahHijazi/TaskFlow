using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.DependencyType;
using TaskFlow.Application.Features.DependencyTypes.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.DependencyTypes.Handlers;

public class UpdateDependencyTypeCommandHandler : IRequestHandler<UpdateDependencyTypeCommand, DependencyTypeDto>
{
    private readonly IRepository<DependencyType> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDependencyTypeCommandHandler(IRepository<DependencyType> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DependencyTypeDto> Handle(UpdateDependencyTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetAll().FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException("نوع التبعية", request.Id);

        if (string.IsNullOrWhiteSpace(request.Dto.Name))
            throw new BadRequestException("اسم نوع التبعية مطلوب");

        entity.Name = request.Dto.Name.Trim();
        entity.Description = request.Dto.Description?.Trim();

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DependencyTypeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }
}

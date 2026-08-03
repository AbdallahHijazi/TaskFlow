using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Services;
using TaskFlow.Application.DTOs.Initiative;
using TaskFlow.Application.Features.Initiatives.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Initiatives.Handlers
{
    public class CreateInitiativeCommandHandler : IRequestHandler<CreateInitiativeCommand, InitiativeDto>
    {
        private readonly IRepository<Initiative> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService imageService;

        public CreateInitiativeCommandHandler(
            IRepository<Initiative> repository, 
            IUnitOfWork unitOfWork,
            IImageService imageService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            this.imageService = imageService;
        }

        public async Task<InitiativeDto> Handle(CreateInitiativeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var imageId = await imageService.SaveImageAsync(request.Dto.Image, cancellationToken);

                var initiative = new Initiative
                {
                    Name = request.Dto.Name.Trim(),
                    Description = request.Dto.Description?.Trim(),
                    StartDate = request.Dto.StartDate,
                    EndDate = request.Dto.EndDate,
                    Progress = request.Dto.Progress,
                    IsAISuggested = request.Dto.IsAISuggested,
                    AssignedToId = request.Dto.AssignedTo,
                    Color = request.Dto.Color,
                    Icon = request.Dto.Icon,
                    StatusId = request.Dto.StatusId,
                    ImageId = imageId,
                };

                _repository.Add(initiative);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new InitiativeDto
                {
                    Id = initiative.Id,
                    Name = initiative.Name,
                    Description = initiative.Description,
                    StartDate = initiative.StartDate,
                    EndDate = initiative.EndDate,
                    Progress = initiative.Progress,
                    IsAISuggested = initiative.IsAISuggested,
                    AssignedTo = initiative.AssignedToId ?? Guid.Empty,
                    Color = initiative.Color,
                    Icon = initiative.Icon,
                    StatusId = initiative.StatusId ?? Guid.Empty,
                    ImageId = imageId,
                };
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء إنشاء المبادرة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}

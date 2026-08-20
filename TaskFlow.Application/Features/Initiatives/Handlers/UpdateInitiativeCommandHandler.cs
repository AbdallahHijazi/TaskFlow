using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Initiative;
using TaskFlow.Application.Features.Initiatives.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Application.Common.Services;

namespace TaskFlow.Application.Features.Initiatives.Handlers
{
    public class UpdateInitiativeCommandHandler
    : IRequestHandler<UpdateInitiativeCommand, InitiativeDto>
    {
        private readonly IRepository<Initiative> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public UpdateInitiativeCommandHandler(
            IRepository<Initiative> repository,
            IUnitOfWork unitOfWork,
            IImageService imageService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<InitiativeDto> Handle(
            UpdateInitiativeCommand request,
            CancellationToken cancellationToken)
        {
            var initiative = await _repository.GetAll()
                .Where(i => i.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (initiative == null)
                throw new NotFoundException("المبادرة", request.Id);

            initiative.Name = request.Dto.Name;
            initiative.Description = request.Dto.Description;
            initiative.StartDate = request.Dto.StartDate;
            initiative.EndDate = request.Dto.EndDate;
            initiative.Progress = request.Dto.Progress;
            initiative.IsAISuggested = request.Dto.IsAISuggested;
            initiative.StatusId = request.Dto.StatusId;
            initiative.AssignedToId = request.Dto.AssignedTo;
            var style = WorkItemStyleDefaults.ForInitiative(request.Dto.Name, request.Dto.Description, request.Dto.Color, request.Dto.Icon);
            initiative.Color = style.Color;
            initiative.Icon = style.Icon;

            if (request.Dto.Image != null && request.Dto.Image.Length > 0)
            {
                var newImageId = await _imageService.SaveImageAsync(
                    request.Dto.Image,
                    cancellationToken);

                initiative.ImageId = newImageId;
            }

            _repository.Update(initiative);
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
                ImageId = initiative.ImageId
            };
        }
    }
}

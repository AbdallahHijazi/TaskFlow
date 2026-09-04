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
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService currentUser;

        public CreateInitiativeCommandHandler(
            IRepository<Initiative> repository, 
            IUnitOfWork unitOfWork,
            IImageService imageService,
            TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            this.imageService = imageService;
            this.currentUser = currentUser;
        }

        public async Task<InitiativeDto> Handle(CreateInitiativeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var imageId = await imageService.SaveImageAsync(request.Dto.Image, cancellationToken);

                var style = WorkItemStyleDefaults.ForInitiative(request.Dto.Name, request.Dto.Description, request.Dto.Color, request.Dto.Icon);
                var initiative = new Initiative
                {
                    Name = request.Dto.Name.Trim(),
                    Description = request.Dto.Description?.Trim(),
                    StartDate = request.Dto.StartDate,
                    EndDate = request.Dto.EndDate,
                    // Initiative progress is derived from its tasks. A new initiative has no tasks.
                    Progress = 0,
                    IsAISuggested = request.Dto.IsAISuggested,
                    AssignedToId = currentUser.UserId
                        ?? throw new TaskFlow.Domain.Exceptions.UnauthorizedException("Your session does not contain a user. Please sign in again."),
                    Color = style.Color,
                    Icon = style.Icon,
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

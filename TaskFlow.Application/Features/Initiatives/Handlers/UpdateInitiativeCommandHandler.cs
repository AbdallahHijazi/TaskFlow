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

namespace TaskFlow.Application.Features.Initiatives.Handlers
{
    public class UpdateInitiativeCommandHandler : IRequestHandler<UpdateInitiativeCommand, InitiativeDto>
    {
        private readonly IRepository<Initiative> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateInitiativeCommandHandler(IRepository<Initiative> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<InitiativeDto> Handle(UpdateInitiativeCommand request, CancellationToken cancellationToken)
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
            initiative.ImageId = request.Dto.ImageId;

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
                ImageId = initiative.ImageId,
                CreatedBy = initiative.CreatedBy
            };
        }
    }
}

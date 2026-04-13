using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Initiative;
using TaskFlow.Application.Features.Initiatives.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Initiatives.Handlers
{
    public class CreateInitiativeCommandHandler : IRequestHandler<CreateInitiativeCommand, InitiativeDto>
    {
        private readonly IRepository<Initiative> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateInitiativeCommandHandler(IRepository<Initiative> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<InitiativeDto> Handle(CreateInitiativeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var initiative = new Initiative
                {
                    Name = request.Dto.Name.Trim(),
                    Description = request.Dto.Description?.Trim(),
                    StartDate = request.Dto.StartDate,
                    EndDate = request.Dto.EndDate,
                    Progress = request.Dto.Progress,
                    IsAISuggested = request.Dto.IsAISuggested,
                    ImageId = request.Dto.ImageId,
                    CreatedBy = request.Dto.CreatedBy
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
                    ImageId = initiative.ImageId,
                    CreatedBy = initiative.CreatedBy,
                    UpdatedAt = initiative.UpdatedAt,
                    UpdatedBy = initiative.UpdatedBy
                };
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء إنشاء المبادرة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Status;
using TaskFlow.Application.Features.Statuses.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Statuses.Handlers
{
    public class CreateStatusCommandHandler : IRequestHandler<CreateStatusCommand, StatusDto>
    {
        private readonly IRepository<Status> _statusRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateStatusCommandHandler(IRepository<Status> statusRepository, IUnitOfWork unitOfWork)
        {
            _statusRepository = statusRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StatusDto> Handle(CreateStatusCommand request, CancellationToken cancellationToken)
        {
            var status = new Status
            {
                Name = request.Dto.Name,
                Description = request.Dto.Description,
                Color = request.Dto.Color
            };

            _statusRepository.Add(status);
            await _unitOfWork.SaveChangesAsync();

            return new StatusDto
            {
                Id = status.Id,
                Name = status.Name,
                Description = status.Description,
                Color = status.Color
            };
        }
    }
}

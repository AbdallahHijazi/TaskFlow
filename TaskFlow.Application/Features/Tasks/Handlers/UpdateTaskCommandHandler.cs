using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
    {
        private readonly IRepository<TaskItem> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTaskCommandHandler(IRepository<TaskItem> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _repository.GetAll()
                .Where(t => t.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
                throw new NotFoundException("المهمة", request.Id);

            task.Name = request.Dto.Name.Trim();
            task.Description = request.Dto.Description?.Trim();
            task.StartDate = request.Dto.StartDate;
            task.EndDate = request.Dto.EndDate;
            task.Progress = request.Dto.Progress;
            task.Priority = request.Dto.Priority;
            task.StatusId = request.Dto.StatusId;
            task.InitiativeId = request.Dto.InitiativeId;
            task.AssignedToId = request.Dto.AssignedToId;
            task.CreatedBy = request.Dto.CreatedById;
            task.ImageId = request.Dto.ImageId;
            task.UpdatedAt = DateTime.UtcNow;
            task.UpdatedBy = request.Dto.UpdatedById;

            _repository.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TaskDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                Progress = task.Progress,
                Priority = task.Priority,
                StatusId = task.StatusId,
                InitiativeId = task.InitiativeId,
                AssignedToId = task.AssignedToId,
                CreatedById = task.CreatedBy ?? Guid.Empty,
                ImageId = task.ImageId,
                UpdatedAt = task.UpdatedAt,
                UpdatedById = task.UpdatedBy
            };
        }
    }
}

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Handlers
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
    {
        private readonly IRepository<TaskItem> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTaskCommandHandler(IRepository<TaskItem> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var task = new TaskItem
                {
                    Name = request.Dto.Name.Trim(),
                    Description = request.Dto.Description?.Trim(),
                    StartDate = request.Dto.StartDate,
                    EndDate = request.Dto.EndDate,
                    Progress = request.Dto.Progress,
                    StatusId = request.Dto.StatusId,
                    InitiativeId = request.Dto.InitiativeId,
                    AssignedToId = request.Dto.AssignedToId,
                    CreatedBy = request.Dto.CreatedById,
                    ImageId = request.Dto.ImageId
                };

                _repository.Add(task);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new TaskDto
                {
                    Id = task.Id,
                    Name = task.Name,
                    Description = task.Description,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    Progress = task.Progress,
                    StatusId = task.StatusId,
                    InitiativeId = task.InitiativeId,
                    AssignedToId = task.AssignedToId,
                    CreatedById = task.CreatedBy ?? Guid.Empty,
                    ImageId = task.ImageId,
                    UpdatedAt = task.UpdatedAt,
                    UpdatedById = task.UpdatedBy
                };
            }
            catch (Exception)
            {
                throw new InvalidOperationException("حدث خطأ أثناء إنشاء المهمة. يرجى المحاولة مرة أخرى.");
            }
        }
    }
}

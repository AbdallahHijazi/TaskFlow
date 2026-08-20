using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Application.Common.Services;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Handlers
{
    public sealed class SaveGeneratedInitiativeCommandHandler
        : IRequestHandler<
            SaveGeneratedInitiativeCommand,
            SaveGeneratedInitiativeResponse>
    {
        private readonly IRepository<Initiative> _initiativeRepository;
        private readonly IRepository<TaskItem> _taskRepository;
        private readonly IRepository<Status> _statusRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SaveGeneratedInitiativeCommandHandler(
            IRepository<Initiative> initiativeRepository,
            IRepository<TaskItem> taskRepository,
            IRepository<Status> statusRepository,
            IRepository<User> userRepository,
            IUnitOfWork unitOfWork)
        {
            _initiativeRepository = initiativeRepository;
            _taskRepository = taskRepository;
            _statusRepository = statusRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<SaveGeneratedInitiativeResponse> Handle(
            SaveGeneratedInitiativeCommand command,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Request);

            var request = command.Request;

            var statusExists =
                _statusRepository.GetAll()
                    .Any(status => status.Id == request.StatusId);

            if (!statusExists)
            {
                throw new NotFoundException(
                    "الحالة",
                    request.StatusId);
            }

            var assignedUserExists =
                _userRepository.GetAll()
                    .Any(user => user.Id == request.AssignedToId);

            if (!assignedUserExists)
            {
                throw new NotFoundException(
                    "المستخدم المسؤول",
                    request.AssignedToId);
            }

            var initiativeStyle = WorkItemStyleDefaults.ForInitiative(request.Name, request.Description, request.Color, request.Icon);
            var initiative = new Initiative
            {
                Id = Guid.NewGuid(),

                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),

                StartDate = request.StartDate,
                EndDate = request.EndDate,

                Progress = 0,
                IsAISuggested = true,
                IsActive = true,

                Color = initiativeStyle.Color,
                Icon = initiativeStyle.Icon,

                StatusId = request.StatusId,
                AssignedToId = request.AssignedToId
            };

            _initiativeRepository.Add(initiative);

            foreach (var generatedTask in request.Tasks)
            {
                var taskStyle = WorkItemStyleDefaults.ForTask(generatedTask.Name, generatedTask.Description, generatedTask.Color, generatedTask.Icon);
                var task = new TaskItem
                {
                    Id = Guid.NewGuid(),

                    Name = generatedTask.Name.Trim(),
                    Description = generatedTask.Description?.Trim(),

                    StartDate = generatedTask.StartDate,
                    EndDate = generatedTask.EndDate,

                    Progress = 0,
                    IsAISuggested = true,
                    IsActive = true,

                    Color = taskStyle.Color,
                    Icon = taskStyle.Icon,

                    InitiativeId = initiative.Id,

                    StatusId = request.StatusId,
                    AssignedToId = request.AssignedToId,

                    ParentId = null
                };

                _taskRepository.Add(task);
            }

            var affectedRows =
                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

            if (affectedRows <= 0)
            {
                throw new BadRequestException(
                    "لم يتم حفظ المبادرة والمهام.");
            }

            return new SaveGeneratedInitiativeResponse
            {
                InitiativeId = initiative.Id,
                CreatedTasksCount = request.Tasks.Count,
                Message = "تم إنشاء المبادرة والمهام بنجاح."
            };
        }
    }
}

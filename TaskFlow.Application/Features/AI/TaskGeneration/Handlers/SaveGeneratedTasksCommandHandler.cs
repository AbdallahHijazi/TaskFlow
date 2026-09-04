using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.AI.TaskGeneration;
using TaskFlow.Application.Features.AI.TaskGeneration.Commands;
using TaskFlow.Application.Features.AI.TaskGeneration.Validators;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Application.Common.Services;

namespace TaskFlow.Application.Features.AI.TaskGeneration.Handlers
{
    public sealed class SaveGeneratedTasksCommandHandler
        : IRequestHandler<
            SaveGeneratedTasksCommand,
            SaveGeneratedTasksResponse>
    {
        private readonly IRepository<Initiative> _initiativeRepository;
        private readonly IRepository<TaskItem> _taskRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Status> _statusRepository;
        private readonly TaskFlow.Domain.Interfaces.ICurrentUserService _currentUser;

        public SaveGeneratedTasksCommandHandler(
            IRepository<Initiative> initiativeRepository,
            IRepository<TaskItem> taskRepository,
            IRepository<Status> statusRepository,
            IUnitOfWork unitOfWork,
            TaskFlow.Domain.Interfaces.ICurrentUserService currentUser)
        {
            _initiativeRepository = initiativeRepository;
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
            _statusRepository = statusRepository;
            _currentUser = currentUser;
        }

        public async Task<SaveGeneratedTasksResponse> Handle(
            SaveGeneratedTasksCommand command,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Request);

            var request = command.Request;

            var newStatus = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(
                    _statusRepository.GetAll().Where(status => status.Name != null && status.Name.ToLower() == "new"),
                    cancellationToken);

            if (newStatus is null)
            {
                throw new InvalidOperationException("The default 'New' status is not configured for this workspace.");
            }

            var currentUserId = _currentUser.UserId
                ?? throw new UnauthorizedException("Your session does not contain a user. Please sign in again.");

            var initiative =
                _initiativeRepository.Get(request.InitiativeId);

            if (initiative is null)
            {
                throw new NotFoundException(
                    "المبادرة",
                    request.InitiativeId);
            }

            if (initiative.StartDate is null)
            {
                throw new InvalidOperationException(
                    "تاريخ بداية المبادرة غير موجود.");
            }

            if (initiative.EndDate is null)
            {
                throw new InvalidOperationException(
                    "تاريخ نهاية المبادرة غير موجود.");
            }

            var existingTaskNames =
                _taskRepository.GetAll()
                    .Where(task =>
                        task.InitiativeId == initiative.Id)
                    .Select(task => task.Name)
                    .Where(name =>
                        !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!)
                    .ToList();

            var preview = new GeneratedTasksPreview
            {
                InitiativeId = initiative.Id,
                InitiativeName =
                    initiative.Name ?? string.Empty,
                Tasks = request.Tasks
            };

            var validationErrors =
                GeneratedTasksPreviewValidator.Validate(
                    preview,
                    initiative.StartDate.Value,
                    initiative.EndDate,
                    existingTaskNames);

            if (validationErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "تعذر حفظ المهام: "
                    +
                    string.Join(
                        " | ",
                        validationErrors));
            }

            foreach (var generatedTask in request.Tasks)
            {
                var taskStyle = WorkItemStyleDefaults.ForTask(generatedTask.Name, generatedTask.Description, generatedTask.Color, generatedTask.Icon);
                var task = new TaskItem
                {
                    Name = generatedTask.Name.Trim(),
                    Description =
                        generatedTask.Description?.Trim(),

                    StartDate = generatedTask.StartDate,
                    EndDate = generatedTask.EndDate,

                    Progress = 0,

                    InitiativeId = initiative.Id,
                    StatusId = newStatus.Id,
                    AssignedToId = currentUserId,

                    Color = taskStyle.Color,
                    Icon = taskStyle.Icon,

                    IsAISuggested = true,
                    IsActive = true
                };

                _taskRepository.Add(task);
            }

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return new SaveGeneratedTasksResponse
            {
                InitiativeId = initiative.Id,
                CreatedTasksCount = request.Tasks.Count,
                Message = "تم حفظ المهام المقترحة بنجاح."
            };
        }
    }
}

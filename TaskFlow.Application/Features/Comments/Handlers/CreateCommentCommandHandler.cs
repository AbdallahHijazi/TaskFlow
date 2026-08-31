using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Comment;
using TaskFlow.Application.Features.Comments.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Comments.Handlers;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IRepository<Comment> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IWorkEventService _workEvents;

    public CreateCommentCommandHandler(IRepository<Comment> repository, IUnitOfWork unitOfWork,
        IRepository<TaskItem> taskRepository, IWorkEventService workEvents)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _taskRepository = taskRepository;
        _workEvents = workEvents;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Dto.Content))
                throw new BadRequestException("محتوى التعليق مطلوب");

            var comment = new Comment
            {
                Content = request.Dto.Content.Trim(),
                UserId = request.Dto.UserId,
                TaskId = request.Dto.TaskId,
                CreatedAt = DateTime.UtcNow
            };

            _repository.Add(comment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var task = await _taskRepository.GetAll().Where(item => item.Id == comment.TaskId)
                .Select(item => new { item.Id, item.Name, item.AssignedToId }).FirstOrDefaultAsync(cancellationToken);
            if (task != null)
                await _workEvents.RecordAsync(task.AssignedToId, task.Id, "comment_added", "New task comment",
                    $"A comment was added to {task.Name}: {comment.Content}", null, comment.Content, true, cancellationToken);

            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content ?? string.Empty,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                TaskId = comment.TaskId
            };
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new InvalidOperationException("حدث خطأ أثناء إنشاء التعليق");
        }
    }
}

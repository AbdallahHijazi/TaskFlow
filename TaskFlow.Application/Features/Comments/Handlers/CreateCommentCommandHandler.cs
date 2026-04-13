using MediatR;
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

    public CreateCommentCommandHandler(IRepository<Comment> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
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

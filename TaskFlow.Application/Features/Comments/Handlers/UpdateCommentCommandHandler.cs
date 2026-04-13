using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Comment;
using TaskFlow.Application.Features.Comments.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Comments.Handlers;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly IRepository<Comment> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommentCommandHandler(IRepository<Comment> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetAll().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (comment == null)
            throw new NotFoundException("التعليق", request.Id);

        if (string.IsNullOrWhiteSpace(request.Dto.Content))
            throw new BadRequestException("محتوى التعليق مطلوب");

        comment.Content = request.Dto.Content.Trim();
        comment.UserId = request.Dto.UserId;
        comment.TaskId = request.Dto.TaskId;

        _repository.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UserId = comment.UserId,
            TaskId = comment.TaskId
        };
    }
}

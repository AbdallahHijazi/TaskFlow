using MediatR;
using TaskFlow.Application.DTOs.Comment;

namespace TaskFlow.Application.Features.Comments.Commands;

public class CreateCommentCommand : IRequest<CommentDto>
{
    public CreateCommentDto Dto { get; set; }

    public CreateCommentCommand(CreateCommentDto dto)
    {
        Dto = dto;
    }
}

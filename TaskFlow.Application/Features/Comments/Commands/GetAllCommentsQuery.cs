using MediatR;
using TaskFlow.Application.DTOs.Comment;

namespace TaskFlow.Application.Features.Comments.Commands;

public class GetAllCommentsQuery : IRequest<List<CommentDto>>
{
}

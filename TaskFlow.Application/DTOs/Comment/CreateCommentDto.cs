namespace TaskFlow.Application.DTOs.Comment;

public class CreateCommentDto
{
    public string? Content { get; set; }
    public Guid? UserId { get; set; }
    public Guid? TaskId { get; set; }
}

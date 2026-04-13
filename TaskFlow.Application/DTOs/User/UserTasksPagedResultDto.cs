using TaskFlow.Application.DTOs.Task;

namespace TaskFlow.Application.DTOs.User;

public class UserTasksPagedResultDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<TaskDto> Items { get; set; } = [];
}

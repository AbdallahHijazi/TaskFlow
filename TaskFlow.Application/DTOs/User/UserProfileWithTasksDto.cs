namespace TaskFlow.Application.DTOs.User;

public class UserProfileWithTasksDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int TotalTasksCount { get; set; }
    public List<UserTaskSummaryDto> Tasks { get; set; } = [];
}

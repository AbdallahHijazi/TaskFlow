namespace TaskFlow.Application.DTOs.User;

public class UserTaskSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? StatusId { get; set; }
    public Guid? InitiativeId { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Progress { get; set; }
}

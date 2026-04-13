namespace TaskFlow.Application.DTOs.User;

public class UserTasksQueryParametersDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? Status { get; set; }
    public Guid? InitiativeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDirection { get; set; } = "desc";
}

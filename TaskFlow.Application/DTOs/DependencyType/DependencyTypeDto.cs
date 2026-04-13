namespace TaskFlow.Application.DTOs.DependencyType;

public class DependencyTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

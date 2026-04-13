namespace TaskFlow.Application.DTOs.TaskDependency;

public class TaskDependencyDto
{
    public Guid Id { get; set; }
    public Guid? DependencyTypeId { get; set; }
    public Guid? PredecessorId { get; set; }
    public Guid? SuccessorId { get; set; }
}

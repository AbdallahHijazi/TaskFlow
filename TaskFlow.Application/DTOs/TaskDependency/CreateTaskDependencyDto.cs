namespace TaskFlow.Application.DTOs.TaskDependency;

public class CreateTaskDependencyDto
{
    public Guid? DependencyTypeId { get; set; }
    public Guid? PredecessorId { get; set; }
    public Guid? SuccessorId { get; set; }
}

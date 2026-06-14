namespace TaskFlow.Application.DTOs.Task
{
    public class TaskBoardColumnDto
    {
        public Guid? StatusId { get; set; }
        public string StatusName { get; set; } = "Unknown Status";
        public string? Color { get; set; }
        public int TaskCount { get; set; }
        public List<TaskDto> Tasks { get; set; } = new();
    }
}

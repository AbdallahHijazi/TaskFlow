using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Client : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Status> Statuses { get; set; } = new List<Status>();
    public ICollection<DependencyType> DependencyTypes { get; set; } = new List<DependencyType>();
    public ICollection<Initiative> Initiatives { get; set; } = new List<Initiative>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TaskDependency> TaskDependencies { get; set; } = new List<TaskDependency>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
}

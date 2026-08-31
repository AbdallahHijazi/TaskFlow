using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class ActivityLog : BaseEntity, ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid? ActorUserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public TaskItem? Task { get; set; }
    public User? ActorUser { get; set; }
}

using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Notification : BaseEntity, ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public Guid RecipientUserId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid? InitiativeId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public User? RecipientUser { get; set; }
    public TaskItem? Task { get; set; }
    public Initiative? Initiative { get; set; }
}

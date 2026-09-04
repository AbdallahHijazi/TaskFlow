namespace TaskFlow.Application.Common.Interfaces;

public record RealtimeNotification(Guid Id, Guid? TaskId, Guid? InitiativeId, string Type, string Title, string Message,
    bool IsRead, DateTime CreatedAt);

public interface INotificationRealtimePublisher
{
    Task PublishAsync(Guid recipientUserId, RealtimeNotification notification,
        CancellationToken cancellationToken = default);
}

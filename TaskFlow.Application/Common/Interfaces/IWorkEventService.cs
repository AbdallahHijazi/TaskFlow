namespace TaskFlow.Application.Common.Interfaces;

public interface IWorkEventService
{
    Task RecordAsync(Guid? recipientUserId, Guid? taskId, string type, string title, string message,
        string? oldValue = null, string? newValue = null, bool sendEmail = true,
        CancellationToken cancellationToken = default);
}

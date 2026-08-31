using Microsoft.AspNetCore.SignalR;
using TaskFlow.API.Hubs;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.API.Services;

public sealed class SignalRNotificationPublisher(IHubContext<NotificationsHub> hub)
    : INotificationRealtimePublisher
{
    public Task PublishAsync(Guid recipientUserId, RealtimeNotification notification,
        CancellationToken cancellationToken = default) =>
        hub.Clients.User(recipientUserId.ToString()).SendAsync("notificationReceived", notification, cancellationToken);
}

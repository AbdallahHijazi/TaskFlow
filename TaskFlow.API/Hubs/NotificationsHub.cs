using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskFlow.API.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub;

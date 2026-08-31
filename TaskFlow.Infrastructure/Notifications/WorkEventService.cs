using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Notifications;

public class WorkEventService : IWorkEventService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly SmtpOptions _options;
    private readonly ILogger<WorkEventService> _logger;

    public WorkEventService(AppDbContext db, ICurrentUserService currentUser, IOptions<SmtpOptions> options,
        ILogger<WorkEventService> logger)
    { _db = db; _currentUser = currentUser; _options = options.Value; _logger = logger; }

    public async Task RecordAsync(Guid? recipientUserId, Guid? taskId, string type, string title, string message,
        string? oldValue = null, string? newValue = null, bool sendEmail = true, CancellationToken cancellationToken = default)
    {
        _db.ActivityLogs.Add(new ActivityLog { TaskId = taskId, ActorUserId = _currentUser.UserId,
            Type = type, Description = message, OldValue = oldValue, NewValue = newValue });
        User? recipient = null;
        if (recipientUserId.HasValue && recipientUserId != _currentUser.UserId)
        {
            recipient = await _db.Users.FirstOrDefaultAsync(user => user.Id == recipientUserId.Value && user.ClientId == _currentUser.ClientId, cancellationToken);
            if (recipient != null) _db.Notifications.Add(new Notification { RecipientUserId = recipient.Id, TaskId = taskId,
                Type = type, Title = title, Message = message });
        }
        await _db.SaveChangesAsync(cancellationToken);
        if (!sendEmail || !_options.Enabled || recipient?.Email == null) return;
        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            { EnableSsl = _options.UseSsl, Credentials = new NetworkCredential(_options.Username, _options.Password) };
            using var mail = new MailMessage { From = new MailAddress(_options.Username, _options.FromName), Subject = title,
                Body = $"{message}\n\nOpen ORQIST: {_options.AppUrl}/tasks", IsBodyHtml = false };
            mail.To.Add(recipient.Email);
            await client.SendMailAsync(mail, cancellationToken);
        }
        catch (Exception exception) { _logger.LogWarning(exception, "Notification email could not be sent to user {UserId}", recipient.Id); }
    }
}

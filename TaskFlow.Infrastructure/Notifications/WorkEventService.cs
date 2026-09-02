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
    private readonly INotificationRealtimePublisher _realtime;

    public WorkEventService(AppDbContext db, ICurrentUserService currentUser, IOptions<SmtpOptions> options,
        ILogger<WorkEventService> logger, INotificationRealtimePublisher realtime)
    { _db = db; _currentUser = currentUser; _options = options.Value; _logger = logger; _realtime = realtime; }

    public async Task RecordAsync(Guid? recipientUserId, Guid? taskId, string type, string title, string message,
        string? oldValue = null, string? newValue = null, bool sendEmail = true, CancellationToken cancellationToken = default)
    {
        _db.ActivityLogs.Add(new ActivityLog { TaskId = taskId, ActorUserId = _currentUser.UserId,
            Type = type, Description = message, OldValue = oldValue, NewValue = newValue });
        User? recipient = null;
        Notification? notification = null;
        if (recipientUserId.HasValue && recipientUserId != _currentUser.UserId)
        {
            recipient = await _db.Users.FirstOrDefaultAsync(user => user.Id == recipientUserId.Value && user.ClientId == _currentUser.ClientId, cancellationToken);
            if (recipient != null) { notification = new Notification { RecipientUserId = recipient.Id, TaskId = taskId,
                Type = type, Title = title, Message = message }; _db.Notifications.Add(notification); }
        }
        await _db.SaveChangesAsync(cancellationToken);
        if (recipient != null && notification != null)
        {
            try
            {
                await _realtime.PublishAsync(recipient.Id,
                    new RealtimeNotification(notification.Id, notification.TaskId, notification.InitiativeId,
                        notification.Type, notification.Title, notification.Message, notification.IsRead, notification.CreatedAt), cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Realtime notification could not be delivered to user {UserId}", recipient.Id);
            }
        }
        if (!sendEmail || !_options.Enabled || recipient?.Email == null) return;
        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            { EnableSsl = _options.UseSsl, Credentials = new NetworkCredential(_options.Username, _options.Password) };
            var targetUrl = $"{_options.AppUrl.TrimEnd('/')}/tasks/{taskId}";
            using var mail = new MailMessage { From = new MailAddress(_options.Username, _options.FromName), Subject = $"[ORQIST] {title}",
                Body = BuildWorkNotificationEmail(recipient.Name, title, message, targetUrl, type, "View task"), IsBodyHtml = true };
            mail.To.Add(recipient.Email);
            await client.SendMailAsync(mail, cancellationToken);
        }
        catch (Exception exception) { _logger.LogWarning(exception, "Notification email could not be sent to user {UserId}", recipient.Id); }
    }

    public async Task RecordInitiativeAsync(Guid? recipientUserId, Guid initiativeId, string type, string title,
        string message, string? oldValue = null, string? newValue = null, bool sendEmail = true,
        CancellationToken cancellationToken = default)
    {
        User? recipient = null;
        Notification? notification = null;
        if (recipientUserId.HasValue && recipientUserId != _currentUser.UserId)
        {
            recipient = await _db.Users.FirstOrDefaultAsync(user => user.Id == recipientUserId.Value && user.ClientId == _currentUser.ClientId, cancellationToken);
            if (recipient != null)
            {
                notification = new Notification { RecipientUserId = recipient.Id, InitiativeId = initiativeId,
                    Type = type, Title = title, Message = message };
                _db.Notifications.Add(notification);
            }
        }
        await _db.SaveChangesAsync(cancellationToken);
        if (recipient != null && notification != null)
        {
            try
            {
                await _realtime.PublishAsync(recipient.Id,
                    new RealtimeNotification(notification.Id, null, notification.InitiativeId, notification.Type,
                        notification.Title, notification.Message, notification.IsRead, notification.CreatedAt), cancellationToken);
            }
            catch (Exception exception) { _logger.LogWarning(exception, "Realtime initiative notification could not be delivered to user {UserId}", recipient.Id); }
        }
        if (!sendEmail || !_options.Enabled || recipient?.Email == null) return;
        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            { EnableSsl = _options.UseSsl, Credentials = new NetworkCredential(_options.Username, _options.Password) };
            var targetUrl = $"{_options.AppUrl.TrimEnd('/')}/initiatives/{initiativeId}";
            using var mail = new MailMessage { From = new MailAddress(_options.Username, _options.FromName), Subject = $"[ORQIST] {title}",
                Body = BuildWorkNotificationEmail(recipient.Name, title, message, targetUrl, type, "View initiative"), IsBodyHtml = true };
            mail.To.Add(recipient.Email);
            await client.SendMailAsync(mail, cancellationToken);
        }
        catch (Exception exception) { _logger.LogWarning(exception, "Initiative notification email could not be sent to user {UserId}", recipient.Id); }
    }

    private static string BuildWorkNotificationEmail(string? recipientName, string title, string message,
        string targetUrl, string eventType, string actionLabel)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName);
        var safeTitle = WebUtility.HtmlEncode(title);
        var safeMessage = WebUtility.HtmlEncode(message);
        var safeUrl = WebUtility.HtmlEncode(targetUrl);
        var safeAction = WebUtility.HtmlEncode(actionLabel);
        var isAssignment = eventType.Contains("assigned", StringComparison.OrdinalIgnoreCase);
        var accent = isAssignment ? "#2563eb" : "#6d4ce8";
        var badge = isAssignment ? "NEW ASSIGNMENT" : "WORK UPDATE";
        var icon = isAssignment ? "&#10003;" : "&#8596;";

        return $$"""
        <!doctype html>
        <html><body style="margin:0;padding:0;background:#f4f6fb;font-family:Arial,Helvetica,sans-serif;color:#172033">
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f4f6fb;padding:32px 14px"><tr><td align="center">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:600px;overflow:hidden;border:1px solid #e6e9f2;border-radius:22px;background:#ffffff;box-shadow:0 14px 36px rgba(30,41,59,.08)">
            <tr><td style="padding:26px 32px;background:linear-gradient(135deg,#332377 0%,#6d4ce8 58%,#4f7de8 100%);color:#fff">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0"><tr>
                <td><div style="font-size:25px;font-weight:800;letter-spacing:.09em">ORQIST</div><div style="margin-top:4px;font-size:10px;letter-spacing:.16em;opacity:.78">WORK EXECUTION PLATFORM</div></td>
                <td align="right"><span style="display:inline-block;padding:7px 10px;border:1px solid rgba(255,255,255,.28);border-radius:999px;background:rgba(255,255,255,.12);font-size:9px;font-weight:700;letter-spacing:.1em">{{badge}}</span></td>
              </tr></table>
            </td></tr>
            <tr><td style="padding:34px 32px 30px">
              <div style="display:inline-block;width:48px;height:48px;border-radius:15px;background:{{accent}};color:#fff;text-align:center;font-size:24px;font-weight:700;line-height:48px">{{icon}}</div>
              <p style="margin:22px 0 7px;color:#64748b;font-size:13px">Hello {{safeName}},</p>
              <h1 style="margin:0;color:#172033;font-size:24px;line-height:1.25">{{safeTitle}}</h1>
              <div style="margin:20px 0 24px;padding:17px 18px;border:1px solid #e5e7f2;border-left:4px solid {{accent}};border-radius:12px;background:#f8f9fd;color:#475569;font-size:14px;line-height:1.65">{{safeMessage}}</div>
              <a href="{{safeUrl}}" style="display:inline-block;padding:13px 21px;border-radius:11px;background:{{accent}};color:#fff;text-decoration:none;font-size:13px;font-weight:700">{{safeAction}} &nbsp;&#8594;</a>
              <p style="margin:24px 0 0;color:#94a3b8;font-size:11px;line-height:1.6">This notification was sent because this work item is assigned to you. Open ORQIST to review the latest details.</p>
            </td></tr>
            <tr><td style="padding:17px 32px;border-top:1px solid #edf0f5;background:#fafbfc;color:#94a3b8;font-size:10px">
              <table role="presentation" width="100%"><tr><td>ORQIST Notifications</td><td align="right">Plan clearly. Execute confidently.</td></tr></table>
            </td></tr>
          </table>
        </td></tr></table>
        </body></html>
        """;
    }
}

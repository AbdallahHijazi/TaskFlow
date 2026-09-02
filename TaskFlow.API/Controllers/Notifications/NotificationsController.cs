using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.API.Controllers.Notifications;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    public NotificationsController(AppDbContext db, ICurrentUserService currentUser) { _db = db; _currentUser = currentUser; }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        var items = await _db.Notifications.AsNoTracking()
            .Where(item => item.RecipientUserId == _currentUser.UserId.Value)
            .OrderByDescending(item => item.CreatedAt).Take(50)
            .Select(item => new { item.Id, item.TaskId, item.InitiativeId, item.Type, item.Title, item.Message, item.IsRead, item.CreatedAt })
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        return Ok(new { count = await _db.Notifications.CountAsync(item => item.RecipientUserId == _currentUser.UserId.Value && !item.IsRead, ct) });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var item = await _db.Notifications.FirstOrDefaultAsync(value => value.Id == id && value.RecipientUserId == _currentUser.UserId, ct);
        if (item == null) return NotFound();
        item.IsRead = true; item.ReadAt = DateTime.UtcNow; await _db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        var items = await _db.Notifications.Where(item => item.RecipientUserId == _currentUser.UserId.Value && !item.IsRead).ToListAsync(ct);
        foreach (var item in items) { item.IsRead = true; item.ReadAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("activity/task/{taskId:guid}")]
    public async Task<IActionResult> TaskActivity(Guid taskId, CancellationToken ct)
    {
        var allowed = await _db.Tasks.AnyAsync(task => task.Id == taskId && (_currentUser.IsAdmin || task.AssignedToId == _currentUser.UserId), ct);
        if (!allowed) return NotFound();
        var items = await _db.ActivityLogs.AsNoTracking().Where(item => item.TaskId == taskId)
            .OrderByDescending(item => item.CreatedAt).Take(100)
            .Select(item => new { item.Id, item.Type, item.Description, item.OldValue, item.NewValue, item.CreatedAt,
                ActorName = item.ActorUser == null ? "System" : item.ActorUser.Name })
            .ToListAsync(ct);
        return Ok(items);
    }
}

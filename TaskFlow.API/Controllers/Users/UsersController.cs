using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskFlow.API.Infrastructure;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Users.Commands;

namespace TaskFlow.API.Controllers.Users;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private static readonly HashSet<string> ElevatedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Manager"
    };

    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] CreateUserDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات المستخدم مطلوبة" });

        var result = await _mediator.Send(new CreateUserCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await _mediator.Send(new GetUserByIdQuery(userId));
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllUsersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));
        return Ok(result);
    }


    [HttpGet("{id}/tasks")]
    [ProducesResponseType(typeof(UserTasksPagedResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserTasks(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? status = null,
        [FromQuery] Guid? initiativeId = null,
        [FromQuery] int? priority = null,
        [FromQuery] bool? isOverdue = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDirection = "desc")
    {
        if (!CanAccessUserData(id))
            return Forbid();

        var parameters = new UserTasksQueryParametersDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            InitiativeId = initiativeId,
            Priority = priority,
            IsOverdue = isOverdue,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var result = await _mediator.Send(new GetUserTasksQuery(id, parameters));
        return Ok(result);
    }

    [HttpGet("{id}/profile-with-tasks")]
    [ProducesResponseType(typeof(UserProfileWithTasksDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfileWithTasks(Guid id)
    {
        if (!CanAccessUserData(id))
            return Forbid();

        var result = await _mediator.Send(new GetUserProfileWithTasksQuery(id));
        return Ok(result);
    }

    private bool CanAccessUserData(Guid requestedUserId)
    {
        if (IsElevatedRole())
            return true;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var currentUserId) && currentUserId == requestedUserId;
    }

    private bool IsElevatedRole()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        return !string.IsNullOrWhiteSpace(role) && ElevatedRoles.Contains(role);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateUserDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

        var result = await _mediator.Send(new UpdateUserCommand(id, dto));
        return Ok(new { Message = "تم تحديث المستخدم بنجاح", Data = result });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}

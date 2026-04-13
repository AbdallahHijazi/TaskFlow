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
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات المستخدم مطلوبة" });

        var result = await _mediator.Send(new CreateUserCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllUsersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves paged tasks assigned to a user with optional filtering and sorting.
    /// </summary>
    /// <param name="id">Target user identifier.</param>
    /// <param name="pageNumber">Page number (minimum 1).</param>
    /// <param name="pageSize">Page size between 1 and 100.</param>
    /// <param name="status">Optional status identifier filter.</param>
    /// <param name="initiativeId">Optional initiative identifier filter.</param>
    /// <param name="priority">Optional priority filter (1-5).</param>
    /// <param name="isOverdue">Optional overdue filter based on due date.</param>
    /// <param name="fromDate">Optional start date lower bound.</param>
    /// <param name="toDate">Optional due date upper bound.</param>
    /// <param name="search">Optional search term for task name, description, and initiative name.</param>
    /// <param name="sortBy">Sorting field: createdAt, dueDate, priority.</param>
    /// <param name="sortDirection">Sorting direction: asc or desc.</param>
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

        var result = await _mediator.Send(new UpdateUserCommand(id, dto));
        return Ok(new { Message = "تم تحديث المستخدم بنجاح", Data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}

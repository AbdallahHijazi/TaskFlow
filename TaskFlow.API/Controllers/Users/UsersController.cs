using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Users.Commands;

namespace TaskFlow.API.Controllers.Users;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
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

    [HttpGet("{id}/tasks")]
    [ProducesResponseType(typeof(UserTasksPagedResultDto), StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfileWithTasks(Guid id)
    {
        var result = await _mediator.Send(new GetUserProfileWithTasksQuery(id));
        return Ok(result);
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

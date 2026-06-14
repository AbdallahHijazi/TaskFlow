using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;

namespace TaskFlow.API.Controllers.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات المهمة مطلوبة" });

        var result = await _mediator.Send(new CreateTaskCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllTasksQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("board")]
    public async Task<IActionResult> GetBoard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTaskBoardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

        var result = await _mediator.Send(new UpdateTaskCommand(id, dto), cancellationToken);
        return Ok(new { Message = "تم تحديث المهمة بنجاح", Data = result });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusDto dto, CancellationToken cancellationToken)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات الحالة مطلوبة" });

        var result = await _mediator.Send(new UpdateTaskStatusCommand(id, dto), cancellationToken);
        return Ok(new { Message = "تم تحديث حالة المهمة بنجاح", Data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTaskCommand(id), cancellationToken);
        return NoContent();
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.TaskDependency;
using TaskFlow.Application.Features.TaskDependencies.Commands;

namespace TaskFlow.API.Controllers.TaskDependencies;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TaskDependenciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaskDependenciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDependencyDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات تبعية المهمة مطلوبة" });

        var result = await _mediator.Send(new CreateTaskDependencyCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllTaskDependenciesQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTaskDependencyByIdQuery(id));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateTaskDependencyDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

        var result = await _mediator.Send(new UpdateTaskDependencyCommand(id, dto));
        return Ok(new { Message = "تم تحديث تبعية المهمة بنجاح", Data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTaskDependencyCommand(id));
        return NoContent();
    }
}

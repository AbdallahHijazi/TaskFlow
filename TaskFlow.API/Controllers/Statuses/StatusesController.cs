using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Status;
using TaskFlow.Application.Features.Statuses.Commands;

namespace TaskFlow.API.Controllers.Statuses;

[Route("api/[controller]")]
[ApiController]
public class StatusesController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatusesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStatusDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات الحالة مطلوبة" });

        var result = await _mediator.Send(new CreateStatusCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllStatusesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetStatusByIdQuery(id));
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStatusDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

        var result = await _mediator.Send(new UpdateStatusCommand(id, dto));
        return Ok(new { Message = "تم تحديث الحالة بنجاح", Data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteStatusCommand(id));
        return NoContent();
    }
}

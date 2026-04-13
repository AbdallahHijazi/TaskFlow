using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Initiative;
using TaskFlow.Application.Features.Initiatives.Commands;

namespace TaskFlow.API.Controllers.Initiatives;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InitiativesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InitiativesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInitiativeDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات المبادرة مطلوبة" });

        var result = await _mediator.Send(new CreateInitiativeCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllInitiativesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetInitiativeByIdQuery(id));
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateInitiativeDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

        var result = await _mediator.Send(new UpdateInitiativeCommand(id, dto));
        return Ok(new { Message = "تم تحديث المبادرة بنجاح", Data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteInitiativeCommand(id));
        return NoContent();
    }
}

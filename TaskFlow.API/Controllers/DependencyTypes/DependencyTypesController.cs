using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.DependencyType;
using TaskFlow.Application.Features.DependencyTypes.Commands;

namespace TaskFlow.API.Controllers.DependencyTypes;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DependencyTypesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DependencyTypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDependencyTypeDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات نوع التبعية مطلوبة" });

        var result = await _mediator.Send(new CreateDependencyTypeCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllDependencyTypesQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDependencyTypeByIdQuery(id));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateDependencyTypeDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

        var result = await _mediator.Send(new UpdateDependencyTypeCommand(id, dto));
        return Ok(new { Message = "تم تحديث نوع التبعية بنجاح", Data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDependencyTypeCommand(id));
        return NoContent();
    }
}

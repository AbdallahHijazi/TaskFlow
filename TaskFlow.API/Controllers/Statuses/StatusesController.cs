using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Infrastructure;
using TaskFlow.Application.DTOs.Status;
using TaskFlow.Application.Features.Statuses.Commands;

namespace TaskFlow.API.Controllers.Statuses
{
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

            try
            {
                var result = await _mediator.Send(new CreateStatusCommand(dto));
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _mediator.Send(new GetAllStatusesQuery());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetStatusByIdQuery(id));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStatusDto dto)
        {
            if (dto == null)
                return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

            try
            {
                var result = await _mediator.Send(new UpdateStatusCommand(id, dto));
                return Ok(new { Message = "تم تحديث الحالة بنجاح", Data = result });
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteStatusCommand(id));
                return NoContent();
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }
    }
}

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Status;
using TaskFlow.Application.Features.Statuses.Commands;
using TaskFlow.Domain.Exceptions;

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
                return BadRequest(new { ex.Message });
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
                return BadRequest(new { ex.Message });
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
            catch (NotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
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
            catch (NotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _mediator.Send(new DeleteStatusCommand(id));
                if (success)
                    return Ok(new { Message = "تم حذف الحالة بنجاح" });

                return BadRequest(new { Message = "فشل في حذف الحالة" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }
    }
}

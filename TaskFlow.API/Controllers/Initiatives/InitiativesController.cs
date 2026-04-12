using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Initiative;
using TaskFlow.Application.Features.Initiatives.Commands;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.API.Controllers.Initiatives
{
    [Route("api/[controller]")]
    [ApiController]
    public class InitiativesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InitiativesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateInitiativeDto dto)
        {
            if (dto == null)
                return BadRequest(new { Message = "بيانات المبادرة مطلوبة" });

            try
            {
                var result = await _mediator.Send(new CreateInitiativeCommand(dto));
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _mediator.Send(new GetAllInitiativesQuery());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetInitiativeByIdQuery(id));
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id,CreateInitiativeDto dto)
        {
            if (dto == null)
                return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

            try
            {
                var result = await _mediator.Send(new UpdateInitiativeCommand(id, dto));
                return Ok(new { Message = "تم تحديث المبادرة بنجاح", Data = result });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _mediator.Send(new DeleteInitiativeCommand(id));
                return Ok(new { Message = "تم حذف المبادرة بنجاح" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}

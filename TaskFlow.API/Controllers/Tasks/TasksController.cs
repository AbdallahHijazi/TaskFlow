using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Infrastructure;
using TaskFlow.Application.DTOs.Task;
using TaskFlow.Application.Features.Tasks.Commands;

namespace TaskFlow.API.Controllers.Tasks
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            if (dto == null)
                return BadRequest(new { Message = "بيانات المهمة مطلوبة" });

            try
            {
                var result = await _mediator.Send(new CreateTaskCommand(dto));
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
                var result = await _mediator.Send(new GetAllTasksQuery());
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
                var result = await _mediator.Send(new GetTaskByIdQuery(id));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateTaskDto dto)
        {
            if (dto == null)
                return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

            try
            {
                var result = await _mediator.Send(new UpdateTaskCommand(id, dto));
                return Ok(new { Message = "تم تحديث المهمة بنجاح", Data = result });
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
                await _mediator.Send(new DeleteTaskCommand(id));
                return NoContent();
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }
    }
}

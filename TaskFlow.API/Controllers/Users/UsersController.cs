using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Infrastructure;
using TaskFlow.Application.DTOs.User;
using TaskFlow.Application.Features.Users.Commands;

namespace TaskFlow.API.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (dto == null)
                return BadRequest(new { Message = "بيانات المستخدم مطلوبة" });

            try
            {
                var result = await _mediator.Send(new CreateUserCommand(dto));
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
                var result = await _mediator.Send(new GetAllUsersQuery());
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
                var result = await _mediator.Send(new GetUserByIdQuery(id));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            if (dto == null)
                return BadRequest(new { Message = "بيانات التحديث مطلوبة" });

            try
            {
                var result = await _mediator.Send(new UpdateUserCommand(id, dto));
                return Ok(new { Message = "تم تحديث المستخدم بنجاح", Data = result });
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
                await _mediator.Send(new DeleteUserCommand(id));
                return NoContent();
            }
            catch (Exception ex)
            {
                return ApiErrors.From(ex);
            }
        }
    }
}

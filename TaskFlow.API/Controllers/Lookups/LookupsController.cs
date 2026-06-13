using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Lookups.Commands;

namespace TaskFlow.API.Controllers.Lookups
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupsController : ControllerBase
    {
        private readonly IMediator middleware;

        public LookupsController(IMediator middleware)
        {
            this.middleware = middleware;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var lookups = await middleware.Send(new GetStatusLookupsQuery());
            return Ok(lookups);
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks()
        {
            var lookups = await middleware.Send(new GetTaskLookupsQuery());
            return Ok(lookups);

        }

        [HttpGet("initiatives")]
        public async Task<IActionResult> GetInitiatives()
        {
            var lookups = await middleware.Send(new GetInitiativeLookupsQuery());
            return Ok(lookups);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var lookups = await middleware.Send(new GetUserLookupsQuery());
            return Ok(lookups);
        }
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var lookups = await middleware.Send(new GetRoleLookupsQuery());
            return Ok(lookups);
        }

        [HttpGet("dependencyTypes")]
        public async Task<IActionResult> GetDependencyTypes()
        {
            var lookups = await middleware.Send(new GetDependencyTypeLookupsQuery());
            return Ok(lookups);
        }
    }
}

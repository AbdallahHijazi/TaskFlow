using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Lookup;
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

        [HttpGet("task-style-options")]
        public IActionResult GetTaskStyleOptions()
        {
            var colors = new List<KeyValueLookupDto>
            {
                new() { Key = "#2563EB", Value = "Blue" },
                new() { Key = "#16A34A", Value = "Green" },
                new() { Key = "#F59E0B", Value = "Amber" },
                new() { Key = "#DC2626", Value = "Red" },
                new() { Key = "#7C3AED", Value = "Violet" },
                new() { Key = "#0F766E", Value = "Teal" },
                new() { Key = "#475569", Value = "Slate" },
                new() { Key = "#DB2777", Value = "Pink" }
            };

            var icons = new List<KeyValueLookupDto>
            {
                new() { Key = "ti ti-checklist", Value = "Checklist" },
                new() { Key = "ti ti-clipboard-list", Value = "Clipboard List" },
                new() { Key = "ti ti-calendar-event", Value = "Calendar Event" },
                new() { Key = "ti ti-flag", Value = "Flag" },
                new() { Key = "ti ti-target-arrow", Value = "Target" },
                new() { Key = "ti ti-bolt", Value = "Bolt" },
                new() { Key = "ti ti-code", Value = "Code" },
                new() { Key = "ti ti-users", Value = "Users" }
            };

            return Ok(new { Colors = colors, Icons = icons });
        }

        [HttpGet("initiative-style-options")]
        public IActionResult GetInitiativeStyleOptions()
        {
            var colors = new List<KeyValueLookupDto>
            {
                new() { Key = "#2563EB", Value = "Blue" },
                new() { Key = "#059669", Value = "Emerald" },
                new() { Key = "#D97706", Value = "Amber" },
                new() { Key = "#7C3AED", Value = "Violet" },
                new() { Key = "#0891B2", Value = "Cyan" },
                new() { Key = "#BE123C", Value = "Rose" },
                new() { Key = "#DB2777", Value = "Pink" },
                new() { Key = "#475569", Value = "Slate" }
            };

            var icons = new List<KeyValueLookupDto>
            {
                new() { Key = "ti ti-rocket", Value = "Rocket" },
                new() { Key = "ti ti-bulb", Value = "Idea" },
                new() { Key = "ti ti-target-arrow", Value = "Target" },
                new() { Key = "ti ti-chart-line", Value = "Growth" },
                new() { Key = "ti ti-briefcase", Value = "Portfolio" },
                new() { Key = "ti ti-sparkles", Value = "Sparkles" },
                new() { Key = "ti ti-school", Value = "Education" },
                new() { Key = "ti ti-building", Value = "Building" }
            };

            return Ok(new { Colors = colors, Icons = icons });
        }
    }
}

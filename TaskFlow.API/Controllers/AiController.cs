using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.AI;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiChatService _aiChatService;

    public AiController(IAiChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<AiChatResponse>> Chat(
        [FromBody] AiChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var answer = await _aiChatService.SendMessageAsync(
            request.Message,
            cancellationToken);

        return Ok(new AiChatResponse(answer));
    }
}
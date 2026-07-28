using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.AI.Models;
using TaskFlow.Application.AI.Providers;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.AI;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;

namespace TaskFlow.API.Controllers;
[AllowAnonymous]
[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiChatService _aiChatService;
    private readonly ILLMProvider _llmProvider;
    private readonly IMediator _mediator;
    public AiController(IAiChatService aiChatService, ILLMProvider llmProvider, IMediator mediator)
    {
        _aiChatService = aiChatService;
        _llmProvider = llmProvider;
        _mediator = mediator;
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

    [HttpGet("provider_test")]
    public async Task<IActionResult> Test(
        CancellationToken cancellationToken)
    {
        var request = new LLMRequest
        {
            SystemPrompt =
                "You are a test assistant. Follow the user instruction exactly.",

            Prompt =
                "Return only the word OK without any additional text."
        };

        var response = await _llmProvider.ExecuteAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            succeeded = true,
            content = response.Content
        });
    }


    [HttpPost("generate-initiative")]
    public async Task<ActionResult<GeneratedInitiativePreview>> GenerateInitiative(
    [FromBody] GenerateInitiativeRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GenerateInitiativeCommand(request),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("save-generated-initiative")]
    public async Task<ActionResult<SaveGeneratedInitiativeResponse>>
    SaveGeneratedInitiative(
        [FromBody] SaveGeneratedInitiativeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SaveGeneratedInitiativeCommand(request),
            cancellationToken);

        return Ok(result);
    }
}
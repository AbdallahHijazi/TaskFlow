using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.AI.Models;
using TaskFlow.Application.AI.Providers;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.AI;
using TaskFlow.Application.DTOs.AI.CriticalTaskAnalysis;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;
using TaskFlow.Application.DTOs.AI.RiskAnalysis;
using TaskFlow.Application.DTOs.AI.TaskGeneration;
using TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Commands;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;
using TaskFlow.Application.Features.AI.RiskAnalysis.Commands;
using TaskFlow.Application.Features.AI.TaskGeneration.Commands;

namespace TaskFlow.API.Controllers.AI;
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

    [HttpPost("generate-initiatives")]
    public async Task<ActionResult<List<GeneratedInitiativePreview>>> GenerateInitiatives(
        [FromBody] GenerateInitiativesBatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GenerateInitiativesBatchCommand(request), cancellationToken);
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

    [HttpPost("generate-tasks-for-initiative")]
    public async Task<ActionResult<GeneratedTasksPreview>>GenerateTasksForInitiative([FromBody] GenerateTasksForInitiativeRequest request, 
                                                                                                CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GenerateTasksForInitiativeCommand(request),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("save-generated-tasks")]
    public async Task<ActionResult<SaveGeneratedTasksResponse>>SaveGeneratedTasks([FromBody] SaveGeneratedTasksRequest request,
                                                                                             CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SaveGeneratedTasksCommand(request),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("analyze-critical-tasks")]
    public async Task<ActionResult<CriticalTasksAnalysisResponse>>
    AnalyzeCriticalTasks(
        [FromBody] AnalyzeCriticalTasksRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AnalyzeCriticalTasksCommand(request),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("analyze-risks")]
    public async Task<IActionResult> AnalyzeRisks(
    AnalyzeRisksRequest request,
    CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(
                new AnalyzeRisksCommand(request),
                cancellationToken);

        return Ok(result);
    }
}

using System.Text.Json;
using MediatR;
using TaskFlow.Application.AI.Models;
using TaskFlow.Application.AI.Providers;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Validators;
using TaskFlow.Application.Common.Services;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Handlers;

public sealed class GenerateInitiativesBatchCommandHandler
    : IRequestHandler<GenerateInitiativesBatchCommand, List<GeneratedInitiativePreview>>
{
    private readonly ILLMProvider _llmProvider;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public GenerateInitiativesBatchCommandHandler(ILLMProvider llmProvider) => _llmProvider = llmProvider;

    public async Task<List<GeneratedInitiativePreview>> Handle(
        GenerateInitiativesBatchCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var language = GenerationLanguageDetector.Detect(request.Prompt);
        var languageName = GenerationLanguageDetector.Name(language);
        var prompt = BuildPrompt(request, languageName);
        var response = await _llmProvider.ExecuteAsync(new LLMRequest
        {
            OutputLanguage = language,
            SystemPrompt = $"You generate diverse structured initiative plans in {languageName} only. Return valid JSON only, without markdown or explanations. Never switch languages.",
            Prompt = prompt
        }, cancellationToken);

        var initiatives = Deserialize(response.Content);
        ApplyStyleDefaults(initiatives);
        var errors = Validate(initiatives, request.Count, language);
        if (errors.Count == 0) return initiatives;

        var correction = await _llmProvider.ExecuteAsync(new LLMRequest
        {
            OutputLanguage = language,
            SystemPrompt = "You correct structured JSON. Return corrected valid JSON only.",
            Prompt = $$"""
                Correct the following JSON array. Write every name and description in {{languageName}} only.
                It must contain exactly {{request.Count}} valid and meaningfully different initiatives.
                Validation errors:
                {{string.Join(Environment.NewLine, errors.Select(error => $"- {error}"))}}

                Invalid JSON:
                {{CleanJson(response.Content)}}

                Preserve the required array structure, use {{languageName}} content, use ISO 8601 dates,
                keep every task inside its initiative date range, use #RRGGBB colors,
                and generate between 3 and 8 unique tasks per initiative.
                """
        }, cancellationToken);

        initiatives = Deserialize(correction.Content);
        ApplyStyleDefaults(initiatives);
        errors = Validate(initiatives, request.Count, language);
        if (errors.Count > 0)
            throw new InvalidOperationException("فشل الذكاء الاصطناعي في توليد مجموعة مبادرات صالحة: " + string.Join(" | ", errors));

        return initiatives;
    }

    private static string BuildPrompt(GenerateInitiativesBatchRequest request, string languageName) => $$"""
        Generate exactly {{request.Count}} meaningfully different initiatives for this request:
        "{{request.Prompt}}"

        Return one JSON array only. Each item must have this structure:
        {
          "name": "اسم المبادرة",
          "description": "وصف المبادرة",
          "startDate": "2026-08-01T00:00:00Z",
          "endDate": "2026-09-01T00:00:00Z",
          "color": "#4F46E5",
          "icon": "initiative",
          "tasks": [{
            "name": "اسم المهمة", "description": "وصف المهمة",
            "startDate": "2026-08-01T00:00:00Z", "endDate": "2026-08-05T00:00:00Z",
            "color": "#4F46E5", "icon": "task"
          }]
        }

        Rules:
        - Write every initiative and task name and description in {{languageName}} only.
        - Never translate the output to Chinese or any other language.
        - JSON only; no markdown, comments, null, undefined, or explanations.
        - Use clear {{languageName}} names and descriptions.
        - Make the initiatives different in scope or execution strategy, not renamed copies.
        - Generate 3 to 8 unique, domain-specific tasks for each initiative.
        - Use logical ISO 8601 dates and keep every task within its initiative dates.
        - Use valid #RRGGBB colors and short meaningful icon values.
        """;

    private static List<string> Validate(List<GeneratedInitiativePreview> initiatives, int count, GenerationLanguage language)
    {
        var errors = new List<string>();
        if (initiatives.Count != count) errors.Add($"عدد المبادرات المتوقع {count} لكن الناتج {initiatives.Count}.");
        for (var index = 0; index < initiatives.Count; index++)
        {
            errors.AddRange(GeneratedInitiativePreviewValidator.Validate(initiatives[index]).Select(error => $"المبادرة {index + 1}: {error}"));
            if (!GenerationLanguageDetector.Matches(initiatives[index].Name, language) ||
                !GenerationLanguageDetector.Matches(initiatives[index].Description, language) ||
                initiatives[index].Tasks.Any(task => !GenerationLanguageDetector.Matches(task.Name, language) || !GenerationLanguageDetector.Matches(task.Description, language)))
                errors.Add($"المبادرة {index + 1}: لغة المحتوى لا تطابق لغة طلب المستخدم.");
        }
        var duplicateNames = initiatives.GroupBy(item => item.Name.Trim(), StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1);
        errors.AddRange(duplicateNames.Select(group => $"اسم المبادرة مكرر: {group.Key}"));
        return errors;
    }

    private static void ApplyStyleDefaults(IEnumerable<GeneratedInitiativePreview> initiatives)
    {
        foreach (var initiative in initiatives)
        {
            var style = WorkItemStyleDefaults.ForInitiative(initiative.Name, initiative.Description, initiative.Color, initiative.Icon);
            initiative.Color = style.Color;
            initiative.Icon = style.Icon;
            foreach (var task in initiative.Tasks)
            {
                var taskStyle = WorkItemStyleDefaults.ForTask(task.Name, task.Description, task.Color, task.Icon);
                task.Color = taskStyle.Color;
                task.Icon = taskStyle.Icon;
            }
        }
    }

    private static List<GeneratedInitiativePreview> Deserialize(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<List<GeneratedInitiativePreview>>(CleanJson(content), JsonOptions)
                   ?? throw new InvalidOperationException("تعذر تحويل استجابة الذكاء الاصطناعي.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("استجابة الذكاء الاصطناعي ليست مصفوفة JSON صحيحة.", exception);
        }
    }

    private static string CleanJson(string content)
    {
        var cleaned = content.Trim();
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase)) cleaned = cleaned[7..];
        else if (cleaned.StartsWith("```")) cleaned = cleaned[3..];
        if (cleaned.EndsWith("```")) cleaned = cleaned[..^3];
        return cleaned.Trim();
    }
}

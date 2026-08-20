using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskFlow.Application.AI.Models;
using TaskFlow.Application.AI.Providers;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.AI.TaskGeneration;
using TaskFlow.Application.Features.AI.TaskGeneration.Commands;
using TaskFlow.Application.Features.AI.TaskGeneration.Services;
using TaskFlow.Application.Features.AI.TaskGeneration.Validators;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Application.Common.Services;

namespace TaskFlow.Application.Features.AI.TaskGeneration.Handlers
{
    public sealed class GenerateTasksForInitiativeCommandHandler
        : IRequestHandler<
            GenerateTasksForInitiativeCommand,
            GeneratedTasksPreview>
    {
        private readonly ILLMProvider _llmProvider;
        private readonly IRepository<Initiative> _initiativeRepository;
        private readonly IRepository<TaskItem> _taskRepository;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public GenerateTasksForInitiativeCommandHandler(
            ILLMProvider llmProvider,
            IRepository<Initiative> initiativeRepository,
            IRepository<TaskItem> taskRepository)
        {
            _llmProvider = llmProvider;
            _initiativeRepository = initiativeRepository;
            _taskRepository = taskRepository;
        }

        public async Task<GeneratedTasksPreview> Handle(
            GenerateTasksForInitiativeCommand command,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Request);

            var request = command.Request;
            var language = GenerationLanguageDetector.Detect(request.Prompt);
            var languageName = GenerationLanguageDetector.Name(language);

            var initiative =
                _initiativeRepository.Get(request.InitiativeId);

            if (initiative is null)
            {
                throw new NotFoundException(
                    "المبادرة",
                    request.InitiativeId);
            }

            if (initiative.StartDate is null)
            {
                throw new InvalidOperationException(
                    "تاريخ بداية المبادرة غير موجود.");
            }

            if (initiative.EndDate is null)
            {
                throw new InvalidOperationException(
                    "تاريخ نهاية المبادرة غير موجود.");
            }

            var existingTaskNames =
                _taskRepository.GetAll()
                    .Where(task =>
                        task.InitiativeId == initiative.Id)
                    .Select(task => task.Name)
                    .Where(name =>
                        !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!)
                    .ToList();

            var prompt =
                GenerateTasksForInitiativePromptBuilder.Build(
                    request,
                    initiative.Name ?? string.Empty,
                    initiative.Description,
                    initiative.StartDate.Value,
                    initiative.EndDate,
                    existingTaskNames);

            var llmResponse =
                await _llmProvider.ExecuteAsync(
                    new LLMRequest
                    {
                        OutputLanguage = language,
                        SystemPrompt =
                            $$"""
                        You generate structured tasks
                        for an existing initiative.

                        Return valid JSON only.
                        Do not return markdown.
                        Do not return explanations.
                        Use {{languageName}} only. Never switch languages.
                        """,

                        Prompt = prompt
                    },
                    cancellationToken);

            var preview =
                DeserializeResponse(llmResponse.Content);
            ApplyStyleDefaults(preview);

            preview.InitiativeId = initiative.Id;
            preview.InitiativeName =
                initiative.Name ?? string.Empty;

            var validationErrors =
               GeneratedTasksPreviewValidator.Validate(
                   preview,
                   initiative.StartDate.Value,
                   initiative.EndDate,
                   existingTaskNames);
            AddLanguageErrors(preview, language, validationErrors);

            if (validationErrors.Count == 0)
            {
                return preview;
            }

            var correctedPreview =
                await TryCorrectResponseAsync(
                    llmResponse.Content,
                    validationErrors,
                    initiative,
                    existingTaskNames,
                    language,
                    cancellationToken);
            ApplyStyleDefaults(correctedPreview);

            correctedPreview.InitiativeId = initiative.Id;
            correctedPreview.InitiativeName =
                initiative.Name ?? string.Empty;

            var correctedValidationErrors =
                GeneratedTasksPreviewValidator.Validate(
                    correctedPreview,
                    initiative.StartDate.Value,
                    initiative.EndDate,
                    existingTaskNames);
            AddLanguageErrors(correctedPreview, language, correctedValidationErrors);

            if (correctedValidationErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "فشل الذكاء الاصطناعي في توليد مهام صالحة بعد محاولة التصحيح: "
                    +
                    string.Join(
                        " | ",
                        correctedValidationErrors));
            }

            return correctedPreview;
        }

        private async Task<GeneratedTasksPreview>
            TryCorrectResponseAsync(
                string originalContent,
                List<string> validationErrors,
                Initiative initiative,
                IReadOnlyCollection<string> existingTaskNames,
                GenerationLanguage language,
                CancellationToken cancellationToken)
                {
                    var existingTasksText =
                        existingTaskNames.Count == 0
                            ? "- No existing tasks."
                            : string.Join(
                                Environment.NewLine,
                                existingTaskNames.Select(
                                    name => $"- {name}"));

                    var correctionPrompt =
                        $$"""
                The following generated tasks JSON is invalid.

                Validation errors:
                {{string.Join(
                            Environment.NewLine,
                            validationErrors.Select(
                                error => $"- {error}"))}}

                Existing initiative:
                - Name: {{initiative.Name}}
                - Description: {{initiative.Description}}
                - Start date: {{initiative.StartDate:O}}
                - End date: {{initiative.EndDate:O}}

                Tasks already existing in the initiative:
                {{existingTasksText}}

                Invalid JSON:
                {{CleanJson(originalContent)}}

                Correct the JSON while preserving the user's intent.

                Important rules:
                - Write every task name and description in {{GenerationLanguageDetector.Name(language)}} only.
                - Never switch to Chinese or any other language.
                - Return JSON only.
                - Do not include markdown.
                - Do not include explanations.
                - Generate tasks only, not an initiative.
                - Use clear and meaningful {{GenerationLanguageDetector.Name(language)}}.
                - Do not duplicate any existing task.
                - Do not duplicate names among the generated tasks.
                - Every task must fall completely within the initiative date range.
                - Task end date must not precede its start date.
                - Do not return null or undefined values.
                - Colors must use the format #RRGGBB.
                - Keep exactly this JSON structure:

                {
                  "tasks": [
                    {
                      "name": "اسم المهمة",
                      "description": "وصف المهمة",
                      "startDate": "2026-08-01T00:00:00Z",
                      "endDate": "2026-08-05T00:00:00Z",
                      "color": "#4F46E5",
                      "icon": "task"
                    }
                  ]
                }
                """;

            var correctionResponse =
                await _llmProvider.ExecuteAsync(
                    new LLMRequest
                    {
                        OutputLanguage = language,
                        SystemPrompt =
                            """
                    You correct invalid structured task JSON.

                    Return valid JSON only.
                    Do not return markdown.
                    Do not return explanations.
                    Use clear and meaningful Arabic language.
                    """,

                        Prompt = correctionPrompt
                    },
                    cancellationToken);

            return DeserializeResponse(
                correctionResponse.Content);
        }
        private static GeneratedTasksPreview DeserializeResponse(
            string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException(
                    "لم يُرجع الذكاء الاصطناعي أي بيانات.");
            }

            var json = CleanJson(content);

            try
            {
                var result =
                    JsonSerializer.Deserialize<
                        GeneratedTasksPreview>(
                        json,
                        JsonOptions);

                return result
                       ??
                       throw new InvalidOperationException(
                           "تعذر تحويل استجابة الذكاء الاصطناعي.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "استجابة الذكاء الاصطناعي ليست بصيغة JSON صحيحة.",
                    ex);
            }
        }

        private static void AddLanguageErrors(
            GeneratedTasksPreview preview,
            GenerationLanguage language,
            List<string> errors)
        {
            if (preview.Tasks.Any(task =>
                    !GenerationLanguageDetector.Matches(task.Name, language) ||
                    !GenerationLanguageDetector.Matches(task.Description, language)))
                errors.Add("لغة المهام المولدة لا تطابق لغة طلب المستخدم.");
        }

        private static void ApplyStyleDefaults(GeneratedTasksPreview preview)
        {
            foreach (var task in preview.Tasks)
            {
                var style = WorkItemStyleDefaults.ForTask(task.Name, task.Description, task.Color, task.Icon);
                task.Color = style.Color;
                task.Icon = style.Icon;
            }
        }

        private static string CleanJson(string content)
        {
            var cleaned = content.Trim();

            if (cleaned.StartsWith(
                    "```json",
                    StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned[7..];
            }
            else if (cleaned.StartsWith("```"))
            {
                cleaned = cleaned[3..];
            }

            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned[..^3];
            }

            return cleaned.Trim();
        }
    }
}

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
using TaskFlow.Application.DTOs.AI.CriticalTaskAnalysis;
using TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Commands;
using TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Services;
using TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Validators;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Handlers
{
    public sealed class AnalyzeCriticalTasksCommandHandler
        : IRequestHandler<
            AnalyzeCriticalTasksCommand,
            CriticalTasksAnalysisResponse>
    {
        private readonly ILLMProvider _llmProvider;
        private readonly IRepository<Initiative> _initiativeRepository;
        private readonly IRepository<TaskItem> _taskRepository;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public AnalyzeCriticalTasksCommandHandler(
            ILLMProvider llmProvider,
            IRepository<Initiative> initiativeRepository,
            IRepository<TaskItem> taskRepository)
        {
            _llmProvider = llmProvider;
            _initiativeRepository = initiativeRepository;
            _taskRepository = taskRepository;
        }

        public async Task<CriticalTasksAnalysisResponse> Handle(
            AnalyzeCriticalTasksCommand command,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Request);

            var initiative =
                _initiativeRepository.Get(
                    command.Request.InitiativeId);

            if (initiative is null)
            {
                throw new NotFoundException(
                    "المبادرة",
                    command.Request.InitiativeId);
            }

            var tasks =
                _taskRepository.GetAll()
                    .Where(task =>
                        task.InitiativeId == initiative.Id)
                    .ToList();

            if (tasks.Count == 0)
            {
                throw new InvalidOperationException(
                    "لا توجد مهام ضمن المبادرة لتحليلها.");
            }

            var prompt =
                AnalyzeCriticalTasksPromptBuilder.Build(
                    initiative,
                    tasks);

            var llmResponse =
                await _llmProvider.ExecuteAsync(
                    new LLMRequest
                    {
                        SystemPrompt =
                            """
                        You analyze critical tasks in project initiatives.

                        Use only the tasks provided in the user prompt.
                        Return valid JSON only.
                        Do not return markdown or explanations outside JSON.
                        Write summary, reasons, and recommendations in clear Arabic.
                        """,

                        Prompt = prompt
                    },
                    cancellationToken);

            var response =
                DeserializeResponse(llmResponse.Content);

            response.InitiativeId = initiative.Id;
            response.InitiativeName =
                initiative.Name ?? string.Empty;

            var validationErrors =
                           CriticalTasksAnalysisResponseValidator.Validate(
                               response,
                               tasks);

            if (validationErrors.Count == 0)
            {
                return response;
            }

            var correctedResponse =
                await TryCorrectResponseAsync(
                    llmResponse.Content,
                    validationErrors,
                    initiative,
                    tasks,
                    cancellationToken);

            correctedResponse.InitiativeId = initiative.Id;
            correctedResponse.InitiativeName =
                initiative.Name ?? string.Empty;

            var correctedValidationErrors =
                CriticalTasksAnalysisResponseValidator.Validate(
                    correctedResponse,
                    tasks);

            if (correctedValidationErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "فشل الذكاء الاصطناعي في إرجاع تحليل صالح بعد محاولة التصحيح: "
                    +
                    string.Join(
                        " | ",
                        correctedValidationErrors));
            }

            return correctedResponse;
        }

        private async Task<CriticalTasksAnalysisResponse>TryCorrectResponseAsync(
                                                                            string originalContent,
                                                                            List<string> validationErrors,
                                                                            Initiative initiative,
                                                                            IReadOnlyCollection<TaskItem> tasks,
                                                                            CancellationToken cancellationToken)
        {
            var validTasksText =
                string.Join(
                    Environment.NewLine,
                    tasks.Select(task =>
                        $"- TaskId: {task.Id}, TaskName: {task.Name}"));

            var correctionPrompt =
                $$"""
                    The following critical-task analysis JSON is invalid.

                    Validation errors:
                    {{string.Join(
                                Environment.NewLine,
                                validationErrors.Select(
                                    error => $"- {error}"))}}

                    Initiative:
                    - InitiativeId: {{initiative.Id}}
                    - Name: {{initiative.Name}}

                    Valid tasks:
                    {{validTasksText}}

                    Invalid JSON:
                    {{CleanJson(originalContent)}}

                    Correct the JSON using only the valid tasks listed above.

                    Important rules:
                    - Return valid JSON only.
                    - Do not return markdown.
                    - Do not include explanations outside JSON.
                    - Never invent a TaskId.
                    - Copy TaskId and TaskName exactly from the valid tasks.
                    - Do not repeat the same task.
                    - CriticalityScore must be an integer from 1 to 100.
                    - CriticalityLevel must match the score:
                      1-25 = Low
                      26-50 = Medium
                      51-75 = High
                      76-100 = Critical
                    - Summary, reason, and recommendation must be written in Arabic.
                    - If no task is critical, return an empty criticalTasks array.
                    - Do not return null or undefined values.

                    Return exactly this JSON structure:

                    {
                      "summary": "ملخص عام لنتيجة التحليل",
                      "criticalTasks": [
                        {
                          "taskId": "00000000-0000-0000-0000-000000000000",
                          "taskName": "اسم المهمة كما هو في النظام",
                          "criticalityScore": 80,
                          "criticalityLevel": "Critical",
                          "reason": "سبب اعتبار المهمة حرجة",
                          "recommendation": "الإجراء المقترح"
                        }
                      ]
                    }
                    """;

            var correctionResponse =
                await _llmProvider.ExecuteAsync(
                    new LLMRequest
                    {
                        SystemPrompt =
                            """
                    You correct invalid critical-task analysis JSON.

                    Use only the provided valid tasks.
                    Return valid JSON only.
                    Do not return markdown or explanations outside JSON.
                    """,

                        Prompt = correctionPrompt
                    },
                    cancellationToken);

            return DeserializeResponse(
                correctionResponse.Content);
        }
        private static CriticalTasksAnalysisResponse DeserializeResponse(
            string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException(
                    "لم يُرجع الذكاء الاصطناعي نتيجة تحليل.");
            }

            var json = CleanJson(content);

            try
            {
                var result =
                    JsonSerializer.Deserialize<
                        CriticalTasksAnalysisResponse>(
                        json,
                        JsonOptions);

                return result
                       ??
                       throw new InvalidOperationException(
                           "تعذر تحويل استجابة تحليل المهام الحرجة.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "استجابة تحليل المهام الحرجة ليست بصيغة JSON صحيحة.",
                    ex);
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

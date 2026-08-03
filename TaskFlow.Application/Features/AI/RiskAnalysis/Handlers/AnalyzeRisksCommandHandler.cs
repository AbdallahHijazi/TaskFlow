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
using TaskFlow.Application.DTOs.AI.RiskAnalysis;
using TaskFlow.Application.Features.AI.RiskAnalysis.Commands;
using TaskFlow.Application.Features.AI.RiskAnalysis.Services;
using TaskFlow.Application.Features.AI.RiskAnalysis.Validators;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.AI.RiskAnalysis.Handlers
{

    public sealed class AnalyzeRisksCommandHandler:IRequestHandler< AnalyzeRisksCommand,
                                                                    RiskAnalysisResponse>
    {
        private readonly ILLMProvider _llmProvider;
        private readonly IRepository<Initiative> _initiativeRepository;
        private readonly IRepository<TaskItem> _taskRepository;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public AnalyzeRisksCommandHandler(
            ILLMProvider llmProvider,
            IRepository<Initiative> initiativeRepository,
            IRepository<TaskItem> taskRepository)
        {
            _llmProvider = llmProvider;
            _initiativeRepository = initiativeRepository;
            _taskRepository = taskRepository;
        }

        public async Task<RiskAnalysisResponse> Handle(
    AnalyzeRisksCommand command,
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
                    "لا توجد مهام ضمن المبادرة لتحليل المخاطر.");
            }

            var prompt =
                RiskAnalysisPromptBuilder.Build(
                    initiative,
                    tasks);

            var llmResponse =
                await _llmProvider.ExecuteAsync(
                    new LLMRequest
                    {
                        SystemPrompt =
                            """
                    You analyze project risks in initiatives.

                    Use only the provided initiative and tasks.
                    Return valid JSON only.
                    Do not return markdown or explanations outside JSON.
                    Write summary and recommendations in clear Arabic.
                    """,

                        Prompt = prompt
                    },
                    cancellationToken);

            //    var response =
            //        DeserializeResponse(
            //            llmResponse.Content);

            //    response.InitiativeId = initiative.Id;
            //    response.InitiativeName =
            //        initiative.Name ?? string.Empty;

            //var validationErrors =
            //    RiskAnalysisResponseValidator.Validate(
            //        response);

            //if (validationErrors.Count > 0)
            //{
            //    throw new InvalidOperationException(
            //        "فشل الذكاء الاصطناعي في إرجاع تحليل مخاطر صالح: "
            //        +
            //        string.Join(
            //            " | ",
            //            validationErrors));
            //}

            //return response;
            RiskAnalysisResponse? response = null;
            var validationErrors = new List<string>();

            try
            {
                response =
                    DeserializeResponse(
                        llmResponse.Content);

                response.InitiativeId = initiative.Id;
                response.InitiativeName =
                    initiative.Name ?? string.Empty;

                validationErrors =
                    RiskAnalysisResponseValidator.Validate(
                        response);

                if (validationErrors.Count == 0)
                {
                    return response;
                }
            }
            catch (InvalidOperationException ex)
            {
                validationErrors.Add(
                    $"استجابة التحليل غير صالحة: {ex.Message}");
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
                RiskAnalysisResponseValidator.Validate(
                    correctedResponse);

            if (correctedValidationErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "فشل الذكاء الاصطناعي في إرجاع تحليل مخاطر صالح بعد محاولة التصحيح: "
                    +
                    string.Join(
                        " | ",
                        correctedValidationErrors));
            }

            return correctedResponse;
        }
        private async Task<RiskAnalysisResponse>
    TryCorrectResponseAsync(
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
                        $"""
                - TaskId: {task.Id}
                  Name: {task.Name}
                  Description: {task.Description}
                  StartDate: {task.StartDate}
                  EndDate: {task.EndDate}
                  Progress: {task.Progress}
                """));

            var correctionPrompt =
                $$"""
        The following project risk-analysis response is invalid.

        Validation errors:
        {{string.Join(
                    Environment.NewLine,
                    validationErrors.Select(
                        error => $"- {error}"))}}

        Initiative:
        - InitiativeId: {{initiative.Id}}
        - Name: {{initiative.Name}}
        - Description: {{initiative.Description ?? "No description"}}

        Valid initiative tasks:
        {{validTasksText}}

        Invalid response:
        {{originalContent}}

        Correct the response while using only the provided initiative
        and task information.

        Important rules:
        - Return valid JSON only.
        - Do not return markdown.
        - Do not include explanations outside JSON.
        - Do not invent unsupported facts or tasks.
        - Do not repeat the same risk.
        - Severity must be exactly one of:
          Low, Medium, High, Critical.
        - Write summary, title, reason, impact, and recommendation in Arabic.
        - Do not return null or undefined values.
        - If no meaningful risks exist, return an empty risks array.

        Return exactly this JSON structure:

        {
          "summary": "ملخص عام لتحليل المخاطر",
          "risks": [
            {
              "riskTitle": "عنوان الخطر",
              "severity": "High",
              "reason": "سبب وجود الخطر",
              "impact": "التأثير المتوقع",
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
                    You correct invalid project risk-analysis JSON.

                    Use only the provided initiative and tasks.
                    Return valid JSON only.
                    Do not return markdown or explanations outside JSON.
                    """,

                        Prompt = correctionPrompt
                    },
                    cancellationToken);

            return DeserializeResponse(
                correctionResponse.Content);
        }
        private static RiskAnalysisResponse DeserializeResponse(
                                                                string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException(
                    "لم يُرجع الذكاء الاصطناعي نتيجة تحليل المخاطر.");
            }

            var json = CleanJson(content);

            try
            {
                var result =
                    JsonSerializer.Deserialize<RiskAnalysisResponse>(
                        json,
                        JsonOptions);

                return result
                       ??
                       throw new InvalidOperationException(
                           "تعذر تحويل استجابة تحليل المخاطر.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "استجابة تحليل المخاطر ليست بصيغة JSON صحيحة.",
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

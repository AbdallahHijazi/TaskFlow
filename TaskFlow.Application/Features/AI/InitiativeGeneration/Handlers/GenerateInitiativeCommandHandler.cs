using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskFlow.Application.AI.Models;
using TaskFlow.Application.AI.Providers;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Commands;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Services;
using TaskFlow.Application.Features.AI.InitiativeGeneration.Validators;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Handlers
{
    public sealed class GenerateInitiativeCommandHandler
     : IRequestHandler<
         GenerateInitiativeCommand,
         GeneratedInitiativePreview>
    {
        private readonly ILLMProvider _llmProvider;

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public GenerateInitiativeCommandHandler(
            ILLMProvider llmProvider)
        {
            _llmProvider = llmProvider;
        }

        public async Task<GeneratedInitiativePreview> Handle(
            GenerateInitiativeCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Request);

            var prompt =
                GenerateInitiativePromptBuilder.Build(
                    request.Request);

            var firstResponse =
                await _llmProvider.ExecuteAsync(
                    new LLMRequest
                    {
                        SystemPrompt =
                            """
                        You generate structured initiative plans.

                        Return valid JSON only.
                        Do not return markdown.
                        Do not return explanations.
                        Use clear and meaningful Arabic language.
                        """,

                        Prompt = prompt
                    },
                    cancellationToken);

            var firstResult =
                DeserializeResponse(firstResponse.Content);

            var firstValidationErrors =
                GeneratedInitiativePreviewValidator.Validate(
                    firstResult);

            if (firstValidationErrors.Count == 0)
            {
                return firstResult;
            }

            // محاولة واحدة لتصحيح نتيجة الذكاء
            var correctedResult =
                await TryCorrectResponseAsync(
                    firstResponse.Content,
                    firstValidationErrors,
                    cancellationToken);

            var correctedValidationErrors =
                GeneratedInitiativePreviewValidator.Validate(
                    correctedResult);

            if (correctedValidationErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "فشل الذكاء الاصطناعي في توليد مبادرة صالحة بعد محاولة التصحيح: "
                    +
                    string.Join(
                        " | ",
                        correctedValidationErrors));
            }

            return correctedResult;
        }

        private async Task<GeneratedInitiativePreview>
            TryCorrectResponseAsync(
                string originalContent,
                List<string> validationErrors,
                CancellationToken cancellationToken)
        {
            var correctionPrompt =
                $$"""
            The following generated initiative JSON is invalid.

            Validation errors:
            {{string.Join(
                    Environment.NewLine,
                    validationErrors.Select(
                        error => $"- {error}"))}}

            Invalid JSON:
            {{CleanJson(originalContent)}}

            Correct the JSON while preserving the original intent.

            Important rules:
            - Return JSON only.
            - Do not include markdown.
            - Do not include explanations.
            - Use clear and meaningful Arabic language.
            - Do not return null or undefined values.
            - Initiative end date must not precede its start date.
            - Every task must fall completely within the initiative date range.
            - Task names must be unique.
            - Generate between 3 and 8 tasks.
            - Colors must use the format #RRGGBB.
            - Keep exactly the same JSON structure.
            """;

            var correctionResponse =
                await _llmProvider.ExecuteAsync(
                    new LLMRequest
                    {
                        SystemPrompt =
                            """
                        You correct invalid structured JSON.

                        Return corrected valid JSON only.
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

        private static GeneratedInitiativePreview
            DeserializeResponse(string content)
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
                        GeneratedInitiativePreview>(
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

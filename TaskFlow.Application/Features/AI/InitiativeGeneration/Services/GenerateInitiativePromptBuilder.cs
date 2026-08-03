using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Services
{
    public static class GenerateInitiativePromptBuilder
    {
        public static string Build(
            GenerateInitiativeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return $$"""
        You are an assistant specialized in project and initiative planning.

        Generate one initiative with its related tasks based on the user's request.

        User request:
        "{{request.Prompt}}"

        Important rules:
        - Return JSON only.
        - Do not include markdown.
        - Do not include explanations.
        - Do not include comments.
        - Do not return null or undefined values.
        - Generate between 5 and 10 tasks.
        - Every task must belong to the generated initiative.
        - Task names must be unique.
        - Use clear Arabic names and descriptions.
        - Generate tasks that are specific to the business domain of the initiative.
        - Do not generate only generic software lifecycle stages such as analysis, design, development, and testing.
        - The tasks must represent actual functional work inside the requested system.
        - Initiative and task dates must be logical.
        - Every task start date and end date must fall within the initiative date range.
        - The end date must not be earlier than the start date.
        - Use ISO 8601 date format.
        - Color must be a valid hexadecimal color such as #4F46E5.
        - Icon must be a short meaningful text value.

        Return exactly this JSON structure:

        {
          "name": "اسم المبادرة",
          "description": "وصف المبادرة",
          "startDate": "2026-08-01T00:00:00Z",
          "endDate": "2026-09-01T00:00:00Z",
          "color": "#4F46E5",
          "icon": "initiative",
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
        }
    }
}

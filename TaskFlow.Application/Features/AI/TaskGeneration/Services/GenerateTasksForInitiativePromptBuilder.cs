using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.TaskGeneration;

namespace TaskFlow.Application.Features.AI.TaskGeneration.Services
{
    public static class GenerateTasksForInitiativePromptBuilder
    {
        public static string Build(
            GenerateTasksForInitiativeRequest request,
            string initiativeName,
            string? initiativeDescription,
            DateTime initiativeStartDate,
            DateTime? initiativeEndDate,
            IReadOnlyCollection<string> existingTaskNames)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existingTasksText =
                existingTaskNames.Count == 0
                    ? "- No existing tasks."
                    : string.Join(
                        Environment.NewLine,
                        existingTaskNames.Select(name => $"- {name}"));

            return $$"""
        You are an assistant specialized in project task planning.

        Generate new tasks for the existing initiative described below.

        Existing initiative:
        - Name: {{initiativeName}}
        - Description: {{initiativeDescription ?? "No description"}}
        - Start date: {{initiativeStartDate:O}}
        - End date: {{initiativeEndDate?.ToString("O") ?? "No end date"}}

        User request:
        "{{request.Prompt}}"

        Tasks already existing in this initiative:
        {{existingTasksText}}

        Determine the required number of tasks from the user's wording.

        Important rules:
        - If the user clearly requests one task, return exactly one task.
        - If the user requests multiple tasks or suggestions, return between 2 and 4 tasks.
        - Never return more than 4 tasks.
        - Return JSON only.
        - Do not include markdown.
        - Do not include explanations or comments.
        - Generate only tasks, not a new initiative.
        - Use clear and meaningful Arabic names and descriptions.
        - Generated tasks must be relevant to the initiative and user request.
        - Do not duplicate any existing task.
        - Do not generate duplicate names among the new tasks.
        - Avoid semantically equivalent duplicates with different wording.
        - Every task must fall completely within the initiative date range.
        - A task end date must not precede its start date.
        - Do not return null or undefined values.
        - Use ISO 8601 date format.
        - Color must be a valid hexadecimal value such as #4F46E5.
        - Icon must be a short meaningful text value.
        - Do not generate InitiativeId, StatusId, AssignedToId,
          Progress, IsAISuggested, ParentId, or database identifiers.

        Return exactly this JSON structure:

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
        }
    }
}

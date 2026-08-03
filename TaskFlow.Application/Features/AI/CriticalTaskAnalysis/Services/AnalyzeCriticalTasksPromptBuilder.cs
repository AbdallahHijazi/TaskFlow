using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Services
{
    public static class AnalyzeCriticalTasksPromptBuilder
    {
        public static string Build(
            Initiative initiative,
            IReadOnlyCollection<TaskItem> tasks)
        {
            ArgumentNullException.ThrowIfNull(initiative);
            ArgumentNullException.ThrowIfNull(tasks);

            var tasksText = new StringBuilder();

            foreach (var task in tasks)
            {
                tasksText.AppendLine(
                    $$"""
                Task:
                - TaskId: {{task.Id}}
                - Name: {{task.Name}}
                - Description: {{task.Description ?? "No description"}}
                - StartDate: {{task.StartDate:O}}
                - EndDate: {{task.EndDate?.ToString("O") ?? "No end date"}}
                - Progress: {{task.Progress}}
                - IsActive: {{task.IsActive}}
                """);
            }

            return $$"""
                You are a project management assistant specialized in identifying
                critical tasks within an existing initiative.

                Analyze the initiative and its tasks, then return only the tasks
                that are genuinely critical or require special attention.

                Initiative:
                - InitiativeId: {{initiative.Id}}
                - Name: {{initiative.Name}}
                - Description: {{initiative.Description ?? "No description"}}
                - StartDate: {{initiative.StartDate?.ToString("O") ?? "No start date"}}
                - EndDate: {{initiative.EndDate?.ToString("O") ?? "No end date"}}

                Existing tasks:
                {{tasksText}}

                Analyze criticality using the following weighted evaluation model:

                Criticality Score Calculation:

                Calculate the final CriticalityScore from 1 to 100 based on:

                1. Task Importance (30%)
                - How essential is this task for achieving the initiative objective.
                - Does the initiative depend on completing this task.
                - Is this task a main functional or technical component.

                2. Delay Impact (25%)
                - Estimate how much delaying this task affects the initiative timeline.
                - Consider whether other tasks depend on completing this task.

                3. Schedule Pressure (25%)
                - Consider the task start date and end date.
                - Consider the remaining time until deadline.
                - Compare the available time with the current progress.

                4. Progress Risk (20%)
                - Compare current task progress with expected progress based on elapsed time.
                - A task with low progress and a close deadline should receive a higher risk score.

                Score Interpretation:
                - 1 to 25   = Low
                - 26 to 50  = Medium
                - 51 to 75  = High
                - 76 to 100 = Critical


                Important:
                - Do not mark a task as critical only because its name sounds important.
                - Use the provided task data (name, description, dates, progress).
                - The score must reflect the weighted evaluation above.

                Important rules:
                - Analyze only tasks included in the provided list.
                - Do not invent additional risk factors that are not supported by the provided data.
                - Every critical task must include a clear reason explaining which evaluation factors increased its score.
                - Never invent a new TaskId or task.
                - Copy TaskId and TaskName exactly from the provided tasks.
                - Return only tasks that deserve attention.
                - If no task is critical, return an empty criticalTasks array.
                - CriticalityScore must be an integer from 1 to 100.
                - CriticalityLevel must be exactly one of:
                  Low, Medium, High, Critical.
                - Keep the score and level consistent:
                  1-25 = Low
                  26-50 = Medium
                  51-75 = High
                  76-100 = Critical
                - Explain the reason using the available task data.
                - Provide a practical recommendation.
                - Use clear Arabic for summary, reason, and recommendation.
                - Return valid JSON only.
                - Do not return markdown, comments, or explanations outside JSON.
                - Do not return null or undefined values.
                - Return only the top 20-30% most critical tasks when there are many tasks.
                - Do not mark many tasks as Critical unless there is strong evidence from the provided data.
                - Prefer identifying the most impactful tasks instead of assigning high scores to all tasks.
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
        }
    }
}

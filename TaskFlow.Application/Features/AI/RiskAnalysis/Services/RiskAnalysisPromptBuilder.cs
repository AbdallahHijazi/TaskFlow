using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.AI.RiskAnalysis.Services
{
    public static class RiskAnalysisPromptBuilder
    {
        public static string Build(
            Initiative initiative,
            IReadOnlyCollection<TaskItem> tasks)
        {
            var currentDate = DateTime.UtcNow;

            var tasksText =
                tasks.Count == 0
                    ? "No tasks available."
                    : string.Join(
                        Environment.NewLine,
                        tasks.Select(task =>
                            $"""
                        TaskId: {task.Id}
                        Name: {task.Name}
                        Description: {task.Description ?? "No description"}
                        StartDate: {task.StartDate:O}
                        EndDate: {task.EndDate:O}
                        Progress: {task.Progress}
                        ---
                        """));

            return
                $$"""
                    You are a project risk analysis assistant.

                    Analyze the current and potential future risks of the following
                    initiative based only on the supplied initiative and task data.

                    CurrentDateUtc: {{currentDate:O}}

                    Initiative:
                    Id: {{initiative.Id}}
                    Name: {{initiative.Name}}
                    Description: {{initiative.Description ?? "No description"}}
                    StartDate: {{initiative.StartDate?.ToString("O") ?? "No start date"}}
                    EndDate: {{initiative.EndDate?.ToString("O") ?? "No end date"}}

                    Tasks:
                    {{tasksText}}

                    Analyze risks in the following areas:

                    1. Overdue task risks:
                    - A task is overdue when its EndDate is before CurrentDateUtc
                    and its Progress is below 100.
                    - An overdue task must always be reported as a risk.
                    - The severity should normally be High or Critical depending
                    on the delay duration and remaining progress.

                    2. Schedule risks:
                    - Tasks approaching their deadline with low progress.
                    - Tasks with unrealistically short durations.
                    - Multiple tasks overlapping within a limited period.
                    - Tasks concentrated near the initiative deadline.
                    - Insufficient time allocated for testing, review, or fixing errors.

                    3. Progress risks:
                    - Progress that is significantly lower than expected based on
                    elapsed time.
                    - A task that has already started but still has zero or very
                    low progress.
                    - A task whose deadline is near while most of its work remains.

                    4. Dependency and sequencing risks:
                    - Infer only clear and reasonable dependencies from task names,
                    descriptions, dates, and order.
                    - Report when the delay of an earlier foundational task could
                    affect later tasks.
                    - Do not invent undocumented technical dependencies.

                    5. Data-quality risks:
                    - Duplicate or nearly duplicate tasks.
                    - Tasks with unclear, corrupted, meaningless, or inconsistent names
                    or descriptions.
                    - Invalid or suspicious scheduling information.

                    6. Complexity risks:
                    - Tasks involving integrations, infrastructure, testing,
                    deployment, security, migration, or several complex functions.
                    - Complex tasks with insufficient scheduled time.

                    Risk scoring rules:

                    Probability must be exactly one of:
                    Low, Medium, High

                    Severity must be exactly one of:
                    Low, Medium, High, Critical

                    Use the following general assessment:

                    - Critical:
                    A confirmed major delay or a risk likely to significantly affect
                    the initiative deadline or several tasks.

                    - High:
                    A strong risk indicator with substantial expected impact.

                    - Medium:
                    A credible future risk that should be monitored and mitigated.

                    - Low:
                    A limited risk with minor expected impact.

                    Return valid JSON only, using exactly this structure:

                    {
                    "summary": "ملخص عربي واضح ومختصر عن حالة مخاطر المبادرة",
                    "risks": [
                        {
                        "title": "عنوان الخطر",
                        "description": "شرح سبب اعتبار هذا الأمر خطرًا بالاعتماد على البيانات",
                        "probability": "High",
                        "impact": "التأثير المتوقع على المهام أو المبادرة",
                        "severity": "Critical",
                        "affectedTasks": [
                            "00000000-0000-0000-0000-000000000000"
                        ],
                        "recommendation": "إجراء عملي مقترح لتقليل الخطر"
                        }
                    ]
                    }

                    Important rules:

                    - Use only the supplied initiative and tasks.
                    - Do not invent tasks, dates, progress values, or unsupported facts.
                    - Compare every task EndDate with CurrentDateUtc.
                    - Any task whose EndDate is before CurrentDateUtc and Progress is
                    below 100 must appear in at least one risk.
                    - Detect duplicate tasks by comparing names, descriptions, dates,
                    and progress values.
                    - Analyze both existing problems and credible future risks.
                    - Every risk must be supported by specific supplied data.
                    - affectedTasks must contain only TaskId values supplied above.
                    - Include all tasks affected by each risk.
                    - Do not return the same risk more than once.
                    - Do not create a separate identical risk for every overdue task;
                    group related tasks under one meaningful risk when appropriate.
                    - Return an empty risks array only when there are genuinely no
                    current or credible future risk indicators.
                    - Do not return an empty risks array when an overdue task,
                    suspicious duplicate, scheduling conflict, or clear progress
                    problem exists.
                    - Write summary, title, description, impact, and recommendation
                    in clear Arabic.
                    - Do not return null or undefined values.
                    - If a risk has no affected task, return an empty affectedTasks array.
                    - Return JSON only.
                    - Do not include Markdown, code fences, commentary, or explanations
                    outside the JSON.
                    """;
        }
    }
}

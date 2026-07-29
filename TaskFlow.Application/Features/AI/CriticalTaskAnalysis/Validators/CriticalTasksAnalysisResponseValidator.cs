using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.CriticalTaskAnalysis;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.AI.CriticalTaskAnalysis.Validators
{
    public static class CriticalTasksAnalysisResponseValidator
    {
        private static readonly HashSet<string> AllowedLevels =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "Low",
            "Medium",
            "High",
            "Critical"
            };

        public static List<string> Validate(
            CriticalTasksAnalysisResponse response,
            IReadOnlyCollection<TaskItem> existingTasks)
        {
            var errors = new List<string>();

            if (response is null)
            {
                errors.Add("لم يتم إرجاع نتيجة تحليل.");

                return errors;
            }

            if (string.IsNullOrWhiteSpace(response.Summary))
            {
                errors.Add("ملخص التحليل مطلوب.");
            }

            if (response.CriticalTasks is null)
            {
                errors.Add("قائمة المهام الحرجة غير موجودة.");

                return errors;
            }

            var tasksById = existingTasks
                .ToDictionary(task => task.Id);

            ValidateDuplicateTaskIds(
                response.CriticalTasks,
                errors);

            foreach (var item in response.CriticalTasks)
            {
                ValidateItem(
                    item,
                    tasksById,
                    errors);
            }

            return errors;
        }

        private static void ValidateItem(
            CriticalTaskAnalysisItem item,
            IReadOnlyDictionary<Guid, TaskItem> tasksById,
            List<string> errors)
        {
            if (item is null)
            {
                errors.Add("يوجد عنصر تحليل غير صالح.");

                return;
            }

            if (item.TaskId == Guid.Empty)
            {
                errors.Add("معرّف المهمة في نتيجة التحليل غير صالح.");

                return;
            }

            if (!tasksById.TryGetValue(
                    item.TaskId,
                    out var existingTask))
            {
                errors.Add(
                    $"المهمة ذات المعرّف {item.TaskId} " +
                    "غير موجودة ضمن المبادرة.");

                return;
            }

            if (string.IsNullOrWhiteSpace(item.TaskName))
            {
                errors.Add(
                    $"اسم المهمة {item.TaskId} غير موجود.");
            }
            else if (!string.Equals(
                         item.TaskName.Trim(),
                         existingTask.Name?.Trim(),
                         StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"اسم المهمة لا يطابق المهمة الأصلية: " +
                    $"{item.TaskId}");
            }

            if (item.CriticalityScore is < 1 or > 100)
            {
                errors.Add(
                    $"درجة خطورة المهمة {item.TaskId} " +
                    "يجب أن تكون بين 1 و100.");
            }

            if (string.IsNullOrWhiteSpace(
                    item.CriticalityLevel)
                ||
                !AllowedLevels.Contains(
                    item.CriticalityLevel.Trim()))
            {
                errors.Add(
                    $"مستوى خطورة المهمة {item.TaskId} غير صالح.");
            }
            else
            {
                ValidateScoreAndLevelConsistency(
                    item,
                    errors);
            }

            if (string.IsNullOrWhiteSpace(item.Reason))
            {
                errors.Add(
                    $"سبب خطورة المهمة {item.TaskId} مطلوب.");
            }

            if (string.IsNullOrWhiteSpace(
                    item.Recommendation))
            {
                errors.Add(
                    $"التوصية الخاصة بالمهمة {item.TaskId} مطلوبة.");
            }
        }

        private static void ValidateScoreAndLevelConsistency(
            CriticalTaskAnalysisItem item,
            List<string> errors)
        {
            var expectedLevel =
                item.CriticalityScore switch
                {
                    >= 1 and <= 25 => "Low",
                    >= 26 and <= 50 => "Medium",
                    >= 51 and <= 75 => "High",
                    >= 76 and <= 100 => "Critical",
                    _ => string.Empty
                };

            if (expectedLevel.Length > 0 &&
                !string.Equals(
                    item.CriticalityLevel.Trim(),
                    expectedLevel,
                    StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"مستوى الخطورة لا يتوافق مع الدرجة " +
                    $"للمهمة {item.TaskId}. " +
                    $"المستوى المتوقع: {expectedLevel}.");
            }
        }

        private static void ValidateDuplicateTaskIds(
            IReadOnlyCollection<CriticalTaskAnalysisItem> items,
            List<string> errors)
        {
            var duplicateIds = items
                .Where(item =>
                    item is not null &&
                    item.TaskId != Guid.Empty)
                .GroupBy(item => item.TaskId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            foreach (var duplicateId in duplicateIds)
            {
                errors.Add(
                    $"المهمة مكررة في نتيجة التحليل: {duplicateId}");
            }
        }
    }
}

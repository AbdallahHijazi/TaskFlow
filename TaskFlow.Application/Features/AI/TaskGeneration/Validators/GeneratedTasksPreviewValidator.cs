using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;
using TaskFlow.Application.DTOs.AI.TaskGeneration;

namespace TaskFlow.Application.Features.AI.TaskGeneration.Validators
{
    public static class GeneratedTasksPreviewValidator
    {
        private static readonly Regex HexColorRegex =
            new(
                "^#[0-9A-Fa-f]{6}$",
                RegexOptions.Compiled);

        public static List<string> Validate(
            GeneratedTasksPreview preview,
            DateTime initiativeStartDate,
            DateTime? initiativeEndDate,
            IReadOnlyCollection<string> existingTaskNames)
        {
            var errors = new List<string>();

            if (preview is null)
            {
                errors.Add("لم يتم توليد بيانات المهام.");

                return errors;
            }

            if (preview.Tasks is null)
            {
                errors.Add("قائمة المهام المقترحة غير موجودة.");

                return errors;
            }

            if (preview.Tasks.Count is < 1 or > 4)
            {
                errors.Add(
                    "يجب أن يكون عدد المهام المقترحة بين مهمة واحدة و4 مهام.");
            }

            ValidateDuplicateGeneratedNames(
                preview.Tasks,
                errors);

            ValidateExistingTaskDuplicates(
                preview.Tasks,
                existingTaskNames,
                errors);

            for (var index = 0;
                 index < preview.Tasks.Count;
                 index++)
            {
                ValidateTask(
                    preview.Tasks[index],
                    index + 1,
                    initiativeStartDate,
                    initiativeEndDate,
                    errors);
            }

            return errors;
        }

        private static void ValidateTask(
            GeneratedTaskPreview task,
            int taskNumber,
            DateTime initiativeStartDate,
            DateTime? initiativeEndDate,
            List<string> errors)
        {
            if (task is null)
            {
                errors.Add(
                    $"المهمة رقم {taskNumber} غير صالحة.");

                return;
            }

            if (IsInvalidText(task.Name))
            {
                errors.Add(
                    $"اسم المهمة رقم {taskNumber} مطلوب.");
            }

            if (IsInvalidText(task.Description))
            {
                errors.Add(
                    $"وصف المهمة رقم {taskNumber} مطلوب.");
            }

            if (task.StartDate == default)
            {
                errors.Add(
                    $"تاريخ بداية المهمة رقم {taskNumber} غير صالح.");
            }

            if (task.EndDate is null ||
                task.EndDate == default)
            {
                errors.Add(
                    $"تاريخ نهاية المهمة رقم {taskNumber} غير صالح.");
            }
            else
            {
                if (task.EndDate < task.StartDate)
                {
                    errors.Add(
                        $"تاريخ نهاية المهمة رقم {taskNumber} " +
                        "يسبق تاريخ بدايتها.");
                }

                if (initiativeEndDate is not null &&
                    task.EndDate > initiativeEndDate)
                {
                    errors.Add(
                        $"المهمة رقم {taskNumber} تنتهي بعد نهاية المبادرة.");
                }
            }

            if (task.StartDate < initiativeStartDate)
            {
                errors.Add(
                    $"المهمة رقم {taskNumber} تبدأ قبل بداية المبادرة.");
            }

            if (!IsValidColor(task.Color))
            {
                errors.Add(
                    $"لون المهمة رقم {taskNumber} غير صالح.");
            }

            if (IsInvalidText(task.Icon))
            {
                errors.Add(
                    $"أيقونة المهمة رقم {taskNumber} مطلوبة.");
            }
        }

        private static void ValidateDuplicateGeneratedNames(
            IReadOnlyCollection<GeneratedTaskPreview> tasks,
            List<string> errors)
        {
            var duplicateNames = tasks
                .Where(task =>
                    task is not null &&
                    !IsInvalidText(task.Name))
                .GroupBy(
                    task => NormalizeName(task.Name),
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.First().Name.Trim())
                .ToList();

            foreach (var duplicateName in duplicateNames)
            {
                errors.Add(
                    $"اسم المهمة مكرر ضمن المهام المقترحة: {duplicateName}");
            }
        }

        private static void ValidateExistingTaskDuplicates(
            IReadOnlyCollection<GeneratedTaskPreview> generatedTasks,
            IReadOnlyCollection<string> existingTaskNames,
            List<string> errors)
        {
            var normalizedExistingNames = existingTaskNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(NormalizeName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var generatedTask in generatedTasks)
            {
                if (generatedTask is null ||
                    IsInvalidText(generatedTask.Name))
                {
                    continue;
                }

                var normalizedGeneratedName =
                    NormalizeName(generatedTask.Name);

                if (normalizedExistingNames.Contains(
                        normalizedGeneratedName))
                {
                    errors.Add(
                        $"المهمة موجودة مسبقًا ضمن المبادرة: " +
                        $"{generatedTask.Name.Trim()}");
                }
            }
        }

        private static string NormalizeName(string value)
        {
            return string.Join(
                    ' ',
                    value.Trim()
                        .Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries |
                            StringSplitOptions.TrimEntries))
                .ToLowerInvariant();
        }

        private static bool IsInvalidText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var normalized = value.Trim();

            return normalized.Equals(
                       "null",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   normalized.Equals(
                       "undefined",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsValidColor(string? color)
        {
            return !string.IsNullOrWhiteSpace(color)
                   &&
                   HexColorRegex.IsMatch(color.Trim());
        }
    }
}

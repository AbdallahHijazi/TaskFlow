using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.InitiativeGeneration;

namespace TaskFlow.Application.Features.AI.InitiativeGeneration.Validators
{
    public static class GeneratedInitiativePreviewValidator
    {
        private static readonly Regex HexColorRegex =
            new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        public static List<string> Validate(
            GeneratedInitiativePreview initiative)
        {
            var errors = new List<string>();

            if (initiative is null)
            {
                errors.Add("لم يتم توليد بيانات المبادرة.");
                return errors;
            }

            ValidateInitiative(initiative, errors);
            ValidateTasks(initiative, errors);

            return errors;
        }

        private static void ValidateInitiative(
            GeneratedInitiativePreview initiative,
            List<string> errors)
        {
            if (IsInvalidText(initiative.Name))
            {
                errors.Add("اسم المبادرة مطلوب.");
            }

            if (IsInvalidText(initiative.Description))
            {
                errors.Add("وصف المبادرة مطلوب.");
            }

            if (initiative.StartDate == default)
            {
                errors.Add("تاريخ بداية المبادرة غير صالح.");
            }

            if (initiative.EndDate is null ||
                initiative.EndDate == default)
            {
                errors.Add("تاريخ نهاية المبادرة غير صالح.");
            }
            else if (initiative.EndDate < initiative.StartDate)
            {
                errors.Add(
                    "تاريخ نهاية المبادرة يجب أن يكون بعد تاريخ البداية.");
            }

            if (!IsValidColor(initiative.Color))
            {
                errors.Add("لون المبادرة غير صالح.");
            }

            if (IsInvalidText(initiative.Icon))
            {
                errors.Add("أيقونة المبادرة مطلوبة.");
            }
        }

        private static void ValidateTasks(
            GeneratedInitiativePreview initiative,
            List<string> errors)
        {
            if (initiative.Tasks is null ||
                initiative.Tasks.Count == 0)
            {
                errors.Add("يجب توليد مهمة واحدة على الأقل.");
                return;
            }

            if (initiative.Tasks.Count is < 3 or > 8)
            {
                errors.Add(
                    "يجب أن يكون عدد المهام بين 3 و8 مهام.");
            }

            var duplicateNames = initiative.Tasks
                .Where(task => !IsInvalidText(task.Name))
                .GroupBy(
                    task => task.Name.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            foreach (var duplicateName in duplicateNames)
            {
                errors.Add(
                    $"اسم المهمة مكرر: {duplicateName}");
            }

            for (var index = 0;
                 index < initiative.Tasks.Count;
                 index++)
            {
                var task = initiative.Tasks[index];
                var taskNumber = index + 1;

                if (task is null)
                {
                    errors.Add(
                        $"المهمة رقم {taskNumber} غير صالحة.");

                    continue;
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
                            $"تاريخ نهاية المهمة رقم {taskNumber} يسبق تاريخ بدايتها.");
                    }

                    if (initiative.EndDate is not null &&
                        task.EndDate > initiative.EndDate)
                    {
                        errors.Add(
                            $"المهمة رقم {taskNumber} تنتهي بعد نهاية المبادرة.");
                    }
                }

                if (task.StartDate < initiative.StartDate)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs.AI.RiskAnalysis;

namespace TaskFlow.Application.Features.AI.RiskAnalysis.Validators
{
    public static class RiskAnalysisResponseValidator
    {
        private static readonly HashSet<string> AllowedSeverities =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "Low",
            "Medium",
            "High",
            "Critical"
            };

        private static readonly HashSet<string> AllowedProbabilities =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "Low",
            "Medium",
            "High"
            };

        public static List<string> Validate(
            RiskAnalysisResponse response)
        {
            var errors = new List<string>();

            if (response is null)
            {
                errors.Add("لم يتم إرجاع نتيجة تحليل المخاطر.");
                return errors;
            }

            if (IsInvalidText(response.Summary))
            {
                errors.Add("ملخص تحليل المخاطر مطلوب.");
            }

            if (response.Risks is null)
            {
                errors.Add("قائمة المخاطر غير موجودة.");
                return errors;
            }

            ValidateDuplicateRiskTitles(
                response.Risks,
                errors);

            for (var index = 0;
                 index < response.Risks.Count;
                 index++)
            {
                ValidateRisk(
                    response.Risks[index],
                    index + 1,
                    errors);
            }

            return errors;
        }

        private static void ValidateRisk(
            RiskAnalysisItem? risk,
            int riskNumber,
            List<string> errors)
        {
            if (risk is null)
            {
                errors.Add(
                    $"الخطر رقم {riskNumber} غير صالح.");

                return;
            }

            if (IsInvalidText(risk.Title))
            {
                errors.Add(
                    $"عنوان الخطر رقم {riskNumber} مطلوب.");
            }

            if (IsInvalidText(risk.Description))
            {
                errors.Add(
                    $"وصف الخطر رقم {riskNumber} مطلوب.");
            }

            if (IsInvalidText(risk.Probability))
            {
                errors.Add(
                    $"احتمالية الخطر رقم {riskNumber} مطلوبة.");
            }
            else if (!AllowedProbabilities.Contains(
                         risk.Probability.Trim()))
            {
                errors.Add(
                    $"احتمالية الخطر رقم {riskNumber} غير صالحة. " +
                    "القيم المسموحة: Low, Medium, High.");
            }

            if (IsInvalidText(risk.Impact))
            {
                errors.Add(
                    $"تأثير الخطر رقم {riskNumber} مطلوب.");
            }

            if (IsInvalidText(risk.Severity))
            {
                errors.Add(
                    $"درجة خطورة الخطر رقم {riskNumber} مطلوبة.");
            }
            else if (!AllowedSeverities.Contains(
                         risk.Severity.Trim()))
            {
                errors.Add(
                    $"درجة خطورة الخطر رقم {riskNumber} غير صالحة. " +
                    "القيم المسموحة: Low, Medium, High, Critical.");
            }

            if (risk.AffectedTasks is null)
            {
                errors.Add(
                    $"قائمة المهام المتأثرة للخطر رقم {riskNumber} غير موجودة.");
            }

            if (IsInvalidText(risk.Recommendation))
            {
                errors.Add(
                    $"التوصية الخاصة بالخطر رقم {riskNumber} مطلوبة.");
            }
        }

        private static void ValidateDuplicateRiskTitles(
            IReadOnlyCollection<RiskAnalysisItem> risks,
            List<string> errors)
        {
            var duplicateTitles = risks
                .Where(risk =>
                    risk is not null &&
                    !IsInvalidText(risk.Title))
                .GroupBy(
                    risk => NormalizeText(risk.Title),
                    StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group =>
                    group.First().Title.Trim())
                .ToList();

            foreach (var duplicateTitle in duplicateTitles)
            {
                errors.Add(
                    $"الخطر مكرر في نتيجة التحليل: {duplicateTitle}");
            }
        }

        private static bool IsInvalidText(
            string? value)
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

        private static string NormalizeText(
            string value)
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
    }
}

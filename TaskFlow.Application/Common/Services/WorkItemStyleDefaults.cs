using System.Text.RegularExpressions;

namespace TaskFlow.Application.Common.Services;

public static partial class WorkItemStyleDefaults
{
    public const string InitiativeColor = "#7C5CFF";
    public const string InitiativeIcon = "ti ti-target-arrow";
    public const string TaskColor = "#2F80ED";
    public const string TaskIcon = "ti ti-checklist";

    private static readonly HashSet<string> InitiativeIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        "rocket", "bulb", "target-arrow", "chart-line", "briefcase", "sparkles"
    };

    private static readonly HashSet<string> TaskIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        "checklist", "clipboard-list", "calendar-event", "flag", "target-arrow", "bolt"
    };

    public static (string Color, string Icon) ForInitiative(
        string? name, string? description, string? color, string? icon)
    {
        var text = $"{name} {description}".ToLowerInvariant();
        var suggested = text switch
        {
            _ when Contains(text, "school", "education", "training", "مدرس", "تعليم", "تدريب") => ("#F59E0B", "ti ti-bulb"),
            _ when Contains(text, "health", "clinic", "medical", "صحة", "عياد", "طبي") => ("#EF4444", "ti ti-sparkles"),
            _ when Contains(text, "technology", "system", "platform", "app", "تقني", "نظام", "منصة", "تطبيق") => ("#2563EB", "ti ti-rocket"),
            _ when Contains(text, "finance", "budget", "مالي", "ميزاني") => ("#059669", "ti ti-chart-line"),
            _ when Contains(text, "construction", "building", "بناء", "إنشاء", "اعمار", "إعمار") => ("#D97706", "ti ti-briefcase"),
            _ when Contains(text, "marketing", "campaign", "تسويق", "حملة") => ("#DB2777", "ti ti-sparkles"),
            _ => (InitiativeColor, InitiativeIcon)
        };

        return (
            HasCustomColor(color) ? color!.Trim() : suggested.Item1,
            ResolveIcon(icon, InitiativeIcons, suggested.Item2, "initiative"));
    }

    public static (string Color, string Icon) ForTask(
        string? name, string? description, string? color, string? icon)
    {
        var text = $"{name} {description}".ToLowerInvariant();
        var suggested = text switch
        {
            _ when Contains(text, "research", "analy", "inspect", "بحث", "تحليل", "فحص") => ("#7C3AED", "ti ti-clipboard-list"),
            _ when Contains(text, "design", "تصميم") => ("#DB2777", "ti ti-bolt"),
            _ when Contains(text, "develop", "build", "implement", "تطوير", "برمجة", "تنفيذ") => ("#2563EB", "ti ti-checklist"),
            _ when Contains(text, "test", "quality", "اختبار", "جودة") => ("#DC2626", "ti ti-flag"),
            _ when Contains(text, "document", "report", "توثيق", "تقرير") => ("#64748B", "ti ti-clipboard-list"),
            _ when Contains(text, "purchase", "procure", "شراء", "توريد") => ("#D97706", "ti ti-checklist"),
            _ when Contains(text, "repair", "maintenance", "إصلاح", "صيانة", "ترميم") => ("#EA580C", "ti ti-bolt"),
            _ when Contains(text, "meeting", "team", "اجتماع", "فريق") => ("#0891B2", "ti ti-calendar-event"),
            _ => (TaskColor, TaskIcon)
        };

        return (
            HasCustomColor(color) ? color!.Trim() : suggested.Item1,
            ResolveIcon(icon, TaskIcons, suggested.Item2, "task"));
    }

    private static bool Contains(string text, params string[] terms) => terms.Any(text.Contains);

    private static bool ValidColor(string? value) =>
        !string.IsNullOrWhiteSpace(value) && HexColor().IsMatch(value.Trim());

    private static bool HasCustomColor(string? value) =>
        ValidColor(value) &&
        !value!.Trim().Equals("#FFFFFF", StringComparison.OrdinalIgnoreCase) &&
        !value.Trim().Equals("#4F46E5", StringComparison.OrdinalIgnoreCase);

    private static string ResolveIcon(
        string? value,
        IReadOnlySet<string> allowedIcons,
        string fallback,
        string genericIcon)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;

        var icon = value.Trim();
        if (icon.StartsWith("ti ti-", StringComparison.OrdinalIgnoreCase)) icon = icon[6..];
        if (icon.Equals(genericIcon, StringComparison.OrdinalIgnoreCase)) return fallback;

        return allowedIcons.Contains(icon) ? $"ti ti-{icon}" : fallback;
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColor();
}

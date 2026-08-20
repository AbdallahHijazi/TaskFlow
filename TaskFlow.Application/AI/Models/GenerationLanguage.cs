using System.Text.RegularExpressions;

namespace TaskFlow.Application.AI.Models;

public enum GenerationLanguage { Arabic, English }

public static partial class GenerationLanguageDetector
{
    public static GenerationLanguage Detect(string? text) =>
        !string.IsNullOrWhiteSpace(text) && ArabicRegex().IsMatch(text)
            ? GenerationLanguage.Arabic : GenerationLanguage.English;

    public static string Name(GenerationLanguage language) =>
        language == GenerationLanguage.Arabic ? "Arabic" : "English";

    public static bool Matches(string? text, GenerationLanguage language)
    {
        if (string.IsNullOrWhiteSpace(text) || CjkRegex().IsMatch(text)) return false;
        return language == GenerationLanguage.Arabic
            ? ArabicRegex().IsMatch(text)
            : LatinRegex().IsMatch(text) && !ArabicRegex().IsMatch(text);
    }

    [GeneratedRegex("[\\u0600-\\u06FF]")]
    private static partial Regex ArabicRegex();
    [GeneratedRegex("[A-Za-z]")]
    private static partial Regex LatinRegex();
    [GeneratedRegex("[\\u3400-\\u4DBF\\u4E00-\\u9FFF\\uF900-\\uFAFF]")]
    private static partial Regex CjkRegex();
}

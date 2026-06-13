
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MonsterTools.Core;

public interface IToolArgumentNormalizer
{
    Dictionary<string, object?> Normalize(string toolName, Dictionary<string, object?> sourceArguments);
}

public sealed class ToolArgumentNormalizer : IToolArgumentNormalizer
{
    public Dictionary<string, object?> Normalize(string toolName, Dictionary<string, object?> sourceArguments)
    {
        var cleanMap = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (sourceArguments is null || sourceArguments.Count == 0)
            return cleanMap;

        foreach (var (key, value) in sourceArguments)
        {
            if (string.IsNullOrWhiteSpace(key) || value is null)
                continue;

            cleanMap[key.Trim()] = value switch
            {
                string s => SanitizeString(s),
                JsonElement json when json.ValueKind == JsonValueKind.String => SanitizeString(json.GetString() ?? string.Empty),
                JsonElement json when json.ValueKind == JsonValueKind.Number => json.TryGetInt64(out var l) ? l : json.TryGetDouble(out var d) ? d : json.ToString(),
                JsonElement json when json.ValueKind == JsonValueKind.True => true,
                JsonElement json when json.ValueKind == JsonValueKind.False => false,
                _ => value
            };
        }

        return cleanMap;
    }

    private static string SanitizeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sanitized = input
            .Trim()
            .Trim('"', '\'')
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ")
            .Replace("&&", " ")
            .Replace("||", " ")
            .Replace(";", " ")
            .Replace("|", " ")
            .Replace("`", string.Empty);

        sanitized = Regex.Replace(sanitized, @"\s+", " ");
        return sanitized.Trim();
    }
}

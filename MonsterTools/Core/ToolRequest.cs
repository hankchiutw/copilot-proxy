namespace MonsterTools.Core;

public sealed class ToolRequest
{
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, object?> Arguments { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string ExecutionContextPath { get; set; } = string.Empty;

    public ToolRequest() { }

    public ToolRequest(Dictionary<string, object?> arguments)
    {
        Arguments = arguments ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    public T? Get<T>(string key)
    {
        if (!Arguments.TryGetValue(key, out var value) || value is null)
            return default;

        if (value is T typed)
            return typed;

        try
        {
            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), value.ToString() ?? string.Empty, ignoreCase: true);

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }
}

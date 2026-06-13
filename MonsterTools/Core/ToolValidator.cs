namespace MonsterTools.Core;

public static class ToolValidator
{
    public static ToolResult Validate(string toolName, IReadOnlyDictionary<string, object?> args)
    {
        if (!ToolSchemas.Tools.TryGetValue(toolName, out var schema))
            return ToolResult.Failure($"Unknown tool: {toolName}");

        foreach (var required in schema.Required)
        {
            if (!args.ContainsKey(required) || args[required] is null || string.IsNullOrWhiteSpace(args[required]?.ToString()))
                return ToolResult.Failure($"Missing required argument: {required}");
        }

        return ToolResult.Ok("valid");
    }
}

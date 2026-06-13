namespace MonsterTools.Core;

public static class ToolSchemas
{
    public static readonly IReadOnlyDictionary<string, ToolDefinition> Tools =
        new Dictionary<string, ToolDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["workspace_scan"] = new() { Description = "Scan workspace structure", Optional = ["path"] },
            ["read_file"] = new() { Description = "Read file contents", Required = ["path"] },
            ["write_file"] = new() { Description = "Write file contents", Required = ["path", "content"] },
            ["patch_file"] = new() { Description = "Patch text within a file", Required = ["path", "find", "replace"] },
            ["search"] = new() { Description = "Search workspace files", Required = ["pattern"], Optional = ["workspaceRoot"] },
            ["ast"] = new() { Description = "Inspect source structure", Required = ["path"] },
            ["build"] = new() { Description = "Run dotnet build", Required = ["path"] },
            ["compiler"] = new() { Description = "Run compiler diagnostics", Required = ["path"] },
            ["test"] = new() { Description = "Run dotnet test", Required = ["path"] },
            ["git"] = new() { Description = "Run git command", Optional = ["path", "command"] },
            ["validate"] = new() { Description = "Validate tool inputs", Optional = ["input"] },
            ["filesystem"] = new() { Description = "Create or update files in workspace.", Required = ["path"], Optional = ["content"] },
        };
}

public sealed class ToolDefinition
{
    public string Description { get; init; } = string.Empty;
    public string[] Required { get; init; } = [];
    public string[] Optional { get; init; } = [];
}

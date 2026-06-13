namespace MonsterTools.Core;

public sealed class McpRequest
{
    public string id { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string tool { get; set; } = string.Empty;
    public Dictionary<string, object?> args { get; set; } = new();
}

public sealed class McpResponse
{
    public string id { get; set; } = string.Empty;
    public string type { get; set; } = "tool.result";
    public bool success { get; set; }
    public object? result { get; set; }
}

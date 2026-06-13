namespace MonsterTools.Contracts;

public sealed class ToolExecuteResponse
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

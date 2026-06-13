namespace MonsterTools.Core;

public sealed class ToolResult
{
    public bool Success { get; init; }
    public string Output { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;

    public static ToolResult Ok(string output) => new() { Success = true, Output = output };
    public static ToolResult SuccessResult(string output) => Ok(output);
    public static ToolResult Failure(string error) => new() { Success = false, Error = error };
}

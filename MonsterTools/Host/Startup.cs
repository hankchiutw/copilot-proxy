namespace MonsterTools.Host;

public sealed class Startup
{
    public string WorkspaceRoot { get; init; } = Directory.GetCurrentDirectory();
    public string LMStudioUrl { get; init; } = "http://127.0.0.1:1234";
    public string ModelName { get; init; } = "ibm/granite-4-h-tiny";
}

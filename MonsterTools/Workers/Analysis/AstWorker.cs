using System.Text.Json;
using System.Text.RegularExpressions;
using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Analysis;

public sealed class AstWorker : ToolWorkerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly WorkspaceService _workspace;

    public override string Name => "ast";
    public override string Description => "Summarize source structure.";

    public AstWorker(WorkspaceService workspace) => _workspace = workspace;

    protected override Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var path = request.Get<string>("path");
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(ToolResult.Failure("Missing path"));

            var fullPath = _workspace.GetSafePath(path);
            if (!File.Exists(fullPath))
                return Task.FromResult(ToolResult.Failure("File not found"));

            var lines = File.ReadAllLines(fullPath);
            var summary = new
            {
                file = Path.GetRelativePath(_workspace.WorkspaceRoot, fullPath),
                lineCount = lines.Length,
                namespaceCount = lines.Count(line => line.TrimStart().StartsWith("namespace ", StringComparison.Ordinal)),
                classCount = lines.Count(IsClassLine),
                methodCount = lines.Count(IsMethodLine),
                preview = lines.Take(40)
            };

            return Task.FromResult(ToolResult.Ok(JsonSerializer.Serialize(summary, JsonOptions)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Failure(ex.Message));
        }
    }

    private static bool IsClassLine(string line)
    {
        var trimmed = line.TrimStart();
        return trimmed.StartsWith("class ", StringComparison.Ordinal)
            || trimmed.StartsWith("public class ", StringComparison.Ordinal)
            || trimmed.StartsWith("internal class ", StringComparison.Ordinal)
            || trimmed.StartsWith("sealed class ", StringComparison.Ordinal)
            || trimmed.Contains(" class ", StringComparison.Ordinal);
    }

    private static bool IsMethodLine(string line)
    {
        var trimmed = line.Trim();
        if (!trimmed.Contains('(') || !trimmed.Contains(')'))
            return false;

        return Regex.IsMatch(
            trimmed,
            @"^(public|private|internal|protected|static|virtual|override|async|sealed|partial|\s)+[\w<>\[\], ?]+\s+\w+\s*\(.*\)\s*(\{|=>)",
            RegexOptions.CultureInvariant);
    }
}

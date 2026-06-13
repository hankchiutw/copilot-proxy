
using System.Diagnostics;
using System.Text.Json;
using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Build;

public sealed class CompilerWorker : ToolWorkerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly BuildService _buildService;
    private readonly WorkspaceService _workspace;

    public override string Name => "compiler";
    public override string Description => "Run dotnet build and summarize compiler diagnostics.";

    public CompilerWorker(BuildService buildService, WorkspaceService workspace)
    {
        _buildService = buildService;
        _workspace = workspace;
    }

    protected override async Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var workingDirectory = ResolveWorkingDirectory(request.Get<string>("path"));
            if (!Directory.Exists(workingDirectory))
                return ToolResult.Failure($"Workspace directory not found: {workingDirectory}");

            var output = await _buildService.BuildAsync(workingDirectory, cancellationToken);
            var diagnostics = ExtractDiagnostics(output);

            var payload = new
            {
                success = diagnostics.errors.Count == 0,
                workingDirectory,
                errors = diagnostics.errors,
                warnings = diagnostics.warnings,
                rawOutput = diagnostics.errors.Count == 0 ? output : string.Empty
            };

            return ToolResult.Ok(JsonSerializer.Serialize(payload, JsonOptions));
        }
        catch (Exception ex)
        {
            return ToolResult.Failure(ex.Message);
        }
    }

    private string ResolveWorkingDirectory(string? requestedPath)
    {
        if (string.IsNullOrWhiteSpace(requestedPath))
            return _workspace.WorkspaceRoot;

        var normalized = requestedPath.Trim();
        if (Directory.Exists(normalized))
            return Path.GetFullPath(normalized);

        var fileDirectory = Path.GetDirectoryName(normalized);
        if (!string.IsNullOrWhiteSpace(fileDirectory) && Directory.Exists(fileDirectory))
            return Path.GetFullPath(fileDirectory);

        return _workspace.WorkspaceRoot;
    }

    private static (List<string> errors, List<string> warnings) ExtractDiagnostics(string output)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var line in output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Contains(": error ", StringComparison.OrdinalIgnoreCase))
                errors.Add(line.Trim());
            else if (line.Contains(": warning ", StringComparison.OrdinalIgnoreCase))
                warnings.Add(line.Trim());
        }

        return (errors, warnings);
    }
}

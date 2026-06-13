
using System.Diagnostics;
using System.Text.Json;
using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Build;

public sealed class TestWorker : ToolWorkerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly BuildService _buildService;
    private readonly WorkspaceService _workspace;

    public override string Name => "test";
    public override string Description => "Run dotnet test and summarize failures.";

    public TestWorker(BuildService buildService, WorkspaceService workspace)
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

            var output = await RunDotnetAsync(workingDirectory, "test --nologo", cancellationToken);
            var diagnostics = ExtractDiagnostics(output);

            var payload = new
            {
                success = diagnostics.failures.Count == 0,
                workingDirectory,
                failures = diagnostics.failures,
                tests = diagnostics.tests,
                rawOutput = diagnostics.failures.Count == 0 ? output : string.Empty
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

    private static async Task<string> RunDotnetAsync(string workingDirectory, string arguments, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet.");
        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return stdout + Environment.NewLine + stderr;
    }

    private static (List<string> failures, List<string> tests) ExtractDiagnostics(string output)
    {
        var failures = new List<string>();
        var tests = new List<string>();

        foreach (var line in output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                line.Contains(": error ", StringComparison.OrdinalIgnoreCase))
            {
                failures.Add(line.Trim());
            }
            else if (line.Contains("Passed!", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("test(s) passed", StringComparison.OrdinalIgnoreCase))
            {
                tests.Add(line.Trim());
            }
        }

        return (failures, tests);
    }
}

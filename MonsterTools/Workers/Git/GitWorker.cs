
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Git;

public sealed class GitWorker : ToolWorkerBase
{
    private static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "status",
        "diff",
        "log",
        "branch",
        "rev-parse",
        "show",
        "add",
        "commit",
        "pull",
        "push",
        "restore",
        "checkout"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly WorkspaceService _workspace;

    public override string Name => "git";
    public override string Description => "Run safe git commands.";

    public GitWorker(WorkspaceService workspace) => _workspace = workspace;

    protected override async Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var requestedPath = request.Get<string>("path");
            var workingDirectory = ResolveWorkingDirectory(requestedPath);

            if (!Directory.Exists(workingDirectory))
                return ToolResult.Failure($"Git working directory not found: {workingDirectory}");

            var commandText = request.Get<string>("command") ?? "status";
            var tokens = SplitCommand(commandText);
            if (tokens.Count == 0)
                tokens.Add("status");

            var verb = tokens[0];
            if (!AllowedCommands.Contains(verb))
                return ToolResult.Failure($"Unsupported git command: {verb}");

            var psi = new ProcessStartInfo
            {
                FileName = "git",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            psi.ArgumentList.Add(verb);
            for (var i = 1; i < tokens.Count; i++)
                psi.ArgumentList.Add(tokens[i]);

            var envBranch = request.Get<string>("branch");
            if (!string.IsNullOrWhiteSpace(envBranch) && verb.Equals("checkout", StringComparison.OrdinalIgnoreCase))
                psi.ArgumentList.Add(envBranch);

            using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start git.");
            var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var payload = new
            {
                success = process.ExitCode == 0,
                command = commandText,
                workingDirectory,
                stdout = stdout.Trim(),
                stderr = stderr.Trim(),
                exitCode = process.ExitCode
            };

            return process.ExitCode == 0
                ? ToolResult.Ok(JsonSerializer.Serialize(payload, JsonOptions))
                : ToolResult.Failure(JsonSerializer.Serialize(payload, JsonOptions));
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

    private static List<string> SplitCommand(string commandText)
    {
        var tokens = new List<string>();
        foreach (Match match in Regex.Matches(commandText, @"[""'].*?[""']|[^ \t]+"))
        {
            var token = match.Value.Trim();
            if (token.Length >= 2 && ((token.StartsWith('"') && token.EndsWith('"')) || (token.StartsWith('\'') && token.EndsWith('\''))))
                token = token[1..^1];

            if (!string.IsNullOrWhiteSpace(token))
                tokens.Add(token);
        }
        return tokens;
    }
}

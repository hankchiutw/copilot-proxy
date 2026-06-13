using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Analysis;

public class SearchWorker : ToolWorkerBase
{
    private readonly WorkspaceService _workspace;

    public override string Name => "search";

    public override string Description => "Search workspace for text.";

    public SearchWorker(WorkspaceService workspace)
    {
        _workspace = workspace;
    }

    protected override Task<ToolResult> ExecuteCoreAsync(
        ToolRequest request,
        CancellationToken cancellationToken)
    {
        var pattern = request.Get<string>("pattern");

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return Task.FromResult(
                ToolResult.Failure("Missing pattern"));
        }

        var hits = new List<string>();

        var files = _workspace.ScanWorkspace("*.*");

        foreach (var file in files)
        {
            try
            {
                var lineNo = 0;

                foreach (var line in File.ReadLines(file))
                {
                    lineNo++;

                    if (line.Contains(
                        pattern,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        hits.Add(
                            $"{Path.GetRelativePath(_workspace.WorkspaceRoot, file)}:{lineNo}: {line.Trim()}");

                        if (hits.Count >= 100)
                            break;
                    }
                }

                if (hits.Count >= 100)
                    break;
            }
            catch
            {
                // ignore unreadable files
            }
        }

        return Task.FromResult(
            ToolResult.Ok(
                hits.Count == 0
                    ? $"No matches for '{pattern}'."
                    : string.Join(Environment.NewLine, hits)));
    }
}
using System.Text;

namespace MonsterTools.Services;

public sealed class WorkspaceService
{
    public string WorkspaceRoot { get; }

    public WorkspaceService(string? workspaceRoot = null)
    {
        WorkspaceRoot = string.IsNullOrWhiteSpace(workspaceRoot)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(workspaceRoot);
    }

    public string ResolveRoot(string? workspaceRoot) =>
        string.IsNullOrWhiteSpace(workspaceRoot) ? WorkspaceRoot : Path.GetFullPath(workspaceRoot);

    public string GetSafePath(string relativeOrAbsolutePath)
    {
        var full = Path.GetFullPath(Path.IsPathRooted(relativeOrAbsolutePath)
            ? relativeOrAbsolutePath
            : Path.Combine(WorkspaceRoot, relativeOrAbsolutePath));

        if (!full.StartsWith(WorkspaceRoot, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException($"Path escapes workspace root: {relativeOrAbsolutePath}");

        return full;
    }

    public IEnumerable<string> ScanWorkspace(string? pattern = null)
    {
        pattern ??= "*.*";
        if (!Directory.Exists(WorkspaceRoot)) return Enumerable.Empty<string>();

        return Directory.EnumerateFiles(WorkspaceRoot, pattern, SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }

    public bool FileExists(string path) => File.Exists(GetSafePath(path));

    public string ReadFile(string path)
    {
        var full = GetSafePath(path);
        return File.ReadAllText(full, Encoding.UTF8);
    }

    public void WriteFile(string path, string content)
    {
        var full = GetSafePath(path);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(full, content, Encoding.UTF8);
    }

    public void PatchFile(string path, string find, string replace)
    {
        var content = ReadFile(path);
        content = content.Replace(find, replace, StringComparison.Ordinal);
        WriteFile(path, content);
    }
}

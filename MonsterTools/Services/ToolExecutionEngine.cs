using System.Text.Json;
using MonsterTools.Core;

namespace MonsterTools.Services;

public sealed class ToolExecutionEngine
{
    private readonly WorkerDispatcher _dispatcher;

    public ToolExecutionEngine(WorkerDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<string> ExecuteAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = JsonSerializer.Deserialize<ToolExecutionEnvelope>(jsonPayload, JsonOptions()) ?? throw new JsonException("Payload deserialized to null.");
            if (string.IsNullOrWhiteSpace(request.ToolName))
                return JsonSerializer.Serialize(new { success = false, error = "Missing toolName." });

            var result = await _dispatcher.DispatchAsync(request.ToolName, request.Arguments, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = result.Success,
                output = result.Output,
                error = result.Error
            }, JsonOptions());
        }
        catch (JsonException ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"JsonException: {ex.Message}"
            }, JsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            }, JsonOptions());
        }
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private sealed class ToolExecutionEnvelope
    {
        public string ToolName { get; set; } = string.Empty;
        public Dictionary<string, object?> Arguments { get; set; } = new();
    }
}

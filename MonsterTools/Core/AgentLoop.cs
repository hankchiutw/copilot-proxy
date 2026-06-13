using System.Text.Json;
using System.Text.RegularExpressions;
using MonsterTools.Services;

namespace MonsterTools.Core;

public sealed class AgentLoop
{
    private const int MaxIterations = 4;

    private readonly ILMStudioService _lmStudio;
    private readonly ToolRouter _toolRouter;
    private readonly ToolArgumentNormalizer _normalizer;

    public AgentLoop(
        ILMStudioService lmStudio,
        ToolRouter toolRouter,
        ToolArgumentNormalizer normalizer)
    {
        _lmStudio = lmStudio;
        _toolRouter = toolRouter;
        _normalizer = normalizer;
    }

    public async Task<ToolResult> RunAsync(
        string prompt,
        string workspaceRoot,
        CancellationToken cancellationToken = default)
    {
        var history = new List<ChatMessageContext>
        {
            new()
            {
                Role = "system",
                Content = """
                          You are MonsterTools, a deterministic coding assistant.
                          When a tool is needed, return ONLY JSON with this shape:
                          {
                            "id":"call-1",
                            "type":"function",
                            "function":{
                              "name":"tool_name",
                              "arguments":"{ "key": "value" }"
                            }
                          }
                          When no tool is needed, return a concise final answer.
                          """
            },
            new()
            {
                Role = "user",
                Content = JsonSerializer.Serialize(new
                {
                    prompt,
                    workspaceRoot
                })
            }
        };

        string lastResponse = string.Empty;

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            lastResponse = await _lmStudio.QueryModelHistoryAsync(history, cancellationToken);

            if (!TryParseToolCall(lastResponse, out var call))
                return ToolResult.Ok(lastResponse);

            var args = ParseArguments(call.Function.Arguments);
            args = _normalizer.Normalize(call.Function.Name, args);
            args["workspaceRoot"] = workspaceRoot;

            var request = new ToolRequest(args)
            {
                ToolName = call.Function.Name,
                ExecutionContextPath = workspaceRoot
            };

            var toolResult = await _toolRouter.ExecuteAsync(call.Function.Name, request, cancellationToken);

            history.Add(new ChatMessageContext
            {
                Role = "assistant",
                Content = lastResponse
            });

            history.Add(new ChatMessageContext
            {
                Role = "tool",
                Content = JsonSerializer.Serialize(new
                {
                    tool = call.Function.Name,
                    success = toolResult.Success,
                    output = toolResult.Output,
                    error = toolResult.Error
                })
            });

            if (!toolResult.Success)
                return toolResult;
        }

        return ToolResult.Ok(lastResponse);
    }

    public Task<ToolResult> RunToolDirectAsync(
        string toolName,
        Dictionary<string, object?> args,
        CancellationToken cancellationToken = default)
        => _toolRouter.ExecuteAsync(toolName, new ToolRequest(args), cancellationToken);

    private static bool TryParseToolCall(string text, out ToolCall call)
    {
        call = new ToolCall();
        var cleaned = ExtractJsonBlock(text);
        if (string.IsNullOrWhiteSpace(cleaned))
            return false;

        try
        {
            var parsed = JsonSerializer.Deserialize<ToolCall>(cleaned, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (parsed is null || string.IsNullOrWhiteSpace(parsed.Function.Name))
                return false;

            call = parsed;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ExtractJsonBlock(string input)
    {
        var fenced = Regex.Match(input, @"```json\s*(?<json>\{.*?\})\s*```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (fenced.Success)
            return fenced.Groups["json"].Value;

        var trimmed = input.Trim();
        return trimmed.StartsWith('{') && trimmed.EndsWith('}') ? trimmed : string.Empty;
    }

    private static Dictionary<string, object?> ParseArguments(string json)
    {
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                string.IsNullOrWhiteSpace(json) ? "{}" : json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new Dictionary<string, JsonElement>();

            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var (key, value) in dict)
            {
                result[key] = value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number when value.TryGetInt64(out var i) => i,
                    JsonValueKind.Number when value.TryGetDouble(out var d) => d,
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => value.ToString()
                };
            }

            return result;
        }
        catch
        {
            return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

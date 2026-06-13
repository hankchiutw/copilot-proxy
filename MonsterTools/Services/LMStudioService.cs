using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MonsterTools.Core;

namespace MonsterTools.Services;

public sealed class LMStudioService : ILMStudioService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public LMStudioService(string baseUrl, string model, HttpClient? httpClient = null)
        : this(httpClient ?? CreateDefaultClient(), baseUrl, model)
    {
    }

    public LMStudioService(HttpClient httpClient, string baseUrl, string model)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = NormalizeBaseUrl(baseUrl);
        _model = string.IsNullOrWhiteSpace(model) ? "ibm/granite-4-h-tiny" : model.Trim();
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"{_baseUrl}/v1/models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public Task<string> QueryModelAsync(string prompt, double temperature = 0.2, CancellationToken cancellationToken = default)
        => AskAsync(
            "You are a deterministic coding assistant. Return only the answer or a JSON tool call.",
            prompt,
            temperature,
            cancellationToken);

    public Task<string> QueryModelHistoryAsync(List<ChatMessageContext> conversationHistory, CancellationToken cancellationToken = default)
        => SendChatCompletionAsync(conversationHistory, 0.2, cancellationToken);

    public Task<string> AskAsync(string systemPrompt, string userPrompt, double temperature = 0.2, CancellationToken cancellationToken = default)
        => SendChatCompletionAsync(
            new List<ChatMessageContext>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userPrompt }
            },
            temperature,
            cancellationToken);

    private async Task<string> SendChatCompletionAsync(
        List<ChatMessageContext> conversationHistory,
        double temperature,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = _model,
            messages = conversationHistory.Select(message => new
            {
                role = message.Role,
                content = message.Content
            }),
            temperature,
            stream = false
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/v1/chat/completions",
            payload,
            JsonOptions,
            cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return JsonSerializer.Serialize(new
            {
                error = $"LM Studio returned {(int)response.StatusCode} ({response.ReasonPhrase ?? "unknown"}): {json}"
            }, JsonOptions);
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content ?? string.Empty;
        }
        catch
        {
            return json;
        }
    }

    private static HttpClient CreateDefaultClient()
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return "http://127.0.0.1:1234";

        return baseUrl.Trim().TrimEnd('/');
    }
}

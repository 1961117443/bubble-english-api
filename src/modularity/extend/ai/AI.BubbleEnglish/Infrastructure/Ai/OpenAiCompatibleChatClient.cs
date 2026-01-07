namespace AI.BubbleEnglish.Infrastructure.Ai;

using AI.BubbleEnglish.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

/// <summary>
/// Works with OpenAI and OpenAI-compatible providers (e.g., DeepSeek with compatible endpoint).
/// </summary>
public class OpenAiCompatibleChatClient : IAiChatClient
{
    private readonly HttpClient _http;
    private readonly OpenAiCompatibleProviderOptions _opt;

    public OpenAiCompatibleChatClient(string providerKey, HttpClient http, OpenAiCompatibleProviderOptions opt)
    {
        ProviderKey = providerKey;
        _http = http;
        _opt = opt;
    }

    public string ProviderKey { get; }

    public async Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken ct = default)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? _opt.DefaultModel : request.Model;

        var url = _opt.BaseUrl.TrimEnd('/') + "/v1/chat/completions";
        using var msg = new HttpRequestMessage(HttpMethod.Post, url);
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey);

        var payload = new
        {
            model,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            response_format = request.JsonOutput ? new { type = "json_object" } : null,
            messages = new object[]
            {
                new { role = "system", content = request.SystemPrompt ?? string.Empty },
                new { role = "user", content = request.UserPrompt ?? string.Empty }
            }
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        msg.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(msg, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"{ProviderKey} chat failed: {(int)resp.StatusCode} {raw}");

        using var doc = JsonDocument.Parse(raw);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        return new AiChatResult { Content = content, Raw = raw };
    }
}

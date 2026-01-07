namespace AI.BubbleEnglish.Infrastructure.Ai;

using AI.BubbleEnglish.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

/// <summary>
/// Baidu Qianfan (Wenxin Workshop) chat client.
/// Uses access_token obtained via API Key / Secret Key.
/// </summary>
public class QianfanChatClient : IAiChatClient
{
    private readonly HttpClient _http;
    private readonly QianfanProviderOptions _opt;
    private readonly IMemoryCache _cache;

    public QianfanChatClient(HttpClient http, QianfanProviderOptions opt, IMemoryCache cache)
    {
        ProviderKey = "qianfan";
        _http = http;
        _opt = opt;
        _cache = cache;
    }

    public string ProviderKey { get; }

    public async Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken ct = default)
    {
        var token = await GetAccessTokenAsync(ct);
        var endpoint = string.IsNullOrWhiteSpace(request.Model) ? _opt.DefaultModelEndpoint : request.Model;

        // Chat Completions endpoint format from Baidu Wenxin Workshop docs:
        // https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/{endpoint}?access_token=...
        var url = $"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/{endpoint}?access_token={token}";

        var payload = new
        {
            temperature = request.Temperature,
            max_output_tokens = request.MaxTokens,
            messages = new object[]
            {
                new { role = "user", content = (request.SystemPrompt ?? string.Empty) + "\n\n" + (request.UserPrompt ?? string.Empty) }
            }
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        using var resp = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"qianfan chat failed: {(int)resp.StatusCode} {raw}");

        using var doc = JsonDocument.Parse(raw);
        // qianfan returns {"result":"..."} for chat
        var content = doc.RootElement.TryGetProperty("result", out var r) ? (r.GetString() ?? "") : "";
        return new AiChatResult { Content = content, Raw = raw };
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        const string cacheKey = "bubble:qianfan:access_token";
        if (_cache.TryGetValue<string>(cacheKey, out var token) && !string.IsNullOrWhiteSpace(token))
            return token;

        if (string.IsNullOrWhiteSpace(_opt.ApiKey) || string.IsNullOrWhiteSpace(_opt.SecretKey))
            throw new InvalidOperationException("Qianfan ApiKey/SecretKey not configured");

        // Baidu OAuth token endpoint (client_credentials)
        // https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id=AK&client_secret=SK
        var url = $"https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={Uri.EscapeDataString(_opt.ApiKey)}&client_secret={Uri.EscapeDataString(_opt.SecretKey)}";
        using var resp = await _http.GetAsync(url, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"qianfan token failed: {(int)resp.StatusCode} {raw}");

        using var doc = JsonDocument.Parse(raw);
        token = doc.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
        var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var ex) ? ex.GetInt32() : 2592000;
        if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("qianfan token empty");

        // refresh a bit earlier
        _cache.Set(cacheKey, token, TimeSpan.FromSeconds(Math.Max(60, expiresIn - 300)));
        return token;
    }
}

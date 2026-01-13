namespace AI.BubbleEnglish.Infrastructure.Ai;

using AI.BubbleEnglish.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QT.Common.Extension;

public class AiChatClientFactory : IAiChatClientFactory
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly BubbleAiProvidersOptions _opt;
    private readonly IMemoryCache _cache;

    public AiChatClientFactory(IHttpClientFactory httpFactory, IOptions<BubbleAiProvidersOptions> opt, IMemoryCache cache)
    {
        _httpFactory = httpFactory;
        _opt = opt.Value;
        _cache = cache;
    }

    public IAiChatClient Get(string? providerKey)
    {
        var key = providerKey;
        if (key.IsNullOrEmpty())
        {
            key = _opt.DefaultProvider ?? "openai";
        }
        key = key.Trim().ToLowerInvariant();
        var http = _httpFactory.CreateClient($"bubble-ai-{key}");

        return key switch
        {
            "deepseek" => new OpenAiCompatibleChatClient("deepseek", http, _opt.DeepSeek),
            "qianfan" => new QianfanChatClient(http, _opt.Qianfan, _cache),
            _ => new OpenAiCompatibleChatClient("openai", http, _opt.OpenAi)
        };
    }
}

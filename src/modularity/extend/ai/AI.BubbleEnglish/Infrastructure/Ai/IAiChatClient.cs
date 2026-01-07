namespace AI.BubbleEnglish.Infrastructure.Ai;

public interface IAiChatClient
{
    string ProviderKey { get; }
    Task<AiChatResult> CompleteAsync(AiChatRequest request, CancellationToken ct = default);
}

public interface IAiChatClientFactory
{
    IAiChatClient Get(string? providerKey);
}

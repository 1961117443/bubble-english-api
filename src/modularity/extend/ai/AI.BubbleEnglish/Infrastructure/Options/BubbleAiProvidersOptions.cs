namespace AI.BubbleEnglish.Infrastructure.Options;

public class BubbleAiProvidersOptions
{
    public string DefaultProvider { get; set; } = "openai";

    public OpenAiCompatibleProviderOptions OpenAi { get; set; } = new();
    public OpenAiCompatibleProviderOptions DeepSeek { get; set; } = new();
    public QianfanProviderOptions Qianfan { get; set; } = new();
}

public class OpenAiCompatibleProviderOptions
{
    public string BaseUrl { get; set; } = "https://api.openai.com";
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gpt-4o-mini";
}

public class QianfanProviderOptions
{
    public string ApiKey { get; set; } = string.Empty; // client_id
    public string SecretKey { get; set; } = string.Empty; // client_secret
    public string DefaultModelEndpoint { get; set; } = "ernie-bot-4"; // endpoint name
}

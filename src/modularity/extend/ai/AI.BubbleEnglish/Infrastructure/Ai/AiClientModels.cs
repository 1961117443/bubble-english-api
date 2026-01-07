namespace AI.BubbleEnglish.Infrastructure.Ai;

public class AiChatRequest
{
    public string Model { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.2;
    public int? MaxTokens { get; set; }
    public bool JsonOutput { get; set; } = true;
}

public class AiChatResult
{
    public string Content { get; set; } = string.Empty;
    public string Raw { get; set; } = string.Empty;
}

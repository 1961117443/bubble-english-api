namespace AI.BubbleEnglish.Infrastructure.Tts;

public interface ITtsEngine
{
    /// <summary>
    /// Synthesize English word/sentence into mp3. Caller ensures output directory exists.
    /// </summary>
    Task SynthesizeToMp3Async(string text, string outputMp3Path, CancellationToken ct = default);
}

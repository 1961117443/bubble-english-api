namespace AI.BubbleEnglish.Infrastructure.Asr;

public record AsrResult(string TextPath, string SrtPath);

public interface IAsrEngine
{
    Task<AsrResult> TranscribeToTextAndSrtAsync(string inputWavPath, string outputDir, CancellationToken ct = default);
}

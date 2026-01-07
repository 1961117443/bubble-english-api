namespace AI.BubbleEnglish.Infrastructure.Audio;

using AI.BubbleEnglish.Infrastructure.Options;
using AI.BubbleEnglish.Infrastructure.Tools;
using Microsoft.Extensions.Options;

public interface IFfmpegAudioService
{
    Task ExtractWav16kMonoAsync(string inputVideoPath, string outputWavPath, CancellationToken ct = default);
    Task CutMp3Async(string inputWavPath, string outputMp3Path, double startSeconds, double endSeconds, CancellationToken ct = default);
    Task ConvertWavToMp3Async(string inputWavPath, string outputMp3Path, CancellationToken ct = default);
}

public class FfmpegAudioService : IFfmpegAudioService
{
    private readonly BubbleToolsOptions _tools;
    private readonly IProcessRunner _runner;

    public FfmpegAudioService(IOptions<BubbleToolsOptions> tools, IProcessRunner runner)
    {
        _tools = tools.Value;
        _runner = runner;
    }

    public async Task ExtractWav16kMonoAsync(string inputVideoPath, string outputWavPath, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputWavPath)!);
        var args = $"-y -i \"{inputVideoPath}\" -vn -ac 1 -ar 16000 \"{outputWavPath}\"";
        var r = await _runner.RunAsync(_tools.FfmpegPath, args, timeout: TimeSpan.FromMinutes(20), cancellationToken: ct);
        if (r.ExitCode != 0) throw new InvalidOperationException($"ffmpeg extract wav failed: {r.StdErr}");
    }

    public async Task CutMp3Async(string inputWavPath, string outputMp3Path, double startSeconds, double endSeconds, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputMp3Path)!);
        // loudnorm for consistent volume; 96k is enough for children app
        var args = $"-y -ss {startSeconds:0.###} -to {endSeconds:0.###} -i \"{inputWavPath}\" -af \"loudnorm\" -ac 1 -ar 16000 -b:a 96k \"{outputMp3Path}\"";
        var r = await _runner.RunAsync(_tools.FfmpegPath, args, timeout: TimeSpan.FromMinutes(10), cancellationToken: ct);
        if (r.ExitCode != 0) throw new InvalidOperationException($"ffmpeg cut mp3 failed: {r.StdErr}");
    }

    public async Task ConvertWavToMp3Async(string inputWavPath, string outputMp3Path, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputMp3Path)!);
        var args = $"-y -i \"{inputWavPath}\" -ac 1 -ar 16000 -b:a 96k \"{outputMp3Path}\"";
        var r = await _runner.RunAsync(_tools.FfmpegPath, args, timeout: TimeSpan.FromMinutes(5), cancellationToken: ct);
        if (r.ExitCode != 0) throw new InvalidOperationException($"ffmpeg wav->mp3 failed: {r.StdErr}");
    }
}

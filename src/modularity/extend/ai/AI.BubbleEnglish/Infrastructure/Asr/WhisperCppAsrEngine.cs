namespace AI.BubbleEnglish.Infrastructure.Asr;

using AI.BubbleEnglish.Infrastructure.Options;
using AI.BubbleEnglish.Infrastructure.Tools;
using Microsoft.Extensions.Options;

/// <summary>
/// ASR via whisper.cpp CLI. It produces txt/srt files.
/// </summary>
public class WhisperCppAsrEngine : IAsrEngine
{
    private readonly BubbleToolsOptions _tools;
    private readonly IProcessRunner _runner;

    public WhisperCppAsrEngine(IOptions<BubbleToolsOptions> tools, IProcessRunner runner)
    {
        _tools = tools.Value;
        _runner = runner;
    }

    public async Task<AsrResult> TranscribeToTextAndSrtAsync(string inputWavPath, string outputDir, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputDir);

        // whisper.cpp outputs files using --output-file basename and --output-dir.
        // We generate both srt and txt.
        var baseName = "asr";
        var args = $"-m \"{_tools.WhisperModelPath}\" -f \"{inputWavPath}\" --output-dir \"{outputDir}\" --output-file {baseName} --language {_tools.WhisperLanguage} --threads {_tools.WhisperThreads} --srt --txt";

        var r = await _runner.RunAsync(_tools.WhisperCliPath, args, timeout: TimeSpan.FromMinutes(60), cancellationToken: ct);
        if (r.ExitCode != 0)
            throw new InvalidOperationException($"whisper-cli failed: {r.StdErr}\n{r.StdOut}");

        var txt = Path.Combine(outputDir, baseName + ".txt");
        var srt = Path.Combine(outputDir, baseName + ".srt");

        if (!File.Exists(txt) || new FileInfo(txt).Length == 0)
            throw new InvalidOperationException("whisper-cli produced empty txt");
        if (!File.Exists(srt) || new FileInfo(srt).Length == 0)
            throw new InvalidOperationException("whisper-cli produced empty srt");

        return new AsrResult(txt, srt);
    }
}

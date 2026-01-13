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

    public async Task<AsrResult> TranscribeToTextAndSrtAsync(
     string inputWavPath,
     string outputDir, // 保留签名，但实际不再依赖它
     CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(inputWavPath) || !File.Exists(inputWavPath))
            throw new FileNotFoundException("input wav not found", inputWavPath);

        // ✅ 以 inputWavPath 所在目录作为实际输出目录
        var dir = Path.GetDirectoryName(inputWavPath)!;
        //Directory.CreateDirectory(outputDir);

        // 记录执行前已有的文件，防止读取到旧产物
        var beforeTxt = new HashSet<string>(Directory.GetFiles(dir, "*.txt"));
        var beforeSrt = new HashSet<string>(Directory.GetFiles(dir, "*.srt"));

        // ✅ 兼容你当前 whisper-cli 版本：不用 --output-dir/--output-file/--txt/--srt
        var args =
            $"-m \"{_tools.WhisperModelPath}\" " +
            $"-f \"{inputWavPath}\" " +
            $"-l {_tools.WhisperLanguage} " +
            $"-t {_tools.WhisperThreads} " +
            $"--output-txt --output-srt";

        // 如果你的 Runner 支持 workingDirectory，建议设置为 dir（更稳）
        var r = await _runner.RunAsync(
            _tools.WhisperCliPath,
            args,
            // workingDirectory: dir, // 如果你有这个参数就打开
            timeout: TimeSpan.FromMinutes(60),
            cancellationToken: ct);

        if (r.ExitCode != 0)
            throw new InvalidOperationException($"whisper-cli failed: {r.StdErr}\n{r.StdOut}");

        // 优先找“新增文件”，找不到再回退到“最新文件”
        var txt = Directory.GetFiles(dir, "*.txt")
            .Where(f => !beforeTxt.Contains(f))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? Directory.GetFiles(dir, "*.txt").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();

        var srt = Directory.GetFiles(dir, "*.srt")
            .Where(f => !beforeSrt.Contains(f))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault()
            ?? Directory.GetFiles(dir, "*.srt").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(txt) || !File.Exists(txt) || new FileInfo(txt).Length == 0)
            throw new InvalidOperationException("whisper-cli produced empty txt");

        if (string.IsNullOrWhiteSpace(srt) || !File.Exists(srt) || new FileInfo(srt).Length == 0)
            throw new InvalidOperationException("whisper-cli produced empty srt");

        return new AsrResult(txt, srt);
    }


}

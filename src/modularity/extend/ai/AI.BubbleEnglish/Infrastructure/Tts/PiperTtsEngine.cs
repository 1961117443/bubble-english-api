namespace AI.BubbleEnglish.Infrastructure.Tts;

using AI.BubbleEnglish.Infrastructure.Audio;
using AI.BubbleEnglish.Infrastructure.Options;
using AI.BubbleEnglish.Infrastructure.Tools;
using Microsoft.Extensions.Options;

public class PiperTtsEngine : ITtsEngine
{
    private readonly BubbleToolsOptions _tools;
    private readonly IProcessRunner _runner;
    private readonly IFfmpegAudioService _ffmpeg;

    public PiperTtsEngine(IOptions<BubbleToolsOptions> tools, IProcessRunner runner, IFfmpegAudioService ffmpeg)
    {
        _tools = tools.Value;
        _runner = runner;
        _ffmpeg = ffmpeg;
    }

    public async Task SynthesizeToMp3Async(string text, string outputMp3Path, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputMp3Path)!);

        var tmpWav = Path.Combine(Path.GetTempPath(), $"bubble_tts_{Guid.NewGuid():N}.wav");
        try
        {
            // Pipe text into piper stdin.
            // piper -m voice.onnx -f out.wav
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _tools.PiperPath,
                Arguments = $"-m \"{_tools.PiperVoiceModelPath}\" -f \"{tmpWav}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = new System.Diagnostics.Process { StartInfo = psi };
            if (!p.Start()) throw new InvalidOperationException("Failed to start piper");

            await p.StandardInput.WriteAsync(text);
            await p.StandardInput.FlushAsync();
            p.StandardInput.Close();

            var stdoutTask = p.StandardOutput.ReadToEndAsync();
            var stderrTask = p.StandardError.ReadToEndAsync();

            var completed = await Task.WhenAny(p.WaitForExitAsync(ct), Task.Delay(TimeSpan.FromMinutes(2), ct));
            if (completed is not Task waitTask || !p.HasExited)
            {
                try { p.Kill(entireProcessTree: true); } catch { }
                throw new TimeoutException("piper timeout");
            }

            var stderr = await stderrTask;
            if (p.ExitCode != 0)
            {
                var stdout = await stdoutTask;
                throw new InvalidOperationException($"piper failed: {stderr}\n{stdout}");
            }

            if (!File.Exists(tmpWav) || new FileInfo(tmpWav).Length == 0)
                throw new InvalidOperationException("piper produced empty wav");

            await _ffmpeg.ConvertWavToMp3Async(tmpWav, outputMp3Path, ct);
        }
        finally
        {
            try { if (File.Exists(tmpWav)) File.Delete(tmpWav); } catch { }
        }
    }
}

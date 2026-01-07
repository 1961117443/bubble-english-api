namespace AI.BubbleEnglish.Infrastructure.Tools;

using System.Diagnostics;

public record ProcessRunResult(int ExitCode, string StdOut, string StdErr, TimeSpan Elapsed);

public interface IProcessRunner
{
    Task<ProcessRunResult> RunAsync(string fileName, string arguments, string? workingDirectory = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
}

public class ProcessRunner : IProcessRunner
{
    public async Task<ProcessRunResult> RunAsync(string fileName, string arguments, string? workingDirectory = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        using var p = new Process();
        p.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? string.Empty,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            if (!p.Start()) throw new InvalidOperationException($"Failed to start process: {fileName}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot start process: {fileName}. {ex.Message}", ex);
        }

        var stdoutTask = p.StandardOutput.ReadToEndAsync();
        var stderrTask = p.StandardError.ReadToEndAsync();

        var waitTask = p.WaitForExitAsync(cancellationToken);
        if (timeout.HasValue)
        {
            var completed = await Task.WhenAny(waitTask, Task.Delay(timeout.Value, cancellationToken));
            if (completed != waitTask)
            {
                try { p.Kill(entireProcessTree: true); } catch { }
                throw new TimeoutException($"Process timeout: {fileName} {arguments}");
            }
        }
        else
        {
            await waitTask;
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        sw.Stop();
        return new ProcessRunResult(p.ExitCode, stdout, stderr, sw.Elapsed);
    }
}

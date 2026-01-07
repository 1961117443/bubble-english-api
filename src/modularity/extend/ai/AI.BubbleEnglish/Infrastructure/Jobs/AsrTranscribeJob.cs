namespace AI.BubbleEnglish.Infrastructure.Jobs;

using AI.BubbleEnglish.Entitys;
using AI.BubbleEnglish.Infrastructure.Asr;
using AI.BubbleEnglish.Infrastructure.Audio;
using AI.BubbleEnglish.Infrastructure.Storage;
using SqlSugar;

/// <summary>
/// Quartz job: extract audio and run whisper.cpp, then write SourceText/SRT.
/// </summary>
public class AsrTranscribeJob : IJob
{
    public const string JobKeyName = "bubble.asr.transcribe";

    private readonly ISqlSugarClient _db;
    private readonly IBubbleStorageService _storage;
    private readonly IFfmpegAudioService _ffmpeg;
    private readonly IAsrEngine _asr;

    public AsrTranscribeJob(ISqlSugarClient db, IBubbleStorageService storage, IFfmpegAudioService ffmpeg, IAsrEngine asr)
    {
        _db = db;
        _storage = storage;
        _ffmpeg = ffmpeg;
        _asr = asr;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var videoId = context.MergedJobDataMap.GetString("videoId") ?? "";
        if (string.IsNullOrWhiteSpace(videoId)) return;

        var v = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == videoId);
        if (v == null) return;

        var job = new BubblePreprocessJobEntity
        {
            VideoId = v.Id,
            Type = "asr_transcribe",
            Status = "processing",
            StartedAt = DateTime.Now,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };
        var jobId = await _db.Insertable(job).ExecuteReturnIdentityAsync();
        job.Id = jobId;

        try
        {
            var workDir = _storage.GetVideoWorkDir(v.Id, v.CreateTime);
            Directory.CreateDirectory(workDir);
            v.WorkDir = workDir;

            var inputVideo = !string.IsNullOrWhiteSpace(v.LocalPath) ? v.LocalPath! : v.FileUrl;
            if (string.IsNullOrWhiteSpace(inputVideo) || !File.Exists(inputVideo))
                throw new FileNotFoundException("video file not found", inputVideo);

            var derivedDir = _storage.GetDerivedDir(v.Id, v.CreateTime);
            var subtitleDir = _storage.GetSubtitleDir(v.Id, v.CreateTime);
            Directory.CreateDirectory(derivedDir);
            Directory.CreateDirectory(subtitleDir);

            var wav = Path.Combine(derivedDir, "audio.wav");
            if (!File.Exists(wav) || new FileInfo(wav).Length == 0)
                await _ffmpeg.ExtractWav16kMonoAsync(inputVideo, wav, ct);

            var asrRes = await _asr.TranscribeToTextAndSrtAsync(wav, subtitleDir, ct);
            var sourceText = await File.ReadAllTextAsync(asrRes.TextPath, ct);

            v.SourceText = sourceText;
            v.SourceTextType = "asr";
            v.SourceTextLang = "en";
            v.LastError = null;
            v.UpdateTime = DateTime.Now;
            await _db.Updateable(v).UpdateColumns(x => new { x.WorkDir, x.SourceText, x.SourceTextType, x.SourceTextLang, x.LastError, x.UpdateTime }).ExecuteCommandAsync();

            job.Status = "success";
            job.ResultText = sourceText;
            job.ResultMetaJson = System.Text.Json.JsonSerializer.Serialize(new { wav, txt = asrRes.TextPath, srt = asrRes.SrtPath });
            job.FinishedAt = DateTime.Now;
            job.UpdateTime = DateTime.Now;
            await _db.Updateable(job).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            v.LastError = ex.Message;
            v.UpdateTime = DateTime.Now;
            await _db.Updateable(v).UpdateColumns(x => new { x.LastError, x.UpdateTime }).ExecuteCommandAsync();

            job.Status = "failed";
            job.ErrorMessage = ex.ToString();
            job.FinishedAt = DateTime.Now;
            job.UpdateTime = DateTime.Now;
            await _db.Updateable(job).ExecuteCommandAsync();

            throw;
        }
    }
}

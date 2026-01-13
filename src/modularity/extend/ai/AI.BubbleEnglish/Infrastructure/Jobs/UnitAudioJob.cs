namespace AI.BubbleEnglish.Infrastructure.Jobs;

using AI.BubbleEnglish.Entitys;
using AI.BubbleEnglish.Infrastructure.Audio;
using AI.BubbleEnglish.Infrastructure.Storage;
using AI.BubbleEnglish.Infrastructure.Tts;
using SqlSugar;

/// <summary>
/// Generate unit audio:
/// - sentence: cut from video audio.wav by aligning with ASR SRT
/// - word: local TTS (Piper) -> mp3
/// </summary>
public class UnitAudioJob : IJob
{
    public const string JobKeyName = "bubble.unit.audio";

    private readonly ISqlSugarClient _db;
    private readonly IBubbleStorageService _storage;
    private readonly IFfmpegAudioService _ffmpeg;
    private readonly ITtsEngine _tts;

    public UnitAudioJob(ISqlSugarClient db, IBubbleStorageService storage, IFfmpegAudioService ffmpeg, ITtsEngine tts)
    {
        _db = db;
        _storage = storage;
        _ffmpeg = ffmpeg;
        _tts = tts;
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
            Type = "unit_audio",
            Status = "processing",
            StartedAt = DateTime.Now,
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };
        var jobId = await _db.Insertable(job).ExecuteReturnIdentityAsync();
        job.Id = jobId;

        try
        {
            var unitsQ = _db.Queryable<BubbleUnitEntity>();
            if (!string.IsNullOrWhiteSpace(videoId)) unitsQ = unitsQ.Where(x => x.VideoId == videoId);
            var units = await unitsQ.ToListAsync();

            var derivedDir = _storage.GetDerivedDir(videoId, v.CreateTime);
            var wav = Path.Combine(derivedDir, "audio.wav");
            if (!File.Exists(wav) || new FileInfo(wav).Length == 0)
            {
                // ensure audio exists
                var inputVideo = !string.IsNullOrWhiteSpace(v.LocalPath) ? v.LocalPath! : v.FileUrl;
                await _ffmpeg.ExtractWav16kMonoAsync(inputVideo, wav, ct);
            }

            var unitAudioDir = _storage.GetUnitAudioDir(videoId, v.CreateTime);
            Directory.CreateDirectory(unitAudioDir);

            // Load SRT for sentence alignment
            var srtPath = Path.Combine(_storage.GetSubtitleDir(videoId, v.CreateTime), "asr.srt");
            List<SrtItem> srtItems = new();
            if (File.Exists(srtPath))
            {
                var srtContent = await File.ReadAllTextAsync(srtPath, ct);
                srtItems = SrtParser.Parse(srtContent);
            }

            int ok = 0, skip = 0;
            foreach (var u in units)
            {
                if (!string.IsNullOrWhiteSpace(u.AudioUrl)) { skip++; continue; }

                if (u.UnitType == "word")
                {
                    var mp3 = Path.Combine(unitAudioDir, $"word_{u.Id}.mp3");
                    if (!File.Exists(mp3) || new FileInfo(mp3).Length == 0)
                        await _tts.SynthesizeToMp3Async(u.Text, mp3, ct);

                    u.AudioUrl = _storage.ToPublicUrl(mp3);
                    u.UpdateTime = DateTime.Now;
                    await _db.Updateable(u).UpdateColumns(x => new { x.AudioUrl, x.UpdateTime }).ExecuteCommandAsync();
                    ok++;
                }
                else if (u.UnitType == "sentence")
                {
                    if (srtItems.Count == 0) { skip++; continue; }

                    var range = SrtAligner.FindTimeRangeForSentence(u.Text, srtItems);
                    if (range == null) { skip++; continue; }

                    // padding
                    var start = Math.Max(0, range.Value.start - 0.15);
                    var end = Math.Max(start + 0.3, range.Value.end + 0.20);

                    var mp3 = Path.Combine(unitAudioDir, $"sentence_{u.Id}.mp3");
                    if (!File.Exists(mp3) || new FileInfo(mp3).Length == 0)
                        await _ffmpeg.CutMp3Async(wav, mp3, start, end, ct);

                    u.AudioUrl = _storage.ToPublicUrl(mp3);
                    u.UpdateTime = DateTime.Now;
                    await _db.Updateable(u).UpdateColumns(x => new { x.AudioUrl, x.UpdateTime }).ExecuteCommandAsync();
                    ok++;
                }
                else
                {
                    skip++;
                }
            }

            job.Status = "success";
            job.ResultMetaJson = System.Text.Json.JsonSerializer.Serialize(new { ok, skip });
            job.FinishedAt = DateTime.Now;
            job.UpdateTime = DateTime.Now;
            await _db.Updateable(job).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            job.Status = "failed";
            job.ErrorMessage = ex.ToString();
            job.FinishedAt = DateTime.Now;
            job.UpdateTime = DateTime.Now;
            await _db.Updateable(job).ExecuteCommandAsync();
            throw;
        }
    }
}

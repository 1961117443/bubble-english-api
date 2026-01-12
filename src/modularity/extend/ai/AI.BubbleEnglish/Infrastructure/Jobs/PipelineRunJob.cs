namespace AI.BubbleEnglish.Infrastructure.Jobs;

using AI.BubbleEnglish.Entitys;
using Quartz;
using SqlSugar;

/// <summary>
/// Quartz job: 串行编排 ASR -> AI Analyze -> UnitAudio。
///
/// 设计目标：
/// - 解决 /pipeline/actions/run 过去“顺序入队但并行抢跑”的问题。
/// - 通过“轮询推进”的方式保证依赖满足后才触发下一步。
///
/// 约定：
/// - JobDataMap: videoId(string), aiJobId(long, optional)
/// - 若 aiJobId 未传入，则仅推进到 ASR 完成（不创建 AI Job）。
/// </summary>
public class PipelineRunJob : IJob
{
    public const string JobKeyName = "bubble.pipeline.run";

    private readonly ISqlSugarClient _db;
    private readonly IBubbleQuartzEnqueue _enqueue;

    public PipelineRunJob(ISqlSugarClient db, IBubbleQuartzEnqueue enqueue)
    {
        _db = db;
        _enqueue = enqueue;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var map = context.MergedJobDataMap;
        var videoId = map.GetString("videoId") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(videoId)) return;

        long? aiJobId = null;
        if (map.ContainsKey("aiJobId"))
        {
            try { aiJobId = Convert.ToInt64(map["aiJobId"]); } catch { /* ignore */ }
        }

        var v = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == videoId);
        if (v == null) return;

        async Task RequeueSelfAsync(int seconds)
        {
            var runKey = new JobKey(JobKeyName + ".run." + Guid.NewGuid().ToString("N"));
            var job = JobBuilder.Create<PipelineRunJob>()
                .WithIdentity(runKey)
                .UsingJobData(map)
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity(runKey.Name + ".trigger")
                .StartAt(DateBuilder.FutureDate(seconds, IntervalUnit.Second))
                .Build();
            await context.Scheduler.ScheduleJob(job, trigger, ct);
        }

        // 1) ASR：如果 SourceText 还没有，先确保 ASR 在跑/已入队
        if (string.IsNullOrWhiteSpace(v.SourceText))
        {
            var asrProcessing = await _db.Queryable<BubblePreprocessJobEntity>()
                .Where(x => x.VideoId == videoId && x.Type == "asr_transcribe" && (x.Status == "processing" || x.Status == "queued"))
                .AnyAsync();

            if (!asrProcessing)
                await _enqueue.EnqueueAsrAsync(videoId, ct);

            // 10s 后再推进（不在 Job 里 sleep，避免占用 worker）
            await RequeueSelfAsync(10);
            return;
        }

        // 2) AI Analyze：必须要有 aiJobId 才推进
        if (aiJobId.HasValue && aiJobId.Value > 0)
        {
            var aiJob = await _db.Queryable<BubbleAiJobEntity>().SingleAsync(x => x.Id == aiJobId.Value);
            if (aiJob == null) return;

            if (aiJob.Status == "failed")
                return; // 失败由人工 retry

            if (aiJob.Status == "queued")
            {
                // 如果还没开始跑，则触发一次（幂等由 Quartz 触发粒度保证）
                await _enqueue.EnqueueAiAnalyzeAsync(aiJobId.Value, ct);
                await RequeueSelfAsync(10);
                return;
            }

            if (aiJob.Status != "success")
            {
                // processing: 等待下一次调度
                await RequeueSelfAsync(10);
                return;
            }

            // 3) UnitAudio：如果已经成功生成过就不再重复
            var audioDone = await _db.Queryable<BubblePreprocessJobEntity>()
                .Where(x => x.VideoId == videoId && x.Type == "unit_audio" && x.Status == "success")
                .AnyAsync();
            if (audioDone) return;

            var audioProcessing = await _db.Queryable<BubblePreprocessJobEntity>()
                .Where(x => x.VideoId == videoId && x.Type == "unit_audio" && (x.Status == "processing" || x.Status == "queued"))
                .AnyAsync();
            if (!audioProcessing)
                await _enqueue.EnqueueUnitAudioAsync(videoId, ct);

            // UnitAudio 已触发，再排一次用于观察完成（可选）
            await RequeueSelfAsync(10);
        }
    }
}

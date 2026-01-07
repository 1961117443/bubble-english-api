namespace AI.BubbleEnglish.Infrastructure.Quartz;

using AI.BubbleEnglish.Infrastructure.Jobs;

public interface IBubbleQuartzScheduler
{
    Task EnqueueAsrAsync(string videoId, CancellationToken ct = default);
    Task EnqueueAiAnalyzeAsync(long aiJobId, CancellationToken ct = default);
    Task EnqueueUnitAudioAsync(string videoId, CancellationToken ct = default);
}

public class BubbleQuartzScheduler : IBubbleQuartzScheduler
{
    private readonly IScheduler _scheduler;

    public BubbleQuartzScheduler(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public Task EnqueueAsrAsync(string videoId, CancellationToken ct = default)
        => EnqueueOnceAsync(AsrTranscribeJob.JobKeyName, typeof(AsrTranscribeJob), new JobDataMap { { "videoId", videoId } }, ct);

    public Task EnqueueAiAnalyzeAsync(long aiJobId, CancellationToken ct = default)
        => EnqueueOnceAsync(AiAnalyzeJob.JobKeyName, typeof(AiAnalyzeJob), new JobDataMap { { "aiJobId", aiJobId } }, ct);

    public Task EnqueueUnitAudioAsync(string videoId, CancellationToken ct = default)
        => EnqueueOnceAsync(UnitAudioJob.JobKeyName, typeof(UnitAudioJob), new JobDataMap { { "videoId", videoId } }, ct);

    private async Task EnqueueOnceAsync(string keyName, Type jobType, JobDataMap data, CancellationToken ct)
    {
        var jobKey = new JobKey(keyName + "." + Guid.NewGuid().ToString("N"));
        var job = JobBuilder.Create(jobType)
            .WithIdentity(jobKey)
            .UsingJobData(data)
            .Build();

        var trigger = TriggerBuilder.Create()
            .StartNow()
            .WithIdentity(jobKey.Name + ".trigger")
            .Build();

        await _scheduler.ScheduleJob(job, trigger, ct);
    }
}

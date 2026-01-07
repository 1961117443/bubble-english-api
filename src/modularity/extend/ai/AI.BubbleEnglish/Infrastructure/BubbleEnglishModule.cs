namespace AI.BubbleEnglish.Infrastructure;

using AI.BubbleEnglish.Infrastructure.Ai;
using AI.BubbleEnglish.Infrastructure.Asr;
using AI.BubbleEnglish.Infrastructure.Audio;
using AI.BubbleEnglish.Infrastructure.Jobs;
using AI.BubbleEnglish.Infrastructure.Options;
using AI.BubbleEnglish.Infrastructure.Storage;
using AI.BubbleEnglish.Infrastructure.Tools;
using AI.BubbleEnglish.Infrastructure.Tts;
using global::Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

public static class BubbleEnglishModule
{
    /// <summary>
    /// Register BubbleEnglish video-processing pipeline dependencies.
    /// You should call this from your main host Startup/Program.
    /// </summary>
    public static IServiceCollection AddBubbleEnglishPipeline(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<BubbleStorageOptions>(config.GetSection("Bubble:Storage"));
        services.Configure<BubbleToolsOptions>(config.GetSection("Bubble:Tools"));
        services.Configure<BubbleAiProvidersOptions>(config.GetSection("Bubble:AI"));

        services.AddMemoryCache();
        services.AddHttpClient();

        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IBubbleStorageService, BubbleStorageService>();
        services.AddSingleton<IFfmpegAudioService, FfmpegAudioService>();
        services.AddSingleton<IAsrEngine, WhisperCppAsrEngine>();
        services.AddSingleton<ITtsEngine, PiperTtsEngine>();

        services.AddSingleton<IAiChatClientFactory, AiChatClientFactory>();

        // Quartz (hosted by Quartz.Extensions.Hosting in main app)
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
            q.AddJob<AsrTranscribeJob>(opts => opts.WithIdentity(AsrTranscribeJob.JobKeyName));
            q.AddJob<AiAnalyzeJob>(opts => opts.WithIdentity(AiAnalyzeJob.JobKeyName));
            q.AddJob<UnitAudioJob>(opts => opts.WithIdentity(UnitAudioJob.JobKeyName));
        });
        services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

        // Helper scheduler (enqueue ad-hoc triggers)
        services.AddSingleton<IBubbleQuartzEnqueue, BubbleQuartzEnqueue>();

        return services;
    }
}

public interface IBubbleQuartzEnqueue
{
    Task EnqueueAsrAsync(string videoId, CancellationToken ct = default);
    Task EnqueueAiAnalyzeAsync(long aiJobId, CancellationToken ct = default);
    Task EnqueueUnitAudioAsync(string videoId, CancellationToken ct = default);
}

internal class BubbleQuartzEnqueue : IBubbleQuartzEnqueue
{
    private readonly ISchedulerFactory _factory;
    public BubbleQuartzEnqueue(ISchedulerFactory factory) { _factory = factory; }

    public Task EnqueueAsrAsync(string videoId, CancellationToken ct = default)
        => EnqueueAsync(AsrTranscribeJob.JobKeyName, new JobDataMap { { "videoId", videoId } }, ct);

    public Task EnqueueAiAnalyzeAsync(long aiJobId, CancellationToken ct = default)
        => EnqueueAsync(AiAnalyzeJob.JobKeyName, new JobDataMap { { "aiJobId", aiJobId } }, ct);

    public Task EnqueueUnitAudioAsync(string videoId, CancellationToken ct = default)
        => EnqueueAsync(UnitAudioJob.JobKeyName, new JobDataMap { { "videoId", videoId } }, ct);

    private async Task EnqueueAsync(string baseJobKey, JobDataMap data, CancellationToken ct)
    {
        var scheduler = await _factory.GetScheduler(ct);
        var runKey = new JobKey(baseJobKey + ".run." + Guid.NewGuid().ToString("N"));
        var job = JobBuilder.Create(Type.GetType(GetJobTypeName(baseJobKey))!)
            .WithIdentity(runKey)
            .UsingJobData(data)
            .Build();

        var trigger = TriggerBuilder.Create()
            .StartNow()
            .WithIdentity(runKey.Name + ".trigger")
            .Build();

        await scheduler.ScheduleJob(job, trigger, ct);
    }

    private static string GetJobTypeName(string baseJobKey)
    {
        return baseJobKey switch
        {
            var k when k == AsrTranscribeJob.JobKeyName => typeof(AsrTranscribeJob).AssemblyQualifiedName!,
            var k when k == AiAnalyzeJob.JobKeyName => typeof(AiAnalyzeJob).AssemblyQualifiedName!,
            var k when k == UnitAudioJob.JobKeyName => typeof(UnitAudioJob).AssemblyQualifiedName!,
            _ => throw new InvalidOperationException("Unknown job key")
        };
    }
}

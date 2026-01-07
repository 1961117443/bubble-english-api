namespace AI.BubbleEnglish.Infrastructure.Quartz;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Self-contained Quartz host. If your main application already hosts Quartz, you can skip this and just register our jobs.
/// </summary>
public class BubbleQuartzHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private IScheduler? _scheduler;

    public BubbleQuartzHostedService(IServiceProvider sp)
    {
        _sp = sp;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new StdSchedulerFactory();
        _scheduler = await factory.GetScheduler(cancellationToken);
        _scheduler.JobFactory = new BubbleJobFactory(_sp);

        // Register jobs (triggers are created on-demand via BubbleQuartzScheduler)
        await _scheduler.Start(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_scheduler != null)
            await _scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken);
    }
}

internal class BubbleJobFactory : IJobFactory
{
    private readonly IServiceProvider _sp;
    public BubbleJobFactory(IServiceProvider sp) { _sp = sp; }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        => (IJob)_sp.GetRequiredService(bundle.JobDetail.JobType);

    public void ReturnJob(IJob job) { }
}

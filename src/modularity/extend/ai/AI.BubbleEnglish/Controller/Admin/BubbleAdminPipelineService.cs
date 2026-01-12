using AI.BubbleEnglish.Entitys;
using AI.BubbleEnglish.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Security;
using QT.Common.Filter;
using SqlSugar;

namespace AI.BubbleEnglish;

/// <summary>
/// 后台：视频 -> ASR(SourceText) -> AI拆解 -> Units音频
/// 说明：实际执行由 Quartz 任务完成；这里负责落库 + 触发入队。
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Pipeline", Order = 2005)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminPipelineService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient _db;
    private readonly IBubbleQuartzEnqueue _enqueue;

    public BubbleAdminPipelineService(ISqlSugarClient db, IBubbleQuartzEnqueue enqueue)
    {
        _db = db;
        _enqueue = enqueue;
    }

    /// <summary>
    /// 1) 先生成 SourceText（whisper.cpp）
    /// </summary>
    [HttpPost("actions/asr")]
    public async Task<dynamic> EnqueueAsr([FromBody] AdminVideoIdInput input)
    {
        var v = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == input.videoId);
        if (v == null) throw Oops.Oh("视频不存在");

        await _enqueue.EnqueueAsrAsync(v.Id);
        return new { ok = true };
    }

    /// <summary>
    /// 2) 创建并入队 AI 分析任务（依赖 SourceText）
    /// </summary>
    [HttpPost("actions/analyze")]
    public async Task<dynamic> EnqueueAi([FromBody] AdminAiAnalyzeInput input)
    {
        var v = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == input.videoId);
        if (v == null) throw Oops.Oh("视频不存在");

        var job = new BubbleAiJobEntity
        {
            VideoId = v.Id,
            Status = "queued",
            Provider = string.IsNullOrWhiteSpace(input.provider) ? null : input.provider.Trim(),
            Model = (input.model ?? string.Empty).Trim(),
            Prompt = (input.prompt ?? string.Empty).Trim(),
            CreateTime = DateTime.Now,
            OutputJson = string.Empty,
            ErrorMessage = string.Empty
        };
        var jobId = await _db.Insertable(job).ExecuteReturnIdentityAsync();
        job.Id = jobId;

        v.Status = "analyzing";
        v.AnalyzeJobId = jobId;
        v.UpdateTime = DateTime.Now;
        await _db.Updateable(v).UpdateColumns(x => new { x.Status, x.AnalyzeJobId, x.UpdateTime }).ExecuteCommandAsync();

        await _enqueue.EnqueueAiAnalyzeAsync(jobId);
        return new { ok = true, jobId };
    }

    /// <summary>
    /// 3) 为 Units 生成音频：sentence 切原声、word 本地TTS
    /// </summary>
    [HttpPost("actions/unit-audio")]
    public async Task<dynamic> EnqueueUnitAudio([FromBody] AdminVideoIdInput input)
    {
        var v = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == input.videoId);
        if (v == null) throw Oops.Oh("视频不存在");

        await _enqueue.EnqueueUnitAudioAsync(v.Id);
        return new { ok = true };
    }

    /// <summary>
    /// 一键：若无 SourceText 先ASR，再AI，再生成音频（顺序入队）。
    /// </summary>
    [HttpPost("actions/run")]
    public async Task<dynamic> RunAll([FromBody] AdminAiAnalyzeInput input)
    {
        var v = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == input.videoId);
        if (v == null) throw Oops.Oh("视频不存在");

        // 先创建 AI Job（不直接触发 AiAnalyzeJob），交给 PipelineRunJob 串行推进
        var job = new BubbleAiJobEntity
        {
            VideoId = v.Id,
            Status = "queued",
            Provider = string.IsNullOrWhiteSpace(input.provider) ? null : input.provider.Trim(),
            Model = (input.model ?? string.Empty).Trim(),
            Prompt = (input.prompt ?? string.Empty).Trim(),
            CreateTime = DateTime.Now,
            OutputJson = string.Empty,
            ErrorMessage = string.Empty
        };
        var jobId = await _db.Insertable(job).ExecuteReturnIdentityAsync();

        v.Status = "analyzing";
        v.AnalyzeJobId = jobId;
        v.UpdateTime = DateTime.Now;
        await _db.Updateable(v).UpdateColumns(x => new { x.Status, x.AnalyzeJobId, x.UpdateTime }).ExecuteCommandAsync();

        await _enqueue.EnqueuePipelineRunAsync(v.Id, jobId);
        return new { ok = true, jobId };
    }
}

public class AdminVideoIdInput
{
    public string videoId { get; set; } = string.Empty;
}

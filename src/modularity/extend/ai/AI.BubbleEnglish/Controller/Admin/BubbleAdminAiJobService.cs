
using QT.Common.Const;

namespace AI.BubbleEnglish;

/// <summary>
/// 后台：AI 任务管理（列表/详情/重试）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "AiJob", Order = 2020)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminAiJobService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;

    public BubbleAdminAiJobService(ISqlSugarClient context)
    {
        _db = (SqlSugarClient)context;
    }

    [HttpGet("")]
    public async Task<List<AdminAiJobOutput>> List([FromQuery] long? videoId, [FromQuery] string? status)
    {
        var q = _db.Queryable<BubbleAiJobEntity>();
        if (videoId.HasValue && videoId.Value > 0) q = q.Where(x => x.VideoId == videoId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status!.Trim());
        var list = await q.OrderByDescending(x => x.Id).ToListAsync();
        return list.Select(ToOutput).ToList();
    }

    [HttpGet("detail")]
    public async Task<AdminAiJobOutput> Detail([FromQuery] long id)
    {
        var e = await _db.Queryable<BubbleAiJobEntity>().SingleAsync(x => x.Id == id);
        if (e == null) throw Oops.Oh("任务不存在");
        return ToOutput(e);
    }

    /// <summary>
    /// 重试任务：把状态重置为 queued（真正执行由队列/后台服务处理）
    /// </summary>
    [HttpPost("actions/retry")]
    [Consumes("application/json")]
    public async Task<dynamic> Retry([FromBody] dynamic body)
    {
        long id = (long)(body?.id ?? 0);
        if (id <= 0) throw Oops.Oh("id无效");
        var e = await _db.Queryable<BubbleAiJobEntity>().SingleAsync(x => x.Id == id);
        if (e == null) throw Oops.Oh("任务不存在");

        e.Status = "queued";
        e.ErrorMessage = string.Empty;
        e.StartedAt = null;
        e.FinishedAt = null;
        await _db.Updateable(e).UpdateColumns(x => new { x.Status, x.ErrorMessage, x.StartedAt, x.FinishedAt }).ExecuteCommandAsync();
        return new { ok = true };
    }

    private static AdminAiJobOutput ToOutput(BubbleAiJobEntity e)
        => new AdminAiJobOutput
        {
            id = e.Id,
            videoId = e.VideoId,
            status = e.Status,
            model = e.Model,
            prompt = e.Prompt,
            outputJson = e.OutputJson,
            errorMessage = e.ErrorMessage,
            startedAt = e.StartedAt,
            finishedAt = e.FinishedAt,
            createTime = e.CreateTime
        };
}

namespace AI.BubbleEnglish;

/// <summary>
/// 后台：视频库（上传记录/列表/详情/触发AI分析）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Video", Order = 2010)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminVideoService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;

    public BubbleAdminVideoService(ISqlSugarClient context)
    {
        _db = (SqlSugarClient)context;
    }

    [HttpGet("list")]
    public async Task<List<AdminVideoOutput>> List([FromQuery] AdminVideoQuery query)
    {
        var q = _db.Queryable<BubbleVideoEntity>();

        if (!string.IsNullOrWhiteSpace(query.keyword))
        {
            var kw = query.keyword.Trim();
            q = q.Where(x => x.Title.Contains(kw) || x.FileUrl.Contains(kw));
        }
        if (!string.IsNullOrWhiteSpace(query.themeKey))
        {
            var tk = query.themeKey.Trim();
            q = q.Where(x => x.ThemeKey == tk);
        }
        if (!string.IsNullOrWhiteSpace(query.status))
        {
            var st = query.status.Trim();
            q = q.Where(x => x.Status == st);
        }

        var list = await q.OrderByDescending(x => x.Id).ToListAsync();
        return list.Select(ToOutput).ToList();
    }

    [HttpGet("detail")]
    public async Task<AdminVideoOutput> Detail([FromQuery] long id)
    {
        var e = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == id);
        if (e == null) throw Oops.Oh("视频不存在");
        return ToOutput(e);
    }

    [HttpPost("create")]
    [Consumes("application/json")]
    public async Task<AdminVideoOutput> Create([FromBody] AdminVideoCreateInput input)
    {
        if (string.IsNullOrWhiteSpace(input.title)) throw Oops.Oh("标题不能为空");
        if (string.IsNullOrWhiteSpace(input.fileUrl)) throw Oops.Oh("fileUrl不能为空");

        var e = new BubbleVideoEntity
        {
            Title = input.title.Trim(),
            ThemeKey = (input.themeKey ?? string.Empty).Trim(),
            FileUrl = input.fileUrl.Trim(),
            CoverUrl = (input.coverUrl ?? string.Empty).Trim(),
            DurationSec = input.durationSec,
            Status = "uploaded",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        };

        var id = await _db.Insertable(e).ExecuteReturnIdentityAsync();
        e.Id = id;
        return ToOutput(e);
    }

    [HttpPost("update")]
    [Consumes("application/json")]
    public async Task<AdminVideoOutput> Update([FromBody] AdminVideoUpdateInput input)
    {
        if (input.id <= 0) throw Oops.Oh("id无效");
        var e = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == input.id);
        if (e == null) throw Oops.Oh("视频不存在");

        e.Title = input.title.Trim();
        e.ThemeKey = (input.themeKey ?? string.Empty).Trim();
        e.FileUrl = input.fileUrl.Trim();
        e.CoverUrl = (input.coverUrl ?? string.Empty).Trim();
        e.DurationSec = input.durationSec;
        if (!string.IsNullOrWhiteSpace(input.status)) e.Status = input.status.Trim();
        e.UpdateTime = DateTime.Now;

        await _db.Updateable(e).ExecuteCommandAsync();
        return ToOutput(e);
    }

    [HttpPost("delete")]
    [Consumes("application/json")]
    public async Task<dynamic> Delete([FromBody] dynamic body)
    {
        long id = (long)(body?.id ?? 0);
        if (id <= 0) throw Oops.Oh("id无效");
        await _db.Deleteable<BubbleVideoEntity>().Where(x => x.Id == id).ExecuteCommandAsync();
        return new { ok = true };
    }

    /// <summary>
    /// 触发 AI 分析任务（这里只落库 queued；真正AI调用建议放后台Job/队列）
    /// </summary>
    [HttpPost("actions/analyze")]
    [Consumes("application/json")]
    public async Task<dynamic> Analyze([FromBody] AdminAiAnalyzeInput input)
    {
        if (input.videoId <= 0) throw Oops.Oh("videoId无效");
        var v = await _db.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == input.videoId);
        if (v == null) throw Oops.Oh("视频不存在");

        var job = new BubbleAiJobEntity
        {
            VideoId = v.Id,
            Status = "queued",
            Model = (input.model ?? string.Empty).Trim(),
            Prompt = (input.prompt ?? string.Empty).Trim(),
            CreateTime = DateTime.Now
        };
        var jobId = await _db.Insertable(job).ExecuteReturnIdentityAsync();
        job.Id = jobId;

        v.Status = "analyzing";
        v.AnalyzeJobId = jobId;
        v.UpdateTime = DateTime.Now;
        await _db.Updateable(v).UpdateColumns(x => new { x.Status, x.AnalyzeJobId, x.UpdateTime }).ExecuteCommandAsync();

        return new { ok = true, jobId };
    }

    private static AdminVideoOutput ToOutput(BubbleVideoEntity e)
        => new AdminVideoOutput
        {
            id = e.Id,
            title = e.Title,
            themeKey = e.ThemeKey,
            fileUrl = e.FileUrl,
            coverUrl = e.CoverUrl,
            durationSec = e.DurationSec,
            status = e.Status,
            analyzeJobId = e.AnalyzeJobId,
            createTime = e.CreateTime,
            updateTime = e.UpdateTime
        };
}

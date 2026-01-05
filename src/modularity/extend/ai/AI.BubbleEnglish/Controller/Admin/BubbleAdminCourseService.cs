namespace AI.BubbleEnglish;

/// <summary>
/// 后台：课程管理（course v2 的容器，units 编排可另做表或 JSON 字段）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Course", Order = 2040)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminCourseService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;

    public BubbleAdminCourseService(ISqlSugarClient context)
    {
        _db = (SqlSugarClient)context;
    }

    [HttpGet("list")]
    public async Task<List<AdminCourseOutput>> List([FromQuery] AdminCourseQuery query)
    {
        var q = _db.Queryable<BubbleCourseEntity>();
        if (!string.IsNullOrWhiteSpace(query.keyword))
        {
            var kw = query.keyword.Trim();
            q = q.Where(x => x.Title.Contains(kw) || x.Description.Contains(kw));
        }
        if (query.isPublish.HasValue)
        {
            q = q.Where(x => x.IsPublish == query.isPublish.Value);
        }
        // themeKey 目前 BubbleCourseEntity 没字段，先忽略（后续可加 ThemeKey 字段或关联表）
        var list = await q.OrderByDescending(x => x.Id).ToListAsync();
        return list.Select(ToOutput).ToList();
    }

    [HttpGet("detail")]
    public async Task<AdminCourseOutput> Detail([FromQuery] long id)
    {
        var e = await _db.Queryable<BubbleCourseEntity>().SingleAsync(x => x.Id == id);
        if (e == null) throw Oops.Oh("课程不存在");
        return ToOutput(e);
    }

    [HttpPost("upsert")]
    [Consumes("application/json")]
    public async Task<AdminCourseOutput> Upsert([FromBody] AdminCourseUpsertInput input)
    {
        if (string.IsNullOrWhiteSpace(input.title)) throw Oops.Oh("title不能为空");

        if (input.id.HasValue && input.id.Value > 0)
        {
            var e = await _db.Queryable<BubbleCourseEntity>().SingleAsync(x => x.Id == input.id.Value);
            if (e == null) throw Oops.Oh("课程不存在");

            e.Title = input.title.Trim();
            e.Cover = (input.cover ?? string.Empty).Trim();
            e.Description = (input.description ?? string.Empty).Trim();
            e.Sort = input.sort;
            e.IsPublish = input.isPublish;
            await _db.Updateable(e).ExecuteCommandAsync();
            return ToOutput(e);
        }
        else
        {
            var e = new BubbleCourseEntity
            {
                Title = input.title.Trim(),
                Cover = (input.cover ?? string.Empty).Trim(),
                Description = (input.description ?? string.Empty).Trim(),
                Sort = input.sort,
                IsPublish = input.isPublish,
                CreateTime = DateTime.Now
            };
            var id = await _db.Insertable(e).ExecuteReturnIdentityAsync();
            e.Id = id;
            return ToOutput(e);
        }
    }

    [HttpPost("actions/publish")]
    [Consumes("application/json")]
    public async Task<dynamic> SetPublish([FromBody] dynamic body)
    {
        long id = (long)(body?.id ?? 0);
        int isPublish = (int)(body?.isPublish ?? 0);
        if (id <= 0) throw Oops.Oh("id无效");
        await _db.Updateable<BubbleCourseEntity>()
            .SetColumns(x => x.IsPublish == isPublish)
            .Where(x => x.Id == id)
            .ExecuteCommandAsync();
        return new { ok = true };
    }

    [HttpPost("delete")]
    [Consumes("application/json")]
    public async Task<dynamic> Delete([FromBody] dynamic body)
    {
        long id = (long)(body?.id ?? 0);
        if (id <= 0) throw Oops.Oh("id无效");
        await _db.Deleteable<BubbleCourseEntity>().Where(x => x.Id == id).ExecuteCommandAsync();
        return new { ok = true };
    }

    private static AdminCourseOutput ToOutput(BubbleCourseEntity e)
        => new AdminCourseOutput
        {
            id = e.Id,
            title = e.Title,
            cover = e.Cover,
            description = e.Description,
            sort = e.Sort,
            isPublish = e.IsPublish,
            createTime = e.CreateTime
        };
}

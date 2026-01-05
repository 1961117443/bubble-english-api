namespace AI.BubbleEnglish;

/// <summary>
/// 后台：主题管理
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Theme", Order = 2050)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminThemeService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;

    public BubbleAdminThemeService(ISqlSugarClient context)
    {
        _db = (SqlSugarClient)context;
    }

    [HttpGet("list")]
    public async Task<List<AdminThemeOutput>> List([FromQuery] string? keyword)
    {
        var q = _db.Queryable<BubbleThemeEntity>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            q = q.Where(x => x.ThemeKey.Contains(kw) || x.Title.Contains(kw));
        }
        var list = await q.OrderBy(x => x.Sort).OrderByDescending(x => x.Id).ToListAsync();
        return list.Select(ToOutput).ToList();
    }

    [HttpGet("detail")]
    public async Task<AdminThemeOutput> Detail([FromQuery] long id)
    {
        var e = await _db.Queryable<BubbleThemeEntity>().SingleAsync(x => x.Id == id);
        if (e == null) throw Oops.Oh("主题不存在");
        return ToOutput(e);
    }

    [HttpPost("upsert")]
    [Consumes("application/json")]
    public async Task<AdminThemeOutput> Upsert([FromBody] AdminThemeUpsertInput input)
    {
        if (string.IsNullOrWhiteSpace(input.themeKey)) throw Oops.Oh("themeKey不能为空");
        if (string.IsNullOrWhiteSpace(input.title)) throw Oops.Oh("title不能为空");

        if (input.id.HasValue && input.id.Value > 0)
        {
            var e = await _db.Queryable<BubbleThemeEntity>().SingleAsync(x => x.Id == input.id.Value);
            if (e == null) throw Oops.Oh("主题不存在");

            e.ThemeKey = input.themeKey.Trim();
            e.Title = input.title.Trim();
            e.CoverUrl = (input.coverUrl ?? string.Empty).Trim();
            e.Description = (input.description ?? string.Empty).Trim();
            e.Sort = input.sort;
            e.Enabled = input.enabled;
            await _db.Updateable(e).ExecuteCommandAsync();
            return ToOutput(e);
        }
        else
        {
            var e = new BubbleThemeEntity
            {
                ThemeKey = input.themeKey.Trim(),
                Title = input.title.Trim(),
                CoverUrl = (input.coverUrl ?? string.Empty).Trim(),
                Description = (input.description ?? string.Empty).Trim(),
                Sort = input.sort,
                Enabled = input.enabled,
                CreateTime = DateTime.Now
            };
            var id = await _db.Insertable(e).ExecuteReturnIdentityAsync();
            e.Id = id;
            return ToOutput(e);
        }
    }

    [HttpPost("delete")]
    [Consumes("application/json")]
    public async Task<dynamic> Delete([FromBody] dynamic body)
    {
        long id = (long)(body?.id ?? 0);
        if (id <= 0) throw Oops.Oh("id无效");
        await _db.Deleteable<BubbleThemeEntity>().Where(x => x.Id == id).ExecuteCommandAsync();
        return new { ok = true };
    }

    private static AdminThemeOutput ToOutput(BubbleThemeEntity e)
        => new AdminThemeOutput
        {
            id = e.Id,
            themeKey = e.ThemeKey,
            title = e.Title,
            coverUrl = e.CoverUrl,
            description = e.Description,
            sort = e.Sort,
            enabled = e.Enabled,
            createTime = e.CreateTime
        };
}

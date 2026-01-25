namespace AI.BubbleEnglish;

using QT.Common.Core;
using QT.Common.Core.Security;

/// <summary>
/// 后台：主题管理
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Theme", Order = 2050)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminThemeService : QTBaseService<BubbleThemeEntity, AdminThemeUpsertInput, AdminThemeUpsertInput, AdminThemeOutput, AdminThemeQuery, AdminThemeOutput>, IDynamicApiController, ITransient
{
    public BubbleAdminThemeService(
        ISqlSugarRepository<BubbleThemeEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager) : base(repository, context, userManager)
    {
    }

    protected override Task<SqlSugarPagedList<AdminThemeOutput>> GetPageList([FromQuery] AdminThemeQuery input)
    {
        var q = _repository.Context.Queryable<BubbleThemeEntity>();
        if (!string.IsNullOrWhiteSpace(input.keyword))
        {
            var kw = input.keyword.Trim();
            q = q.Where(x => x.ThemeKey.Contains(kw) || x.Title.Contains(kw));
        }

        return q.OrderBy(x => x.Sort).OrderByDescending(x => x.Id)
            .Select<AdminThemeOutput>()
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }

    [HttpGet("detail")]
    public async Task<AdminThemeOutput> Detail([FromQuery] string id)
    {
        var e = await _repository.Context.Queryable<BubbleThemeEntity>().SingleAsync(x => x.Id == id);
        if (e == null) throw Oops.Oh("主题不存在");
        return ToOutput(e);
    }

    [HttpPost("upsert")]
    [Consumes("application/json")]
    public async Task<AdminThemeOutput> Upsert([FromBody] AdminThemeUpsertInput input)
    {
        if (string.IsNullOrWhiteSpace(input.themeKey)) throw Oops.Oh("themeKey不能为空");
        if (string.IsNullOrWhiteSpace(input.title)) throw Oops.Oh("title不能为空");

        var id = (input.id ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(id) && id != "0")
        {
            var e = await _repository.Context.Queryable<BubbleThemeEntity>().SingleAsync(x => x.Id == id);
            if (e == null) throw Oops.Oh("主题不存在");

            e.ThemeKey = input.themeKey.Trim();
            e.Title = input.title.Trim();
            e.CoverUrl = (input.coverUrl ?? string.Empty).Trim();
            e.Description = (input.description ?? string.Empty).Trim();
            e.Sort = input.sort;
            e.Enabled = input.enabled;
            await _repository.Context.Updateable(e).ExecuteCommandAsync();
            return ToOutput(e);
        }

        var entity = new BubbleThemeEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            ThemeKey = input.themeKey.Trim(),
            Title = input.title.Trim(),
            CoverUrl = (input.coverUrl ?? string.Empty).Trim(),
            Description = (input.description ?? string.Empty).Trim(),
            Sort = input.sort,
            Enabled = input.enabled,
            CreateTime = DateTime.Now
        };
        await _repository.Context.Insertable(entity).ExecuteCommandAsync();
        return ToOutput(entity);
    }

    [HttpPost("delete")]
    [Consumes("application/json")]
    public async Task<dynamic> Delete([FromBody] dynamic body)
    {
        string id = (body?.id ?? "").ToString();
        if (string.IsNullOrWhiteSpace(id)) throw Oops.Oh("id无效");
        await _repository.Context.Deleteable<BubbleThemeEntity>().Where(x => x.Id == id).ExecuteCommandAsync();
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

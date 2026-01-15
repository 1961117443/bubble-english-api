namespace AI.BubbleEnglish;

using QT.Common.Core;
using QT.Common.Core.Security;

/// <summary>
/// 后台：课程管理（course v2 的容器，units 编排可另做表或 JSON 字段）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Course", Order = 2040)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminCourseService : QTBaseService<BubbleCourseEntity, AdminCourseUpsertInput, AdminCourseUpsertInput, AdminCourseOutput, AdminCourseQuery, AdminCourseOutput>, IDynamicApiController, ITransient
{
    public BubbleAdminCourseService(
        ISqlSugarRepository<BubbleCourseEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager) : base(repository, context, userManager)
    {
    }

    protected override Task<SqlSugarPagedList<AdminCourseOutput>> GetPageList([FromQuery] AdminCourseQuery input)
    {
        var q = _repository.Context.Queryable<BubbleCourseEntity>();
        if (!string.IsNullOrWhiteSpace(input.keyword))
        {
            var kw = input.keyword.Trim();
            q = q.Where(x => x.Title.Contains(kw) || x.Description.Contains(kw));
        }
        if (input.isPublish.HasValue)
        {
            q = q.Where(x => x.IsPublish == input.isPublish.Value);
        }
        // themeKey 目前 BubbleCourseEntity 没字段，先忽略（后续可加 ThemeKey 字段或关联表）
        return q.OrderByDescending(x => x.Id)
            .Select<AdminCourseOutput>()
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }

    [HttpGet("detail")]
    public async Task<AdminCourseOutput> Detail([FromQuery] string id)
    {
        var e = await _repository.Context.Queryable<BubbleCourseEntity>().SingleAsync(x => x.Id == id);
        if (e == null) throw Oops.Oh("课程不存在");
        return ToOutput(e);
    }

    [HttpPost("upsert")]
    [Consumes("application/json")]
    public async Task<AdminCourseOutput> Upsert([FromBody] AdminCourseUpsertInput input)
    {
        if (string.IsNullOrWhiteSpace(input.title)) throw Oops.Oh("title不能为空");

        var id = (input.id ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(id) && id != "0")
        {
            var e = await _repository.Context.Queryable<BubbleCourseEntity>().SingleAsync(x => x.Id == id);
            if (e == null) throw Oops.Oh("课程不存在");

            e.Title = input.title.Trim();
            e.Cover = (input.cover ?? string.Empty).Trim();
            e.Description = (input.description ?? string.Empty).Trim();
            e.Sort = input.sort;
            e.IsPublish = input.isPublish;
            await _repository.Context.Updateable(e).ExecuteCommandAsync();
            return ToOutput(e);
        }

        var entity = new BubbleCourseEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            Title = input.title.Trim(),
            Cover = (input.cover ?? string.Empty).Trim(),
            Description = (input.description ?? string.Empty).Trim(),
            Sort = input.sort,
            IsPublish = input.isPublish,
            CreateTime = DateTime.Now
        };
        await _repository.Context.Insertable(entity).ExecuteCommandAsync();
        return ToOutput(entity);
    }

    [HttpPost("actions/publish")]
    [Consumes("application/json")]
    public async Task<dynamic> SetPublish([FromBody] dynamic body)
    {
        var id = (body?.id ?? "").ToString();
        int isPublish = (int)(body?.isPublish ?? 0);
        if (string.IsNullOrWhiteSpace(id)) throw Oops.Oh("id无效");
        await _repository.Context.Updateable<BubbleCourseEntity>()
            .SetColumns(x => x.IsPublish == isPublish)
            .Where(x => x.Id == id)
            .ExecuteCommandAsync();
        return new { ok = true };
    }

    [HttpPost("delete")]
    [Consumes("application/json")]
    public async Task<dynamic> Delete([FromBody] dynamic body)
    {
        var id = (body?.id ?? "").ToString();
        if (string.IsNullOrWhiteSpace(id)) throw Oops.Oh("id无效");
        await _repository.Context.Deleteable<BubbleCourseEntity>().Where(x => x.Id == id).ExecuteCommandAsync();
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

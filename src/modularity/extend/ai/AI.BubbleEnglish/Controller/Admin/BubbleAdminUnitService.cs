

namespace AI.BubbleEnglish;

/// <summary>
/// 后台：Unit 素材池管理
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Unit", Order = 2030)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminUnitService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;

    public BubbleAdminUnitService(ISqlSugarClient context)
    {
        _db = (SqlSugarClient)context;
    }

    [HttpGet("")]
    public async Task<List<AdminUnitOutput>> List([FromQuery] AdminUnitQuery query)
    {
        var q = _db.Queryable<BubbleUnitEntity>();
        if (query.videoId.HasValue && query.videoId.Value > 0)
            q = q.Where(x => x.VideoId == query.videoId.Value);
        if (!string.IsNullOrWhiteSpace(query.unitType))
            q = q.Where(x => x.UnitType == query.unitType!.Trim());
        if (!string.IsNullOrWhiteSpace(query.status))
            q = q.Where(x => x.Status == query.status!.Trim());
        if (!string.IsNullOrWhiteSpace(query.keyword))
        {
            var kw = query.keyword.Trim();
            q = q.Where(x => x.Text.Contains(kw) || x.Meaning.Contains(kw));
        }

        var list = await q.OrderByDescending(x => x.Id).ToListAsync();
        return list.Select(ToOutput).ToList();
    }

    [HttpGet("detail")]
    public async Task<AdminUnitOutput> Detail([FromQuery] long id)
    {
        var e = await _db.Queryable<BubbleUnitEntity>().SingleAsync(x => x.Id == id);
        if (e == null) throw Oops.Oh("unit不存在");
        return ToOutput(e);
    }

    /// <summary>
    /// 新增/更新
    /// </summary>
    [HttpPost("upsert")]
    [Consumes("application/json")]
    public async Task<AdminUnitOutput> Upsert([FromBody] AdminUnitUpsertInput input)
    {
        if (string.IsNullOrWhiteSpace(input.text)) throw Oops.Oh("text不能为空");
        if (string.IsNullOrWhiteSpace(input.unitType)) throw Oops.Oh("unitType不能为空");

        if (input.id.HasValue && input.id.Value > 0)
        {
            var e = await _db.Queryable<BubbleUnitEntity>().SingleAsync(x => x.Id == input.id.Value);
            if (e == null) throw Oops.Oh("unit不存在");

            e.VideoId = input.videoId;
            e.UnitType = input.unitType.Trim();
            e.Text = input.text.Trim();
            e.Meaning = (input.meaning ?? string.Empty).Trim();
            e.MinAge = input.minAge;
            e.ImageUrl = (input.imageUrl ?? string.Empty).Trim();
            e.AudioUrl = (input.audioUrl ?? string.Empty).Trim();
            e.Status = (input.status ?? "draft").Trim();
            e.UpdateTime = DateTime.Now;

            await _db.Updateable(e).ExecuteCommandAsync();
            return ToOutput(e);
        }
        else
        {
            var e = new BubbleUnitEntity
            {
                VideoId = input.videoId,
                UnitType = input.unitType.Trim(),
                Text = input.text.Trim(),
                Meaning = (input.meaning ?? string.Empty).Trim(),
                MinAge = input.minAge,
                ImageUrl = (input.imageUrl ?? string.Empty).Trim(),
                AudioUrl = (input.audioUrl ?? string.Empty).Trim(),
                Status = (input.status ?? "draft").Trim(),
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
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
        await _db.Deleteable<BubbleUnitEntity>().Where(x => x.Id == id).ExecuteCommandAsync();
        return new { ok = true };
    }

    private static AdminUnitOutput ToOutput(BubbleUnitEntity e)
        => new AdminUnitOutput
        {
            id = e.Id,
            videoId = e.VideoId,
            unitType = e.UnitType,
            text = e.Text,
            meaning = e.Meaning,
            minAge = e.MinAge,
            imageUrl = e.ImageUrl,
            audioUrl = e.AudioUrl,
            status = e.Status,
            createTime = e.CreateTime,
            updateTime = e.UpdateTime
        };
}

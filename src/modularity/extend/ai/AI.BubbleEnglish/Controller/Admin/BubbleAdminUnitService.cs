

namespace AI.BubbleEnglish;

using QT.Common.Core.Security;

/// <summary>
///后台：Unit 素材池管理
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
        if (!string.IsNullOrWhiteSpace(query.videoId))
        {
            var vid = query.videoId.Trim();
            q = q.Where(x => x.VideoId == vid);
        }
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
    public async Task<AdminUnitOutput> Detail([FromQuery] string id)
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

        if (!string.IsNullOrWhiteSpace(input.id))
        {
            var id = input.id.Trim();
            var e = await _db.Queryable<BubbleUnitEntity>().SingleAsync(x => x.Id == id);
            if (e == null) throw Oops.Oh("unit不存在");

            e.VideoId = string.IsNullOrWhiteSpace(input.videoId) ? null : input.videoId.Trim();
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
                Id = SnowflakeIdHelper.NextId(),
                VideoId = string.IsNullOrWhiteSpace(input.videoId) ? null : input.videoId.Trim(),
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
            await _db.Insertable(e).ExecuteCommandAsync();
            return ToOutput(e);
        }
    }

    [HttpPost("delete")]
    [Consumes("application/json")]
    public async Task<dynamic> Delete([FromBody] dynamic body)
    {
        string id = (string)(body?.id ?? "");
        if (string.IsNullOrWhiteSpace(id)) throw Oops.Oh("id鏃犳晥");
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


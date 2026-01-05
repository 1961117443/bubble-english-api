using AI.BubbleEnglish.Dto;
using AI.BubbleEnglish.Entitys;
using Microsoft.AspNetCore.Mvc;
using QT;
using QT.Common.Const;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.FriendlyException;
using SqlSugar;
using System.Globalization;

namespace AI.BubbleEnglish.Controller;

/// <summary>
/// 学习统计（日/周/月）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleEnglish", Name = "Stats", Order = 40)]
[Route("api/bubbleEnglish/learn/stats")]
public class StatsController : IDynamicApiController
{
    private readonly ISqlSugarRepository<BubbleLearningRecordEntity> _recordRepo;
    private readonly ISqlSugarRepository<BubbleChildProfileEntity> _childRepo;

    public StatsController(ISqlSugarRepository<BubbleLearningRecordEntity> recordRepo, ISqlSugarRepository<BubbleChildProfileEntity> childRepo)
    {
        _recordRepo = recordRepo;
        _childRepo = childRepo;
    }

    private static long GetParentId()
    {
        var idObj = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
        if (long.TryParse(idObj, out var id)) return id;
        throw Oops.Oh("未登录");
    }

    private async Task EnsureChildOwner(long parentId, long childId)
    {
        var child = await _childRepo.AsQueryable().FirstAsync(x => x.Id == childId);
        if (child == null || child.ParentId != parentId) throw Oops.Oh("孩子不存在或无权限");
    }

    /// <summary>
    /// 日统计：date=YYYY-MM-DD
    /// </summary>
    [HttpGet("daily")]
    public async Task<LearnStatsResp> Daily([FromQuery] long childId, [FromQuery] string date)
    {
        var parentId = GetParentId();
        await EnsureChildOwner(parentId, childId);
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            throw Oops.Oh("date 格式应为 YYYY-MM-DD");

        var start = d.Date;
        var end = start.AddDays(1);
        return await Aggregate(parentId, childId, start, end, "daily", date);
    }

    /// <summary>
    /// 周统计：week=YYYY-WW（ISO 周）
    /// </summary>
    [HttpGet("weekly")]
    public async Task<LearnStatsResp> Weekly([FromQuery] long childId, [FromQuery] string week)
    {
        var parentId = GetParentId();
        await EnsureChildOwner(parentId, childId);
        var parts = week.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var wk))
            throw Oops.Oh("week 格式应为 YYYY-WW");

        var start = ISOWeek.ToDateTime(year, wk, DayOfWeek.Monday).Date;
        var end = start.AddDays(7);
        return await Aggregate(parentId, childId, start, end, "weekly", week);
    }

    /// <summary>
    /// 月统计：month=YYYY-MM
    /// </summary>
    [HttpGet("monthly")]
    public async Task<LearnStatsResp> Monthly([FromQuery] long childId, [FromQuery] string month)
    {
        var parentId = GetParentId();
        await EnsureChildOwner(parentId, childId);
        if (!DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var m))
            throw Oops.Oh("month 格式应为 YYYY-MM");

        var start = new DateTime(m.Year, m.Month, 1);
        var end = start.AddMonths(1);
        return await Aggregate(parentId, childId, start, end, "monthly", month);
    }

    private async Task<LearnStatsResp> Aggregate(long parentId, long childId, DateTime start, DateTime end, string type, string key)
    {
        var q = _recordRepo.AsQueryable()
            .Where(x => x.ParentId == parentId && x.ChildId == childId && x.LearnTime >= start && x.LearnTime < end);

        var list = await q.ToListAsync();
        var sessions = list.Count;
        var durationMs = list.Sum(x => (long)x.Duration * 1000);
        var words = list.Sum(x => x.WordCount ?? 0);
        var sentences = list.Sum(x => x.SentenceCount ?? 0);
        var learnDays = list.Select(x => x.LearnTime.Date).Distinct().Count();

        return new LearnStatsResp
        {
            ChildId = childId,
            Type = type,
            Key = key,
            LearnDays = learnDays,
            Sessions = sessions,
            DurationMs = durationMs,
            Words = words,
            Sentences = sentences,
        };
    }
}

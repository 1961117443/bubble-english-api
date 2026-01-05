using AI.BubbleEnglish.Dto;
using AI.BubbleEnglish.Entitys;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Core.Manager;
using QT.DependencyInjection;
using QT.DynamicApiController;
using SqlSugar;

namespace AI.BubbleEnglish;

/// <summary>
/// 学习统计（日/周/月）
/// </summary>
[ApiDescriptionSettings(Tag = "BubbleEnglish", Name = "Stats", Order = 1030)]
[Route("api/BubbleEnglish/[controller]")]
public class BubbleStatsService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;
    private readonly IUserManager _userManager;

    public BubbleStatsService(ISqlSugarClient context, IUserManager userManager)
    {
        _db = (SqlSugarClient)context;
        _userManager = userManager;
    }

    [HttpGet("daily")]
    public async Task<StatsOutput> Daily([FromQuery] long childId, [FromQuery] string date)
    {
        if (!DateTime.TryParse(date, out var d)) throw Oops.Oh("date 格式错误");
        var start = d.Date;
        var end = start.AddDays(1);
        return await BuildStats(childId, "daily", start, end, start.ToString("yyyy-MM-dd"));
    }

    [HttpGet("weekly")]
    public async Task<StatsOutput> Weekly([FromQuery] long childId, [FromQuery] string week)
    {
        // week: yyyy-Www or yyyy-ww (兼容)
        var (start, end, key) = ParseWeek(week);
        return await BuildStats(childId, "weekly", start, end, key);
    }

    [HttpGet("monthly")]
    public async Task<StatsOutput> Monthly([FromQuery] long childId, [FromQuery] string month)
    {
        // month: yyyy-MM
        if (month.Length < 7) throw Oops.Oh("month 格式错误");
        if (!int.TryParse(month.Substring(0, 4), out var y)) throw Oops.Oh("month 格式错误");
        if (!int.TryParse(month.Substring(5, 2), out var m)) throw Oops.Oh("month 格式错误");
        var start = new DateTime(y, m, 1);
        var end = start.AddMonths(1);
        return await BuildStats(childId, "monthly", start, end, start.ToString("yyyy-MM"));
    }

    private async Task<StatsOutput> BuildStats(long childId, string type, DateTime start, DateTime end, string key)
    {
        if (childId <= 0) throw Oops.Oh("childId 无效");
        long uid = _userManager.UserId;

        bool owned = await _db.Queryable<BubbleChildProfileEntity>()
            .Where(x => x.Id == childId && x.ParentId == uid)
            .AnyAsync();
        if (!owned) throw Oops.Oh("无权限访问该孩子");

        var records = await _db.Queryable<BubbleLearningRecordEntity>()
            .Where(x => x.ParentId == uid && x.ChildId == childId && x.LearnTime >= start && x.LearnTime < end)
            .ToListAsync();

        int sessions = records.Count;
        int learnDays = records.Select(r => r.LearnTime.Date).Distinct().Count();
        int words = records.Sum(r => r.WordCount ?? 0);
        int sentences = records.Sum(r => r.SentenceCount ?? 0);
        long durationMs = records.Sum(r => (long)r.Duration * 1000);

        return new StatsOutput
        {
            childId = childId,
            type = type,
            key = key,
            sessions = sessions,
            learnDays = learnDays,
            words = words,
            sentences = sentences,
            durationMs = durationMs
        };
    }

    private static (DateTime start, DateTime end, string key) ParseWeek(string week)
    {
        // 支持：2026-01 或 2026-W01
        string w = week.Replace("W", "").Replace("w", "");
        var parts = w.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) throw Oops.Oh("week 格式错误");
        if (!int.TryParse(parts[0], out int year)) throw Oops.Oh("week 格式错误");
        if (!int.TryParse(parts[1], out int wk)) throw Oops.Oh("week 格式错误");
        if (wk < 1 || wk > 53) throw Oops.Oh("week 超出范围");

        // ISO week start (Monday)
        DateTime jan4 = new DateTime(year, 1, 4);
        int dayOfWeek = (int)jan4.DayOfWeek;
        if (dayOfWeek == 0) dayOfWeek = 7;
        DateTime week1Monday = jan4.AddDays(1 - dayOfWeek);
        DateTime start = week1Monday.AddDays((wk - 1) * 7);
        DateTime end = start.AddDays(7);
        string key = $"{year}-W{wk:00}";
        return (start, end, key);
    }
}

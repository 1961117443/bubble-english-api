using AI.BubbleEnglish.Dto;
using AI.BubbleEnglish.Entitys;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Core.Manager;
using QT.DependencyInjection;
using QT.DynamicApiController;
using SqlSugar;

namespace AI.BubbleEnglish;

/// <summary>
/// 学习上报（前端 LearnReport）
/// </summary>
[ApiDescriptionSettings(Tag = "BubbleEnglish", Name = "Learn", Order = 1020)]
[Route("api/BubbleEnglish/[controller]")]
public class BubbleLearnService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;
    private readonly IUserManager _userManager;

    public BubbleLearnService(ISqlSugarClient context, IUserManager userManager)
    {
        _db = (SqlSugarClient)context;
        _userManager = userManager;
    }

    /// <summary>
    /// 提交一次学习报告（Reward done 时调用）
    /// </summary>
    [HttpPost("reports")]
    [Consumes("application/json")]
    public async Task<LearnReportCreateOutput> SubmitReport([FromBody] LearnReportInput input)
    {
        if (input.childId <= 0) throw Oops.Oh("childId 无效");

        long uid = _userManager.UserId;

        // 防止越权：childId 必须属于当前用户
        bool owned = await _db.Queryable<BubbleChildProfileEntity>()
            .Where(x => x.Id == input.childId && x.ParentId == uid)
            .AnyAsync();
        if (!owned) throw Oops.Oh("无权限访问该孩子");

        var entity = new BubbleLearningRecordEntity
        {
            ParentId = uid,
            ChildId = input.childId,
            CourseId = input.courseId ?? 0,
            LessonId = null,
            Duration = (int)Math.Max(0, (input.durationMs ?? 0) / 1000),
            WordCount = input.wordCount,
            SentenceCount = input.sentenceCount,
            Mode = input.mode,
            ReportJson = input.reportJson,
            LearnTime = DateTime.Now
        };

        long id = await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        return new LearnReportCreateOutput { reportId = id };
    }
}

using AI.BubbleEnglish.Dto;
using AI.BubbleEnglish.Entitys;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QT;
using QT.Common.Const;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.FriendlyException;
using SqlSugar;

namespace AI.BubbleEnglish.Controller;

/// <summary>
/// 学习上报（保存一次学习会话）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleEnglish", Name = "Learn", Order = 30)]
[Route("api/bubbleEnglish/learn")]
public class LearnController : IDynamicApiController
{
    private readonly ISqlSugarRepository<BubbleLearningRecordEntity> _recordRepo;
    private readonly ISqlSugarRepository<BubbleChildProfileEntity> _childRepo;

    public LearnController(ISqlSugarRepository<BubbleLearningRecordEntity> recordRepo, ISqlSugarRepository<BubbleChildProfileEntity> childRepo)
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

    /// <summary>
    /// 提交学习报告（前端 LearnReport）
    /// </summary>
    [HttpPost("reports")]
    public async Task<LearnReportSubmitResp> SubmitReport([FromBody] LearnReportSubmitReq req)
    {
        var parentId = GetParentId();
        if (req.ChildId <= 0) throw Oops.Oh("childId 不能为空");
        if (string.IsNullOrWhiteSpace(req.CourseId)) throw Oops.Oh("courseId 不能为空");

        var child = await _childRepo.AsQueryable().FirstAsync(x => x.Id == req.ChildId);
        if (child == null || child.ParentId != parentId) throw Oops.Oh("孩子不存在或无权限");

        var durationSec = (int)Math.Max(0, (req.DurationMs ?? 0) / 1000);
        var json = req.Raw == null ? null : JsonConvert.SerializeObject(req.Raw);

        var entity = new BubbleLearningRecordEntity
        {
            ParentId = parentId,
            ChildId = req.ChildId,
            CourseId = long.TryParse(req.CourseId, out var cid) ? cid : 0,
            LessonId = 0,
            Duration = durationSec,
            WordCount = req.WordCount,
            SentenceCount = req.SentenceCount,
            Mode = req.Mode,
            ReportJson = json,
            LearnTime = DateTime.Now,
        };

        entity.Id = await _recordRepo.InsertReturnIdentityAsync(entity);
        return new LearnReportSubmitResp { RecordId = entity.Id };
    }
}

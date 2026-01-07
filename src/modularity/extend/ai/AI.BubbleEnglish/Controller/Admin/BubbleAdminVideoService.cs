using QT.Common.Core;
using QT.Common.Core.Security;
using QT.Common.Extension;
using QT.Common.Filter;
using Yitter.IdGenerator;

namespace AI.BubbleEnglish;

/// <summary>
/// 后台：视频库（上传记录/列表/详情/触发AI分析）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Video", Order = 2010)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminVideoService : QTBaseService<BubbleVideoEntity, AdminVideoCreateInput, AdminVideoUpdateInput, AdminVideoOutput, AdminVideoQuery, AdminVideoOutput>, IDynamicApiController, ITransient
{
    public BubbleAdminVideoService(ISqlSugarRepository<BubbleVideoEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }


    protected override Task<SqlSugarPagedList<AdminVideoOutput>> GetPageList([FromQuery] AdminVideoQuery input)
    {
        return _repository.Context.Queryable<BubbleVideoEntity>()
            .WhereIF(input.keyword.IsNotEmptyOrNull(),
                x => x.Title.Contains(input.keyword!) || x.FileUrl.Contains(input.keyword!))
           .OrderByDescending(x => x.Id)
           .Select<AdminVideoOutput>()
           .ToPagedListAsync(input.currentPage, input.pageSize);
    }
     
    /// <summary>
    /// 触发 AI 分析任务（这里只落库 queued；真正AI调用建议放后台Job/队列）
    /// </summary>
    [HttpPost("actions/analyze")]
    public async Task<dynamic> Analyze([FromBody] AdminAiAnalyzeInput input)
    {
        var v = await _repository.Context.Queryable<BubbleVideoEntity>().SingleAsync(x => x.Id == input.videoId);
        if (v == null) throw Oops.Oh("视频不存在");

        var job = new BubbleAiJobEntity
        {
            VideoId = v.Id,
            Status = "queued",
            Provider = string.IsNullOrWhiteSpace(input.provider) ? null : input.provider.Trim(),
            Model = (input.model ?? string.Empty).Trim(),
            Prompt = (input.prompt ?? string.Empty).Trim(),
            CreateTime = DateTime.Now,
            OutputJson = string.Empty,
            ErrorMessage = string.Empty
        };
        var jobId = await _repository.Context.Insertable(job).ExecuteReturnIdentityAsync();
        job.Id = jobId;

        v.Status = "analyzing";
        v.AnalyzeJobId = jobId;
        v.UpdateTime = DateTime.Now;
        await _repository.Context.Updateable<BubbleVideoEntity>(v).UpdateColumns(x => new { x.Status, x.AnalyzeJobId, x.UpdateTime }).ExecuteCommandAsync();

        return new { ok = true, jobId };
    }
}

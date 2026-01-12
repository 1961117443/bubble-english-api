using QT.Common.Core;
using QT.Common.Core.Security;
using QT.Common.Extension;
using QT.Common.Filter;
using Yitter.IdGenerator;
using AI.BubbleEnglish.Infrastructure.Storage;
using Microsoft.AspNetCore.Http;

namespace AI.BubbleEnglish;

/// <summary>
/// 后台：视频库（上传记录/列表/详情/触发AI分析）
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Video", Order = 2010)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminVideoService : QTBaseService<BubbleVideoEntity, AdminVideoCreateInput, AdminVideoUpdateInput, AdminVideoOutput, AdminVideoQuery, AdminVideoOutput>, IDynamicApiController, ITransient
{
    private readonly IBubbleStorageService _storage;

    public BubbleAdminVideoService(
        ISqlSugarRepository<BubbleVideoEntity> repository,
        ISqlSugarClient context,
        IUserManager userManager,
        IBubbleStorageService storage) : base(repository, context, userManager)
    {
        _storage = storage;
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

    /// <summary>
    /// 创建视频（携带文件上传，保存到本地，确保 ASR/AI 可直接 File.Exists 读取）
    /// Admin 前端 Form.vue 会优先走该接口。
    /// </summary>
    [HttpPost("actions/createWithUpload")]
    [RequestSizeLimit(long.MaxValue)]
    public async Task<dynamic> CreateWithUpload([FromForm] AdminVideoCreateWithUploadInput input)
    {
        if (input.file == null || input.file.Length == 0)
            throw Oops.Oh("未选择视频文件");

        var now = DateTime.Now;
        var videoId = SnowflakeIdHelper.NextId(); //  Guid.NewGuid().ToString("N");
        var ext = Path.GetExtension(input.file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".mp4";

        // 保存到 {Root}/bubble/videos/{yyyy}/{MM}/{videoId}/original.xxx
        var workDir = _storage.GetVideoWorkDir(videoId, now);
        Directory.CreateDirectory(workDir);
        var originalPath = _storage.GetVideoOriginalPath(videoId, ext, now);

        await using (var fs = new FileStream(originalPath, FileMode.Create, FileAccess.Write))
        {
            await input.file.CopyToAsync(fs);
        }

        var entity = new BubbleVideoEntity
        {
            Id = videoId,
            Title = (input.title ?? string.Empty).Trim(),
            ThemeKey = (input.themeKey ?? string.Empty).Trim(),
            FileUrl = originalPath, // 这里先存绝对路径，确保后续作业可直接读取
            LocalPath = originalPath,
            CoverUrl = string.Empty,
            DurationSec = 0,
            Status = "uploaded",
            WorkDir = workDir,
            CreateTime = now,
            UpdateTime = now
        };

        await _repository.Context.Insertable(entity).ExecuteCommandAsync();

        return new
        {
            code = 200,
            msg = "上传成功",
            data = new { id = entity.Id, fileUrl = entity.FileUrl }
        };
    }
}

public class AdminVideoCreateWithUploadInput
{
    public string? title { get; set; }
    public string? themeKey { get; set; }
    public string? ageRange { get; set; }
    public string? remark { get; set; }
    public IFormFile? file { get; set; }
}

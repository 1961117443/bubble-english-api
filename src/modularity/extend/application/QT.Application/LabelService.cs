using QT.Common.Core;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extend.Entitys.Dto.ProjectGantt;
using QT.Extend.Entitys.Dto.Label;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using QT.Common.Core.Security;

namespace QT.Application;

/// <summary>
    /// 标签管理
    /// </summary>
[ApiDescriptionSettings("乾通ERP.V2",Tag = "标签管理", Name = "Label", Order = 600)]
[Route("api/extend/[controller]")]
public class LabelService : QTBaseService<LabelEntity, LabelCrInput, LabelUpInput, LabelInfoOutput, LabelListPageInput, LabelListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<LabelEntity> _repository;
    private readonly IUserManager _userManager;

    public LabelService(ISqlSugarRepository<LabelEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    protected override async Task BeforeCreate(LabelCrInput input, LabelEntity entity)
    {
        // 同一个用户，同一个类别，标签不能重复
        if (await _repository.AnyAsync(x => x.CreatorUserId == _userManager.UserId && x.FullName == input.fullName && x.Category == input.category))
        {
            throw Oops.Oh("标签已存在！");
        }
        await base.BeforeCreate(input, entity);
    }

    protected override async Task BeforeUpdate(LabelUpInput input, LabelEntity entity)
    {
        // 同一个用户，同一个类别，标签不能重复
        if (await _repository.AnyAsync(x => x.Id != entity.Id && x.CreatorUserId == _userManager.UserId && x.FullName == input.fullName && x.Category == input.category))
        {
            throw Oops.Oh("标签已存在！");
        }
        await base.BeforeUpdate(input, entity);
    }

    /// <summary>
    /// 获取标签集合
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpGet("{category}/list")]
    public async Task<List<LabelListOutput>> GetList(string category, [FromQuery] LabelListPageInput input)
    {
        return await _repository.Where(x => x.Category == category)
            .Where(x => x.CreatorUserId == _userManager.UserId)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), x => x.FullName.Contains(input.keyword))
            .Select<LabelListOutput>().ToListAsync();
    }

    #region 个人标签
    /// <summary>
    /// 创建个人标签集合
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPost("{category}/user/{userid}")]
    public async Task CreateUserLabel(string category, string userid, [FromBody] List<string> input)
    {
        var entity = await _repository.Where(x => x.CreatorUserId == userid && x.Category == category && x.IsGlobal == 0).FirstAsync();
        var label = input.IsAny() ? string.Join(",", input.Distinct()) : "";
        if (entity != null)
        {
            entity.Label = label;
            await _repository.Context.Updateable(entity).UpdateColumns(x => x.Label).ExecuteCommandAsync();
        }
        else
        {
            entity = new LabelEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                Label = label,
                Category = category,
                CreatorUserId = userid,
                IsGlobal = 0
            };
            await _repository.Context.Insertable(entity).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 获取个人标签集合
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpGet("{category}/user/{userid}")]
    public async Task<List<string>> GetUserLabel(string category, string userid)
    {
        var entity = await _repository.Where(x => x.CreatorUserId == userid && x.Category == category && x.IsGlobal == 0).FirstAsync();

        if (entity != null && entity.Label.IsNotEmptyOrNull())
        {
            return entity.Label.Split(",", true).ToList();
        }

        return new List<string>();
    }
    #endregion

    #region 系统标签（全局通用）
    /// <summary>
    /// 创建个人标签集合
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpPost("{category}/global")]
    public async Task CreateGlobalLabel(string category, [FromBody] List<string> input)
    {
        var entity = await _repository.Where(x => x.Category == category && x.IsGlobal == 1).FirstAsync();
        var label = input.IsAny() ? string.Join(",", input.Distinct()) : "";
        if (entity != null)
        {
            entity.Label = label;
            await _repository.Context.Updateable(entity).UpdateColumns(x => x.Label).ExecuteCommandAsync();
        }
        else
        {
            entity = new LabelEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                Label = label,
                Category = category,
                CreatorUserId = _userManager.UserId,
                IsGlobal = 1
            };
            await _repository.Context.Insertable(entity).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 获取个人标签集合
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpGet("{category}/global")]
    public async Task<List<string>> GetGlobalLabel(string category)
    {
        var entity = await _repository.Where(x => x.Category == category && x.IsGlobal == 1).FirstAsync();

        if (entity != null && entity.Label.IsNotEmptyOrNull())
        {
            return entity.Label.Split(",", true).ToList();
        }

        return new List<string>();
    }
    #endregion

    /// <summary>
    /// 获取当前用户标签集合
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    [HttpGet("{category}/current")]
    public async Task<dynamic> GetCurrentUserLabel(string category)
    {

        var list = await _repository.Where(x => x.Category == category && (x.CreatorUserId == _userManager.UserId && x.IsGlobal == 0 || x.IsGlobal == 1)).ToListAsync();

        // 个人标签
        var entity = list.FirstOrDefault(x => x.CreatorUserId == _userManager.UserId && x.IsGlobal == 0);

        // 全局标签
        var global = list.FirstOrDefault(x => x.IsGlobal == 1);

        return new[]
        {
            new
            {
                label="个人标签",
                isGlobal=false,
                editable=false,
                list = entity?.Label?.Split(",", true)?.ToList() ?? new List<string>()
            },
             new
            {
                label="系统标签",
                isGlobal=true,
                editable=false,
                list = global?.Label?.Split(",", true)?.ToList() ?? new List<string>()
            }
        };
    }
}

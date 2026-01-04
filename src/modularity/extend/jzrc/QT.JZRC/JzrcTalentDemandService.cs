using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcTalentDemand;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑人才需求.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcTalentDemand", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcTalentDemandService : IJzrcTalentDemandService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcTalentDemandEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcTalentDemandService"/>类型的新实例.
    /// </summary>
    public JzrcTalentDemandService(
        ISqlSugarRepository<JzrcTalentDemandEntity> jzrcTalentDemandRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcTalentDemandRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取建筑人才需求.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcTalentDemandInfoOutput>();
    }

    /// <summary>
    /// 获取建筑人才需求列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcTalentDemandListQueryInput input)
    {
        var data = await _repository.Context.Queryable<JzrcTalentDemandEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.content), it => it.Content.Contains(input.content))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Content.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcTalentDemandListOutput
            {
                id = it.Id,
                talentId = it.TalentId,
                content = it.Content,
                talentIdName = SqlFunc.Subqueryable<JzrcTalentEntity>().Where(ddd=>ddd.Id == it.TalentId).Select(ddd=>ddd.Name),
                creatorTime = it.CreatorTime
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcTalentDemandListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑人才需求.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcTalentDemandCrInput input)
    {
        var entity = input.Adapt<JzrcTalentDemandEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新建筑人才需求.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcTalentDemandUpInput input)
    {
        var entity = input.Adapt<JzrcTalentDemandEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除建筑人才需求.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<JzrcTalentDemandEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}
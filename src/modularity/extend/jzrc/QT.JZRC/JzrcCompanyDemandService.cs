using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcCompanyDemand;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.JZRC;

/// <summary>
/// 业务实现：建筑企业需求.
/// </summary>
[ApiDescriptionSettings(Tag = "JZRC", Name = "JzrcCompanyDemand", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcCompanyDemandService : IJzrcCompanyDemandService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcCompanyDemandEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcCompanyDemandService"/>类型的新实例.
    /// </summary>
    public JzrcCompanyDemandService(
        ISqlSugarRepository<JzrcCompanyDemandEntity> jzrcCompanyDemandRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcCompanyDemandRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取建筑企业需求.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcCompanyDemandInfoOutput>();
    }

    /// <summary>
    /// 获取建筑企业需求列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcCompanyDemandListQueryInput input)
    {
        var data = await _repository.Context.Queryable<JzrcCompanyDemandEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.content), it => it.Content.Contains(input.content))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Content.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcCompanyDemandListOutput
            {
                id = it.Id,
                companyId = it.CompanyId,
                content = it.Content,
                companyIdName = SqlFunc.Subqueryable<JzrcCompanyEntity>().Where(ddd => ddd.Id == it.CompanyId).Select(ddd => ddd.CompanyName),
                creatorTime = it.CreatorTime,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcCompanyDemandListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建建筑企业需求.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcCompanyDemandCrInput input)
    {
        var entity = input.Adapt<JzrcCompanyDemandEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新建筑企业需求.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcCompanyDemandUpInput input)
    {
        var entity = input.Adapt<JzrcCompanyDemandEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除建筑企业需求.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<JzrcCompanyDemandEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}
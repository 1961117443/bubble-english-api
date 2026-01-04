using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogOrderFinancialConfiguration;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：订单分账配置表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "订单分账配置管理", Name = "LogOrderFinancialConfiguration", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogOrderFinancialConfigurationService : ILogOrderFinancialConfigurationService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogOrderFinancialConfigurationEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogOrderFinancialConfigurationService"/>类型的新实例.
    /// </summary>
    public LogOrderFinancialConfigurationService(
        ISqlSugarRepository<LogOrderFinancialConfigurationEntity> logOrderFinancialConfigurationRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logOrderFinancialConfigurationRepository;
        _userManager = userManager;
    }

    #region 增删改查
    /// <summary>
    /// 获取订单分账配置表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogOrderFinancialConfigurationInfoOutput>();
    }

    /// <summary>
    /// 获取订单分账配置表列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogOrderFinancialConfigurationListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogOrderFinancialConfigurationEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogOrderFinancialConfigurationListOutput
            {
                id = it.Id,
                name = it.Name,
                platformProportion = it.PlatformProportion,
                pointProportion = it.PointProportion,
                status = it.Status ?? 0,
                description = it.Description,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogOrderFinancialConfigurationListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建订单分账配置表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogOrderFinancialConfigurationCrInput input)
    {
        var entity = input.Adapt<LogOrderFinancialConfigurationEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新订单分账配置表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogOrderFinancialConfigurationUpInput input)
    {
        var entity = input.Adapt<LogOrderFinancialConfigurationEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除订单分账配置表.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogOrderFinancialConfigurationEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion

    /// <summary>
    /// 获取第一个订单分账配置记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("getConfig")]
    public async Task<dynamic> GetConfig()
    {
        var entity = await _repository.AsQueryable().Where(it => it.Status == 1).FirstAsync();
        if (entity == null)
        {
            entity = new LogOrderFinancialConfigurationEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                Name = "默认配置",
                PlatformProportion = 0,
                Status = 1,
                PointProportion = 0
            };
            await _repository.InsertAsync(entity);
        }
        return entity.Adapt<LogOrderFinancialConfigurationInfoOutput>();
    }
}
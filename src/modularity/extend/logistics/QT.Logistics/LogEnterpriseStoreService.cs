using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogEnterpriseStore;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：入驻商铺.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "入驻商铺管理", Name = "LogEnterpriseStore", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseStoreService : ILogEnterpriseStoreService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogEnterpriseStoreEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogEnterpriseStoreService"/>类型的新实例.
    /// </summary>
    public LogEnterpriseStoreService(
        ISqlSugarRepository<LogEnterpriseStoreEntity> logEnterpriseStoreRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logEnterpriseStoreRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取入驻商铺.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogEnterpriseStoreInfoOutput>();
    }

    /// <summary>
    /// 获取入驻商铺列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseStoreListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseStoreEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.eId), it => it.EId.Equals(input.eId))
            .WhereIF(!string.IsNullOrEmpty(input.storeNumber), it => it.StoreNumber.Contains(input.storeNumber))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.EId.Contains(input.keyword)
                || it.StoreNumber.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogEnterpriseStoreListOutput
            {
                id = it.Id,
                eId = it.EId,
                storeNumber = it.StoreNumber,
                storeLocation = it.StoreLocation,
                storeArea = it.StoreArea,
                storeRent = it.StoreRent,
                leaseStartTime = it.LeaseStartTime,
                contractDuration = it.ContractDuration,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseStoreListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建入驻商铺.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogEnterpriseStoreCrInput input)
    {
        var entity = input.Adapt<LogEnterpriseStoreEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新入驻商铺.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogEnterpriseStoreUpInput input)
    {
        var entity = input.Adapt<LogEnterpriseStoreEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除入驻商铺.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogEnterpriseStoreEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}
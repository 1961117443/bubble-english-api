using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogEnterpriseFinancial;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：缴费记录.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "商家缴费记录管理", Name = "LogEnterpriseFinancial", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseFinancialService : ILogEnterpriseFinancialService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogEnterpriseFinancialEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogEnterpriseFinancialService"/>类型的新实例.
    /// </summary>
    public LogEnterpriseFinancialService(
        ISqlSugarRepository<LogEnterpriseFinancialEntity> logEnterpriseFinancialRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logEnterpriseFinancialRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取缴费记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogEnterpriseFinancialInfoOutput>();
    }

    /// <summary>
    /// 获取缴费记录列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseFinancialListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseFinancialEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.eId), it => it.EId.Equals(input.eId))
            .WhereIF(!string.IsNullOrEmpty(input.storeNumber), it => it.StoreNumber.Contains(input.storeNumber))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.EId.Contains(input.keyword)
                || it.StoreNumber.Contains(input.keyword)
                || it.No.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogEnterpriseFinancialListOutput
            {
                id = it.Id,
                eId = it.EId,
                storeNumber = it.StoreNumber,
                amount = it.Amount,
                paymentMethod = it.PaymentMethod,
                no = it.No,
                remark = it.Remark,
                creatorTime=it.CreatorTime
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseFinancialListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建缴费记录.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogEnterpriseFinancialCrInput input)
    {
        var entity = input.Adapt<LogEnterpriseFinancialEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新缴费记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogEnterpriseFinancialUpInput input)
    {
        var entity = input.Adapt<LogEnterpriseFinancialEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除缴费记录.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogEnterpriseFinancialEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}
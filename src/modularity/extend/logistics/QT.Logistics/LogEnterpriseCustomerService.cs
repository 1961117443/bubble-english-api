using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogEnterpriseCustomer;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：商家客户.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "商家客户管理", Name = "LogEnterpriseCustomer", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseCustomerService : ILogEnterpriseCustomerService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogEnterpriseCustomerEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogEnterpriseCustomerService"/>类型的新实例.
    /// </summary>
    public LogEnterpriseCustomerService(
        ISqlSugarRepository<LogEnterpriseCustomerEntity> logEnterpriseCustomerRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logEnterpriseCustomerRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取商家客户.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogEnterpriseCustomerInfoOutput>();
    }

    /// <summary>
    /// 获取商家客户列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseCustomerListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseCustomerEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogEnterpriseCustomerListOutput
            {
                id = it.Id,
                name = it.Name,
                address = it.Address,
                admin = it.Admin,
                admintel = it.Admintel,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseCustomerListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建商家客户.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogEnterpriseCustomerCrInput input)
    {
        var entity = input.Adapt<LogEnterpriseCustomerEntity>();
        if (await _repository.Where(it => it.Name == entity.Name).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新商家客户.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogEnterpriseCustomerUpInput input)
    {
        var entity = input.Adapt<LogEnterpriseCustomerEntity>();
        if (await _repository.Where(it => it.Name == entity.Name && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除商家客户.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Updateable<LogEnterpriseCustomerEntity>(new LogEnterpriseCustomerEntity { Id = id})
            .CallEntityMethod(it => it.Delete())
            .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId })
            .ExecuteCommandAsync(); 
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}
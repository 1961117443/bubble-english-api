using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogRoute;
using QT.Logistics.Entitys.Dto.LogDeliveryRoute;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：物流线路.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "物流线路管理", Name = "LogRoute", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogRouteService : ILogRouteService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogRouteEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogRouteService"/>类型的新实例.
    /// </summary>
    public LogRouteService(
        ISqlSugarRepository<LogRouteEntity> logRouteRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logRouteRepository;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    /// <summary>
    /// 获取物流线路.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogRouteInfoOutput>();

        var logDeliveryRouteList = await _repository.Context.Queryable<LogDeliveryRouteEntity>().Where(w => w.RouteId == output.id).ToListAsync();
        output.logDeliveryRouteList = logDeliveryRouteList.Adapt<List<LogDeliveryRouteInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取物流线路列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogRouteListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogRouteEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Code.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogRouteListOutput
            {
                id = it.Id,
                name = it.Name,
                code = it.Code,
                description = it.Description,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogRouteListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建物流线路.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogRouteCrInput input)
    {
        var entity = input.Adapt<LogRouteEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        if (await _repository.Where(it => it.Name == entity.Name).AnyAsync())
        {
            throw Oops.Oh("线路名称已存在！");
        }
        if (await _repository.Where(it => it.Code == entity.Code).AnyAsync())
        {
            throw Oops.Oh("线路编号已存在！");
        }
        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<LogRouteEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var logDeliveryRouteEntityList = input.logDeliveryRouteList.Adapt<List<LogDeliveryRouteEntity>>();
            if(logDeliveryRouteEntityList != null)
            {
                foreach (var item in logDeliveryRouteEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.RouteId = newEntity.Id;
                }

                await _repository.Context.Insertable<LogDeliveryRouteEntity>(logDeliveryRouteEntityList).ExecuteCommandAsync();
            }

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1000);
        }
    }

    /// <summary>
    /// 更新物流线路.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogRouteUpInput input)
    {
        var entity = input.Adapt<LogRouteEntity>();
        if (await _repository.Where(it => it.Name == entity.Name && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh("线路名称已存在！");
        }
        if (await _repository.Where(it => it.Code == entity.Code && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh("线路编号已存在！");
        }
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogRouteEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空线路覆盖配送点(中间表）原有数据
            await _repository.Context.Deleteable<LogDeliveryRouteEntity>().Where(it => it.RouteId == entity.Id).ExecuteCommandAsync();

            // 新增线路覆盖配送点(中间表）新数据
            var logDeliveryRouteEntityList = input.logDeliveryRouteList.Adapt<List<LogDeliveryRouteEntity>>();
            if(logDeliveryRouteEntityList != null)
            {
                foreach (var item in logDeliveryRouteEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.RouteId = entity.Id;
                }

                await _repository.Context.Insertable<LogDeliveryRouteEntity>(logDeliveryRouteEntityList).ExecuteCommandAsync();
            }

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1001);
        }
    }

    /// <summary>
    /// 删除物流线路.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if(!await _repository.Context.Queryable<LogRouteEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }       

        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<LogRouteEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

                // 清空线路覆盖配送点(中间表）表数据
            await _repository.Context.Deleteable<LogDeliveryRouteEntity>().Where(it => it.RouteId.Equals(entity.Id)).ExecuteCommandAsync();

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1002);
        }
    }

    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector")]
    public async Task<dynamic> Selector()
    {
        var data = await _repository.AsQueryable().Select(it => new { id = it.Id, name = it.Name }).ToListAsync();

        return new { list = data };
    }
}
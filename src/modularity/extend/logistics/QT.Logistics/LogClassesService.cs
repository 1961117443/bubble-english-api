using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogClasses;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：班次信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "班次信息管理", Name = "LogClasses", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogClassesService : ILogClassesService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogClassesEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogClassesService"/>类型的新实例.
    /// </summary>
    public LogClassesService(
        ISqlSugarRepository<LogClassesEntity> logClassesRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logClassesRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取班次信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogClassesInfoOutput>();
        output.vIdList = await _repository.Context.Queryable<LogVehicleClassesEntity>().Where(it=>it.CId == id).Select(it=>it.VId).ToListAsync();
        return output;
    }

    /// <summary>
    /// 获取班次信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogClassesListQueryInput input)
    {
        List<string> queryDepartureTime =  input.departureTime?.Split(',').ToObject<List<string>>();
        var startDepartureTime = queryDepartureTime?.First() ;
        var endDepartureTime = queryDepartureTime?.Last();

        int endDepartureTimeHour = -1;
        int endDepartureTimeMin = -1;
        int startDepartureTimeHour = -1;
        int startDepartureTimeMin = -1;
        if (!string.IsNullOrEmpty(startDepartureTime) && DateTime.TryParse(startDepartureTime,out var dt1))
        {
            startDepartureTimeHour = dt1.Hour;
            startDepartureTimeMin = dt1.Minute;
        }
        if (!string.IsNullOrEmpty(endDepartureTime) && DateTime.TryParse(endDepartureTime, out var dt2))
        {
            endDepartureTimeHour = dt2.Hour;
            endDepartureTimeMin = dt2.Minute;
        }
        var data = await _repository.Context.Queryable<LogClassesEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(startDepartureTimeHour>-1,it=> SqlFunc.DateValue(it.DepartureTime.Value, DateType.Hour) >= startDepartureTimeHour && SqlFunc.DateValue(it.DepartureTime.Value, DateType.Minute) >= startDepartureTimeMin)
            .WhereIF(endDepartureTimeHour > -1, it => SqlFunc.DateValue(it.DepartureTime.Value, DateType.Hour) <= endDepartureTimeHour && SqlFunc.DateValue(it.DepartureTime.Value, DateType.Minute) <= endDepartureTimeMin)
            //.WhereIF(queryDepartureTime != null, it => SqlFunc.Between(it.DepartureTime, startDepartureTime, endDepartureTime))
            .WhereIF(!string.IsNullOrEmpty(input.routeId), it => it.RouteId.Equals(input.routeId))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.RouteId.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogClassesListOutput
            {
                id = it.Id,
                name = it.Name,
                departureTime = it.DepartureTime,
                arrivalTime = it.ArrivalTime,
                routeId = it.RouteId,
                routeIdName = SqlFunc.Subqueryable<LogRouteEntity>().Where(x=>x.Id == it.RouteId).Select(x=>x.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogClassesListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建班次信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] LogClassesCrInput input)
    {
        var entity = input.Adapt<LogClassesEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        var logVehicleClassesEntityList = input.vIdList?.Select(it => new LogVehicleClassesEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            CId = entity.Id,
            VId = it
        }).ToList();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);

        if (logVehicleClassesEntityList.IsAny())
        {
            await _repository.Context.Insertable<LogVehicleClassesEntity>(logVehicleClassesEntityList).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 更新班次信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] LogClassesUpInput input)
    {
        var entity = input.Adapt<LogClassesEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

        // 删除关联车辆
        await _repository.Context.Deleteable<LogVehicleClassesEntity>().Where(it=>it.CId == entity.Id).ExecuteCommandAsync();
        // 重新插入关联车辆
        if (input.vIdList.IsAny())
        {
            var logVehicleClassesEntityList = input.vIdList?.Select(vid => new LogVehicleClassesEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                CId = entity.Id,
                VId = vid
            }).ToList();
            await _repository.Context.Insertable<LogVehicleClassesEntity>(logVehicleClassesEntityList).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 删除班次信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogClassesEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        // 删除关联车辆
        await _repository.Context.Deleteable<LogVehicleClassesEntity>().Where(it => it.CId == id).ExecuteCommandAsync();
    }

    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector")]
    public async Task<dynamic> Selector()
    {
        var data = await _repository.AsQueryable().Select(it => new { id = it.Id, name = it.Name, departureTime=it.DepartureTime, arrivalTime= it.ArrivalTime }).ToListAsync();

        return new { list = data };
    }

    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector/Vehicle")]
    public async Task<dynamic> SelectorWithVehicle()
    {
        var data = await _repository.AsQueryable()
            .Select(it => new
            {
                id = it.Id,
                name = it.Name,
                departureTime = it.DepartureTime,
                arrivalTime = it.ArrivalTime
            })
            .ToListAsync();

        var vidList = await _repository.Context.Queryable<LogVehicleClassesEntity>().ToListAsync();



        return new
        {
            list = data.Select(it => new
            {
                id = it.id,
                name = it.name,
                departureTime = it.departureTime,
                arrivalTime = it.arrivalTime,
                vIdList = vidList.Where(x => x.CId == it.id).Select(x => x.VId).ToList()
            }).ToList()
        };
    }

}
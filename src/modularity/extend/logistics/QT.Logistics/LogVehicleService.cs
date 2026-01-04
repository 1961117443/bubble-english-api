using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogVehicle;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Logistics.Entitys.Dto.LogEnterpriseAttachment;

namespace QT.Logistics;

/// <summary>
/// 业务实现：车辆信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics,Tag = "车辆管理", Name = "LogVehicle", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogVehicleService : ILogVehicleService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogVehicleEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="LogVehicleService"/>类型的新实例.
    /// </summary>
    public LogVehicleService(
        ISqlSugarRepository<LogVehicleEntity> logVehicleRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logVehicleRepository;
        _userManager = userManager;

        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取车辆信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id);
        var output = entity.Adapt<LogVehicleInfoOutput>();

        var logVehicleAttachmentList = await _repository.Context.Queryable<LogVehicleAttachmentEntity>().Where(w => w.VId == output.id).ToListAsync();
        output.logVehicleAttachmentList = logVehicleAttachmentList.Adapt<List<LogVehicleAttachmentInfoOutput>>();

        output.cIdList = await _repository.Context.Queryable<LogVehicleClassesEntity>().Where(w => w.VId == output.id).Select(it => it.CId).ToListAsync();
        return output;
    }

    /// <summary>
    /// 获取车辆信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogVehicleListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogVehicleEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.licensePlateNumber), it => it.LicensePlateNumber.Contains(input.licensePlateNumber))
            .WhereIF(!string.IsNullOrEmpty(input.driver), it => it.Driver.Contains(input.driver))
            .WhereIF(!string.IsNullOrEmpty(input.driverPhone), it => it.DriverPhone.Contains(input.driverPhone))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.LicensePlateNumber.Contains(input.keyword)
                || it.Driver.Contains(input.keyword)
                || it.DriverPhone.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogVehicleListOutput
            {
                id = it.Id,
                licensePlateNumber = it.LicensePlateNumber,
                size = it.Size,
                transportType = it.TransportType,
                tone = it.Tone,
                driver = it.Driver,
                driverPhone = it.DriverPhone,
                transportStatus = it.TransportStatus,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogVehicleListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建车辆信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogVehicleCrInput input)
    {
        var entity = input.Adapt<LogVehicleEntity>();
        if (await _repository.Where(it => it.LicensePlateNumber == entity.LicensePlateNumber).AnyAsync())
        {
            throw Oops.Oh("车牌号码已存在！");
        }
        entity.Id = SnowflakeIdHelper.NextId();
        try
        {
            // 开启事务
            _db.BeginTran();
            var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();

            #region 订单附件
            var logVehicleAttachmentList = input.logVehicleAttachmentList.Adapt<List<LogVehicleAttachmentEntity>>();
            if (logVehicleAttachmentList != null)
            {
                foreach (var item in logVehicleAttachmentList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.VId = entity.Id;
                    item.UploadTime = DateTime.Now;
                }
                await _repository.Context.Insertable<LogVehicleAttachmentEntity>(logVehicleAttachmentList).ExecuteCommandAsync();
            }
            #endregion

            if (input.cIdList.IsAny())
            {
                var logClassesList = input.cIdList.Select(cid => new LogVehicleClassesEntity
                {
                    CId = cid,
                    VId = entity.Id,
                    Id = SnowflakeIdHelper.NextId()
                }).ToList();
                await _repository.Context.Insertable<LogVehicleClassesEntity>(logClassesList).ExecuteCommandAsync();
            }

             if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);            
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
    /// 更新车辆信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogVehicleUpInput input)
    {
        var entity = input.Adapt<LogVehicleEntity>();
        if (await _repository.Where(it => it.LicensePlateNumber == entity.LicensePlateNumber && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh("车牌号码已存在！");
        }
        try
        {
            var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

            // 清空入驻商家附件记录原有数据
            await _repository.Context.CUDSaveAsnyc<LogVehicleAttachmentEntity>(it => it.VId == entity.Id, input.logVehicleAttachmentList, it => it.VId = entity.Id);

            await _repository.Context.Deleteable<LogVehicleClassesEntity>().Where(it => it.VId == entity.Id).ExecuteCommandAsync();
            if (input.cIdList.IsAny())
            {
                var logClassesList = input.cIdList.Select(cid => new LogVehicleClassesEntity
                {
                    CId = cid,
                    VId = entity.Id,
                    Id = SnowflakeIdHelper.NextId()
                }).ToList();
                await _repository.Context.Insertable<LogVehicleClassesEntity>(logClassesList).ExecuteCommandAsync();
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
    /// 删除车辆信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogVehicleEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        //删除附件
        await _repository.Context.Deleteable<LogVehicleAttachmentEntity>().Where(it => it.VId == id).ExecuteCommandAsync();

        //删除关联班次
        await _repository.Context.Deleteable<LogVehicleClassesEntity>().Where(it => it.VId == id).ExecuteCommandAsync();
    }


    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector")]
    public async Task<dynamic> Selector()
    {
        var data = await _repository.AsQueryable()
            .Select(it => new { id = it.Id, name = it.LicensePlateNumber, transportStatus=it.TransportStatus })
            .ToListAsync();

        return new { list = data };
    }

    /// <summary>
    /// 获取车辆监控列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Status")]
    public async Task<dynamic> GetStatusList([FromQuery] LogVehicleStatusListQueryInput input)
    {
        List<DateTime> queryOrderDate = input.collectionTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startOrderDate = queryOrderDate?.First();
        DateTime? endOrderDate = queryOrderDate?.Last();
        var data = await _repository.Context.Queryable<LogVehicleStatusEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.vId), it => it.VId == input.vId)
            .WhereIF(queryOrderDate != null, it => SqlFunc.Between(it.CollectionTime, startOrderDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endOrderDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderBy(it => it.VId)
            .OrderByDescending(it => it.CollectionTime)
            .Select(it => new LogVehicleStatusListOutput
            {
                id = it.Id,
                collectionDevice = it.CollectionDevice,
                collectionTime = it.CollectionTime,
                dateSource = it.DataSource,
                latitude = it.Latitude,
                longitude = it.Longitude,
                pointId = it.PointId,
                pointIdName = SqlFunc.Subqueryable<LogDeliveryPointEntity>().Where(xx=>xx.Id == it.PointId).Select(xx=>xx.Name),
                vId = it.VId,
                vIdName = SqlFunc.Subqueryable<LogVehicleEntity>().Where(xx => xx.Id == it.VId).Select(xx => xx.LicensePlateNumber)
            })
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogVehicleStatusListOutput>.SqlSugarPageResult(data);
    }
}
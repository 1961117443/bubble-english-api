using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogOrderClasses;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Logistics.Entitys.Dto.LogOrderDelivery;
using System.ComponentModel.DataAnnotations;
using QT.Systems.Entitys.Permission;
using QT.EventBus;

namespace QT.Logistics;

/// <summary>
/// 业务实现：订单装卸车记录.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "订单装车卸车管理", Name = "LogOrderClasses", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogOrderClassesService : ILogOrderClassesService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogOrderClassesEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ILogDeliveryPointService _logDeliveryPointService;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// 初始化一个<see cref="LogOrderClassesService"/>类型的新实例.
    /// </summary>
    public LogOrderClassesService(
        ISqlSugarRepository<LogOrderClassesEntity> logOrderClassesRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        ILogDeliveryPointService logDeliveryPointService,
        IEventPublisher eventPublisher)
    {
        _repository = logOrderClassesRepository;
        _userManager = userManager;
        _logDeliveryPointService = logDeliveryPointService;
        _eventPublisher = eventPublisher;
    }

    #region 增删改查
    /// <summary>
    /// 获取订单装卸车记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogOrderClassesInfoOutput>();

        if (!string.IsNullOrEmpty(output.orderId))
        {
            output.orderIdOrderNo = await _repository.Context.Queryable<LogOrderEntity>().Where(it => it.Id == output.orderId).Select(it => it.OrderNo).FirstAsync();
        }        

        return output;
    }

    /// <summary>
    /// 获取订单装卸车记录列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogOrderClassesListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogOrderClassesEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.orderIdNo), it => SqlFunc.Subqueryable<LogOrderEntity>().Where(x=>x.Id == it.OrderId && x.OrderNo.Contains(input.orderIdNo)).Any())
            .WhereIF(!string.IsNullOrEmpty(input.cIdNo), it => SqlFunc.Subqueryable<LogClassesVehicleEntity>().Where(x => x.Id == it.CId && x.Code.Contains(input.cIdNo)).Any())
            .WhereIF(!string.IsNullOrEmpty(input.orderId), it => it.OrderId.Contains(input.orderId))
            .WhereIF(!string.IsNullOrEmpty(input.cId), it => it.CId.Equals(input.cId))
            .WhereIF(!string.IsNullOrEmpty(input.inboundPerson), it => it.InboundPerson.Equals(input.inboundPerson))
            .WhereIF(!string.IsNullOrEmpty(input.outboundPerson), it => it.OutboundPerson.Equals(input.outboundPerson))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.OrderId.Contains(input.keyword)
                || it.CId.Contains(input.keyword)
                || it.InboundPerson.Contains(input.keyword)
                || it.OutboundPerson.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogOrderClassesListOutput
            {
                id = it.Id,
                orderId = it.OrderId,
                cId = it.CId,
                inboundTime = it.InboundTime,
                inboundPerson = it.InboundPerson,
                outboundTime = it.OutboundTime,
                outboundPerson = it.OutboundPerson,
                cIdCode = SqlFunc.Subqueryable<LogClassesVehicleEntity>().Where(x=>x.Id == it.CId).Select(x=>x.Code),
                orderIdOrderNo = SqlFunc.Subqueryable<LogOrderEntity>().Where(x=>x.Id == it.OrderId).Select(x=>x.OrderNo),
                inboundPersonName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.InboundPerson).Select(x => x.RealName),
                outboundPersonName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.OutboundPerson).Select(x => x.RealName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogOrderClassesListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建订单装卸车记录.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogOrderClassesCrInput input)
    {
        var entity = input.Adapt<LogOrderClassesEntity>();

        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新订单装卸车记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogOrderClassesUpInput input)
    {
        var entity = input.Adapt<LogOrderClassesEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除订单装卸车记录.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogOrderClassesEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion

    /// <summary>
    /// 新建入库记录.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("Inbound")]
    public async Task CreateInbound([FromBody] LogOrderClassesCrInput input)
    {
        var entity = input.Adapt<LogOrderClassesEntity>();

        //// 同一个订单，同一个车次不应该重复录入
        //if (await _repository.Where(it => it.OrderId == entity.OrderId && it.CId == entity.CId).AnyAsync())
        //{
        //    throw Oops.Oh("订单已装车！");
        //}
        entity.Id = SnowflakeIdHelper.NextId();

        entity.InboundTime = DateTime.Now;
        entity.InboundPerson = _userManager.UserId;

        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);

        // 写入车辆状态
        var point = await _logDeliveryPointService.GetCurrentUserPoint();
        if (point != null)
        {
            var classes = await _repository.Context.Queryable<LogClassesVehicleEntity>().InSingleAsync(input.cId);
            var logVehicleStatusCrInput = new LogVehicleStatusCrInput
            {
                vId = classes.VId,
                collectionDevice = "PC端",
                dataSource = "上货",
                latitude = "",
                longitude = "",
                pointId = point.Id,
            };
            var logVehicleStatusEntity = logVehicleStatusCrInput.Adapt<LogVehicleStatusEntity>();
            logVehicleStatusEntity.Create();
            await _eventPublisher.PublishAsync("Logistics:VehicleStatus:Create", logVehicleStatusEntity);
        }
    }

    /// <summary>
    /// 更新出库记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Outbound/{id}")]
    public async Task UpdateOutbound(string id, [FromBody] LogOrderClassesUpInput input)
    {
        var entity = input.Adapt<LogOrderClassesEntity>();
        entity.OutboundTime = DateTime.Now;
        entity.OutboundPerson = _userManager.UserId;
        var isOk = await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.OutboundPerson, it.OutboundTime }).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);

        // 写入车辆状态
        var point = await _logDeliveryPointService.GetCurrentUserPoint();
        if (point!=null)
        {
            var classes = await _repository.Context.Queryable<LogClassesVehicleEntity>().InSingleAsync(input.cId);
            var logVehicleStatusCrInput = new LogVehicleStatusCrInput
            {
                vId = classes.VId,
                collectionDevice = "PC端",
                dataSource = "下货",
                latitude = "",
                longitude = "",
                pointId = point.Id,
            };
            var logVehicleStatusEntity = logVehicleStatusCrInput.Adapt<LogVehicleStatusEntity>();
            logVehicleStatusEntity.Create();
            await _eventPublisher.PublishAsync("Logistics:VehicleStatus:Create", logVehicleStatusEntity);
        }
    }

    /// <summary>
    /// 根据订单号查询订单
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/QueryOrder")]
    public async Task<LogOrderClassesInfoOutput> QueryOrderByNo([Required, FromQuery] string no)
    {
        var order = await _repository.Context.Queryable<LogOrderEntity>().Where(it => it.OrderNo == no).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);

        if (order.OrderStatus != 1)
        {
            throw Oops.Oh("订单状态异常！");
        }

        LogOrderClassesInfoOutput output = new LogOrderClassesInfoOutput
        {
            orderId = order.Id,
            orderIdOrderNo = order.OrderNo,
        };
        // 判断是否已入库但是未出库的记录
        var entity = await _repository.Where(it => it.OrderId == order.Id && it.InboundTime.HasValue && !it.OutboundTime.HasValue).FirstAsync();

        if (entity != null)
        {
            output = entity.Adapt<LogOrderClassesInfoOutput>();
            output.orderIdOrderNo = order.OrderNo;
        }
        else
        {
        }


        return output;
    }

    #region 装车扫描

    /// <summary>
    /// 获取车次信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Inbound/ScanClasses")]
    public async Task<dynamic> InboundScanClasses([FromQuery, Required] KeywordInput input)
    {
        var barCode = input.keyword;
        if (string.IsNullOrEmpty(barCode))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        var entity = await _repository.Context.Queryable<LogClassesVehicleEntity>()
            .Where(it => it.Id == barCode || it.VId == barCode)
            .Where(it => it.DepartureTime >= DateTime.Now)
            .FirstAsync() ?? throw Oops.Oh("车次不存在！");

        return new { cId = entity.Id };
    }

    /// <summary>
    /// 入库扫码 扫订单
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Inbound/ScanOrder")]
    public async Task<LogOrderDeliveryInboundScanOutput> InboundScanOrder([FromQuery, Required] KeywordInput input)
    {
        var barCode = input.keyword;
        if (string.IsNullOrEmpty(barCode))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        #region 物流订单
        var order = await _repository.Context.Queryable<LogOrderEntity>()
            .Where(it => it.OrderStatus > 0)
            .Where(it => it.Id == barCode || it.OrderNo == barCode)
            .FirstAsync();

        if (order != null)
        {
            return new LogOrderDeliveryInboundScanOutput
            {
                id = order.Id,
                no = order.OrderNo,
                category = InboundScanEnum.order
            };
        }
        #endregion
        #region 配送单
        var note = await _repository.Context.Queryable<LogDeliverynoteEntity>()
           .Where(it => it.Id == barCode || it.OrderNo == barCode)
           .FirstAsync();

        if (note != null)
        {
            return new LogOrderDeliveryInboundScanOutput
            {
                id = note.Id,
                no = note.OrderNo,
                category = InboundScanEnum.note
            };
        }
        #endregion

        throw Oops.Oh(ErrorCode.COM1005);
    }

    /// <summary>
    /// 入库扫码 批量提交
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/Inbound/Scan")]
    public async Task<int> InboundScan([FromBody] LogOrderClassesInboundScanCrInput input)
    {
        // 判断车次是否过期
        var classesVehicleEntity = await _repository.Context.Queryable<LogClassesVehicleEntity>()
            .Where(it => it.DepartureTime >= DateTime.Now)
            .InSingleAsync(input.cId) ?? throw Oops.Oh("车次不存在！");
      
 
        List<LogOrderClassesEntity> insertList = new List<LogOrderClassesEntity>();

        // 把配送清单的数据添加转成订单
        var noteList = input.list.Where(it => it.category == InboundScanEnum.note).Select(it => it.id).ToArray();
        if (noteList.IsAny())
        {
            var noteOrders = await _repository.Context.Queryable<LogDeliverynoteOrderEntity>()
                .Where(it => noteList.Contains(it.NoteId))
                .Select(it => new LogOrderDeliveryInboundScanOutput
                {
                    id = it.OrderId,
                    category = InboundScanEnum.order
                })
                .ToListAsync();

            if (noteOrders.IsAny())
            {
                input.list.AddRange(noteOrders);
            }
        }

        #region 物流订单
        var orderIdList = input.list.Where(it => it.category == InboundScanEnum.order).Select(it => it.id).ToArray();
        if (orderIdList.IsAny())
        {
            // 已经装车的订单
            var inboundOrderList = await _repository
                .Where(it => orderIdList.Contains(it.OrderId) && it.InboundTime.HasValue && !it.OutboundTime.HasValue)
                .Select(it => it.OrderId)
                .ToListAsync();

            string batchNo = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            foreach (var orderId in orderIdList)
            {
                if (inboundOrderList.IsAny() && inboundOrderList.Contains(orderId))
                {
                    continue;
                }

                insertList.Add(new LogOrderClassesEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    InboundPerson = _userManager.UserId,
                    InboundTime = DateTime.Now,
                    OrderId = orderId,
                    CId = classesVehicleEntity.Id,
                    BatchNumber = batchNo
                });
            }
        } 
        #endregion

        if (insertList.IsAny())
        {
            // 写入车辆状态
            var point = await _logDeliveryPointService.GetCurrentUserPoint();
            if (point != null)
            {
                var classes = await _repository.Context.Queryable<LogClassesVehicleEntity>().InSingleAsync(input.cId);
                var logVehicleStatusCrInput = new LogVehicleStatusCrInput
                {
                    vId = classes.VId,
                    collectionDevice = "APP端",
                    dataSource = "上货",
                    latitude = input.latitude,
                    longitude = input.longitude,
                    pointId = point.Id,
                };
                var logVehicleStatusEntity = logVehicleStatusCrInput.Adapt<LogVehicleStatusEntity>();
                logVehicleStatusEntity.Create();

                await _eventPublisher.PublishAsync("Logistics:VehicleStatus:Create", logVehicleStatusEntity);
            }
            return await _repository.InsertAsync(insertList);
        }

        return 0;
    }
    #endregion

    #region 卸车扫描

    /// <summary>
    /// 卸车扫描
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Actions/Outbound/ScanAuto")]
    public async Task<List<LogOrderDeliveryInfoOutput>> OutboundScanAuto([FromQuery, Required] KeywordInput input)
    {
        var barCode = input.keyword;
        if (string.IsNullOrEmpty(barCode))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        InboundScanEnum inboundScanEnum = InboundScanEnum.order;
        if (await _repository.Context.Queryable<LogOrderClassesEntity>().Where(it => it.Id == barCode).AnyAsync())
        {
            inboundScanEnum = InboundScanEnum.classes;
        }
        if (await _repository.Context.Queryable<LogDeliverynoteEntity>().Where(it => it.Id == barCode).AnyAsync())
        {
            inboundScanEnum = InboundScanEnum.note;
        }

        return await OutboundScan(inboundScanEnum, input);
    }

    /// <summary>
    /// 卸车扫描
    /// </summary>
    /// <param name="type">扫描类型 1：车次，1：订单，2：配送清单</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Actions/Outbound/Scan/{type}")]
    public async Task<List<LogOrderDeliveryInfoOutput>> OutboundScan(InboundScanEnum type, [FromQuery, Required] KeywordInput input)
    {
        var barCode = input.keyword;
        if (string.IsNullOrEmpty(barCode))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }
        ISugarQueryable<LogOrderClassesEntity> qur = null;
        switch (type)
        {
            case InboundScanEnum.order:
                qur = _repository.Where(it => it.OrderId == barCode || SqlFunc.Subqueryable<LogOrderEntity>().Where(x => x.Id == it.OrderId && x.OrderNo == barCode).Any());
                break;
            case InboundScanEnum.classes:
                qur = _repository.Where(it => it.CId == barCode);
                break;
            case InboundScanEnum.note:
                qur = _repository.Where(it => SqlFunc.Subqueryable<LogDeliverynoteOrderEntity>().Where(x => x.OrderId == it.OrderId && x.NoteId == barCode).Any());
                break;
            default:
                throw Oops.Oh(errorCode: ErrorCode.COM1005);
        }

        qur = qur.Where(it => it.InboundTime.HasValue && !it.OutboundTime.HasValue);

        var list = await qur.Select(it => new LogOrderDeliveryInfoOutput
        {
            orderIdOrderNo = SqlFunc.Subqueryable<LogOrderEntity>().Where(x => x.Id == it.OrderId).Select(x => x.OrderNo),
            inboundPersonName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.InboundPerson).Select(x => x.RealName)
        }, true).ToListAsync();

        return list;
    }

    /// <summary>
    /// 卸车扫描 批量提交
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/Outbound/Scan")]
    public async Task<int> OutboundScan([FromBody, Required] LogOrderClassesOutboundScanCrInput input)
    {
        var idList = input.list?.Select(it => it.id).ToArray();
        if (!idList.IsAny())
        {
            return 0;
        }

        var isOk = await _repository.Context.Updateable<LogOrderClassesEntity>()
            .SetColumns(it => new LogOrderClassesEntity
            {
                OutboundTime = DateTime.Now,
                OutboundPerson = _userManager.UserId,
            })
            .Where(it => idList.Contains(it.Id) && !it.OutboundTime.HasValue)
            .ExecuteCommandAsync();

        // 写入车辆状态
        var point = await _logDeliveryPointService.GetCurrentUserPoint();
        if (point != null)
        {
            var classesList = await _repository.Context.Queryable<LogClassesVehicleEntity>()
                .Where(it=> SqlFunc.Subqueryable<LogOrderClassesEntity>().Where(xxx=>xxx.CId == it.Id && idList.Contains(xxx.Id)).Any())
                .ToListAsync();
            foreach (var classes in classesList)
            {
                var logVehicleStatusCrInput = new LogVehicleStatusCrInput
                {
                    vId = classes.VId,
                    collectionDevice = "APP端",
                    dataSource = "下货",
                    latitude = input.latitude,
                    longitude = input.longitude,
                    pointId = point.Id,
                };
                var logVehicleStatusEntity = logVehicleStatusCrInput.Adapt<LogVehicleStatusEntity>();
                logVehicleStatusEntity.Create();

                await _eventPublisher.PublishAsync("Logistics:VehicleStatus:Create", logVehicleStatusEntity);
            }
            
        }

        return isOk;
    }
    #endregion
}
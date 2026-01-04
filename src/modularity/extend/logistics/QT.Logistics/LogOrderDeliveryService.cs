using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogOrderDelivery;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using QT.Systems.Entitys.Permission;

namespace QT.Logistics;

/// <summary>
/// 业务实现：出入库记录.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "订单出库入库管理", Name = "LogOrderDelivery", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogOrderDeliveryService : ILogOrderDeliveryService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogOrderDeliveryEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogOrderDeliveryService"/>类型的新实例.
    /// </summary>
    public LogOrderDeliveryService(
        ISqlSugarRepository<LogOrderDeliveryEntity> logOrderDeliveryRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logOrderDeliveryRepository;
        _userManager = userManager;
    }

    #region 增删改查
    /// <summary>
    /// 获取出入库记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogOrderDeliveryInfoOutput>() ?? throw Oops.Oh(ErrorCode.COM1005);

        output.orderIdOrderNo = await _repository.Context.Queryable<LogOrderEntity>().Where(it => it.Id == output.orderId).Select(it => it.OrderNo).FirstAsync();

        return output;
    }

    /// <summary>
    /// 获取出入库记录列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogOrderDeliveryListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogOrderDeliveryEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.orderIdOrderNo), it => SqlFunc.Subqueryable<LogOrderEntity>().Where(x => x.Id == it.OrderId && x.OrderNo.Contains(input.orderIdOrderNo)).Any())
            .WhereIF(!string.IsNullOrEmpty(input.pointId), it => it.PointId.Equals(input.pointId))
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
            //    it.OrderId.Contains(input.keyword)
            //    || it.PointId.Contains(input.keyword)
            //    )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogOrderDeliveryListOutput
            {
                id = it.Id,
                orderId = it.OrderId,
                pointId = it.PointId,
                storeRoomId = it.StoreRoomId,
                inboundTime = it.InboundTime,
                inboundPerson = it.InboundPerson,
                outboundTime = it.OutboundTime,
                outboundPerson = it.OutboundPerson,
                remark = it.Remark,
                storeRoomIdName = SqlFunc.Subqueryable<LogDeliveryStoreroomEntity>().Where(x => x.Id == it.StoreRoomId)
                .Select(x => (x.Category == 0 ? "【仓库】" : x.Category == 1 ? "【库区】" : x.Category == 2 ? "【货柜】" : x.Category == 3 ? "【柜层】" : "") + x.Name),
                orderIdOrderNo = SqlFunc.Subqueryable<LogOrderEntity>().Where(x=>x.Id == it.OrderId).Select(x=>x.OrderNo),
                inboundPersonName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.InboundPerson).Select(x => x.RealName),
                outboundPersonName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.OutboundPerson).Select(x => x.RealName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogOrderDeliveryListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建出入库记录.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogOrderDeliveryCrInput input)
    {
        var entity = input.Adapt<LogOrderDeliveryEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新出入库记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogOrderDeliveryUpInput input)
    {
        var entity = input.Adapt<LogOrderDeliveryEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除出入库记录.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogOrderDeliveryEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion

    /// <summary>
    /// 新建入库记录.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("Inbound")]
    public async Task CreateInbound([FromBody] LogOrderDeliveryCrInput input)
    {
        var entity = input.Adapt<LogOrderDeliveryEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        entity.InboundTime = DateTime.Now;
        entity.InboundPerson = _userManager.UserId;

        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新出库记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Outbound/{id}")]
    public async Task UpdateOutbound(string id, [FromBody] LogOrderDeliveryUpInput input)
    {
        var entity = input.Adapt<LogOrderDeliveryEntity>();
        entity.OutboundTime = DateTime.Now;
        entity.OutboundPerson = _userManager.UserId;
        var isOk = await _repository.Context.Updateable(entity).UpdateColumns(it=>new {it.OutboundPerson,it.OutboundTime,it.Remark}).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 根据订单号查询订单
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/QueryOrder")]
    public async Task<LogOrderDeliveryInfoOutput> QueryOrderByNo([Required, FromQuery] string no)
    {
        var order = await _repository.Context.Queryable<LogOrderEntity>().Where(it => it.OrderNo == no).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);

        if (order.OrderStatus != 1)
        {
            throw Oops.Oh("订单状态异常！");
        }

        LogOrderDeliveryInfoOutput output = new LogOrderDeliveryInfoOutput
        {
            orderId = order.Id,
            orderIdOrderNo = order.OrderNo,
        };
        // 判断是否已入库但是未出库的记录
        var entity = await _repository.Where(it => it.OrderId == order.Id && it.InboundTime.HasValue && !it.OutboundTime.HasValue).FirstAsync();

        if (entity != null)
        {
            output = entity.Adapt<LogOrderDeliveryInfoOutput>();
            output.orderIdOrderNo = order.OrderNo;
            //output.outboundPerson = _userManager.UserId;
        }
        else
        {
            //output.inboundPerson = _userManager.UserId;
        }


        return output;
    }


    #region 入库扫描
    /// <summary>
    /// 入库扫码
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Inbound/ScanCode")]
    public async Task<LogOrderDeliveryInboundScanOutput> InboundScanCode([FromQuery, Required] KeywordInput input)
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
    public async Task<int> InboundScan([FromBody] LogOrderDeliveryInboundScanCrInput input)
    {
        // 根据仓库id获取配送点
        var storeRoom = await _repository.Context.Queryable<LogDeliveryStoreroomEntity>().InSingleAsync(input.storeRoomId) ?? throw Oops.Oh("位置不存在！");
        if (storeRoom.PointId.IsNullOrEmpty())
        {
            throw Oops.Oh("位置不存在！");
        }
        // 判断当前用户是否该配送点的管理员
        if (!await _repository.Context.Queryable<LogDeliveryPointEntity>().AnyAsync(it => it.Id == storeRoom.PointId && it.AdminId == _userManager.UserId))
        {
            throw Oops.Oh("[auth]位置不存在！");
        }
        List<LogOrderDeliveryEntity> insertList = new List<LogOrderDeliveryEntity>();

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
            // 还在仓库的订单
            var inboundOrderList = await _repository
                .Where(it => orderIdList.Contains(it.OrderId) && it.InboundTime.HasValue && !it.OutboundTime.HasValue)
                .Select(it => it.OrderId)
                .ToListAsync();

            foreach (var orderId in orderIdList)
            {
                if (inboundOrderList.IsAny() && inboundOrderList.Contains(orderId))
                {
                    continue;
                }

                insertList.Add(new LogOrderDeliveryEntity
                {
                    Id = SnowflakeIdHelper.NextId(),
                    InboundPerson = _userManager.UserId,
                    InboundTime = DateTime.Now,
                    OrderId = orderId,
                    StoreRoomId = input.storeRoomId,
                    PointId = storeRoom.PointId,
                });
            }
        } 
        #endregion

        if (insertList.IsAny())
        {
            return await _repository.InsertAsync(insertList);
        }

        return 0;
    }
    #endregion

    #region 出库扫描
    /// <summary>
    /// 出库扫描
    /// </summary>
    /// <param name="type">扫描类型 0：位置，1：订单</param>
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
        DeliveryInboundScanEnum deliveryInboundScanEnum = DeliveryInboundScanEnum.order;
        if (await _repository.Context.Queryable<LogDeliveryStoreroomEntity>().Where(it => it.Id == barCode).AnyAsync())
        {
            deliveryInboundScanEnum = DeliveryInboundScanEnum.storeroom;
        }
        if (await _repository.Context.Queryable<LogDeliverynoteEntity>().Where(it => it.Id == barCode).AnyAsync())
        {
            deliveryInboundScanEnum = DeliveryInboundScanEnum.note;
        }

        return await OutboundScan(deliveryInboundScanEnum, input);
    }

    /// <summary>
    /// 出库扫描
    /// </summary>
    /// <param name="type">扫描类型 0：位置，1：订单</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Actions/Outbound/Scan/{type}")]
    public async Task<List<LogOrderDeliveryInfoOutput>> OutboundScan(DeliveryInboundScanEnum type, [FromQuery, Required] KeywordInput input)
    {
        var barCode = input.keyword;
        if (string.IsNullOrEmpty(barCode))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        ISugarQueryable<LogOrderDeliveryEntity> qur = null;
        switch (type)
        {
            case DeliveryInboundScanEnum.order:
                qur = _repository.Where(it => it.OrderId == barCode || SqlFunc.Subqueryable<LogOrderEntity>().Where(x => x.Id == it.OrderId && x.OrderNo == barCode).Any());
                break;
            case DeliveryInboundScanEnum.storeroom:
                qur = _repository.Where(it => it.StoreRoomId == barCode);
                break;
            case DeliveryInboundScanEnum.note:
                qur = _repository.Where(it => SqlFunc.Subqueryable<LogDeliverynoteOrderEntity>().Where(x => x.OrderId == it.OrderId && x.NoteId == barCode).Any());
                break;
            default:
                throw Oops.Oh(errorCode: ErrorCode.COM1005);
        }

        qur = qur.Where(it => it.InboundTime.HasValue && !it.OutboundTime.HasValue);

        var list = await qur.Select(it=>new LogOrderDeliveryInfoOutput
        {
            orderIdOrderNo = SqlFunc.Subqueryable<LogOrderEntity>().Where(x=>x.Id == it.OrderId).Select(x=>x.OrderNo),
            inboundPersonName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.InboundPerson).Select(x => x.RealName)
        },true).ToListAsync();

        return list;
    }

    /// <summary>
    /// 出库扫描 批量提交
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/Outbound/Scan")]
    public async Task<int> OutboundScan([FromBody, Required] List<LogOrderDeliveryInfoOutput> input)
    {
        var idList = input?.Select(it => it.id).ToArray();
        if (!idList.IsAny())
        {
            return 0;
        }

        var isOk = await _repository.Context.Updateable<LogOrderDeliveryEntity>()
            .SetColumns(it => new LogOrderDeliveryEntity
            {
                OutboundTime = DateTime.Now,
                OutboundPerson = _userManager.UserId,
            })
            .Where(it=> idList.Contains(it.Id) && !it.OutboundTime.HasValue)
            .ExecuteCommandAsync();
       
        return isOk;
    }
    #endregion
}
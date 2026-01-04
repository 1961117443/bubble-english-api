using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogOrder;
using QT.Logistics.Entitys.Dto.LogOrderAttachment;
using QT.Logistics.Entitys.Dto.LogOrderDetail;
using QT.Logistics.Entitys.Dto.LogOrderFinancial;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Interfaces.System;
using System.Linq;
using QT.Common.Contracts;
using QT.EventBus;

namespace QT.Logistics;

/// <summary>
/// 业务实现：订单管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "订单管理", Name = "LogOrder", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogOrderService : ILogOrderService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogOrderEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IBillRullService _billRullService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogDeliveryPointService _logDeliveryPointService;

    /// <summary>
    /// 初始化一个<see cref="LogOrderService"/>类型的新实例.
    /// </summary>
    public LogOrderService(
        ISqlSugarRepository<LogOrderEntity> logOrderRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IBillRullService billRullService,IEventPublisher eventPublisher, ILogDeliveryPointService logDeliveryPointService)
    {
        _repository = logOrderRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _billRullService = billRullService;
        _eventPublisher = eventPublisher;
        _logDeliveryPointService = logDeliveryPointService;
        _repository.Context.QueryFilter.AddTableFilter<IDeleteTime>(it => it.DeleteTime == null);
    }

    /// <summary>
    /// 获取订单管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogOrderInfoOutput>();

        //var auxiliayLogOrderAttachment = (await _repository.Context.Queryable<LogOrderAttachmentEntity>().FirstAsync(it => it.OrderId.Equals(output.id))).Adapt<LogOrderAttachmentInfoOutput>();

        //output = ConcurrentDictionaryExtensions.AssignmentObject<LogOrderInfoOutput, LogOrderAttachmentInfoOutput>(output, auxiliayLogOrderAttachment).Adapt<LogOrderInfoOutput>();



        var logOrderAttachmentList = await _repository.Context.Queryable<LogOrderAttachmentEntity>().Where(w => w.OrderId == output.id).ToListAsync();

        output.logOrderAttachmentList = logOrderAttachmentList.Adapt<List<LogOrderAttachmentInfoOutput>>();

        var logOrderDetailList = await _repository.Context.Queryable<LogOrderDetailEntity>().Where(w => w.OrderId == output.id).ToListAsync();

        output.logOrderDetailList = logOrderDetailList.Adapt<List<LogOrderDetailInfoOutput>>();

        var logOrderFinancialList = await _repository.Context.Queryable<LogOrderFinancialEntity>().Where(w => w.OrderId == output.id).ToListAsync();

        output.logOrderFinancialList = logOrderFinancialList.Adapt<List<LogOrderFinancialInfoOutput>>();

        output.logOrderCollectionList = await _repository.Context.Queryable<LogOrderCollectionEntity>().Where(w => w.OrderId == output.id).Select<LogOrderCollectionInfoOutput>().ToListAsync();
        return output;
    }

    /// <summary>
    /// 获取订单管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogOrderListQueryInput input)
    {
        List<DateTime> queryOrderDate = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startOrderDate = queryOrderDate?.First();
        DateTime? endOrderDate = queryOrderDate?.Last();

        string pointId = string.Empty;
        if (input.scope == "point")
        {
            var point = await _logDeliveryPointService.GetCurrentUserPoint();
            pointId = point != null ? point.Id : $"NOTEXISTS_{_userManager.UserId}";
        }
        var data = await _repository.Context
            .Queryable<LogOrderEntity>()
            .Select((it) => new LogOrderListOutput
            {
                id = it.Id,
                orderNo = it.OrderNo,
                sendPointId = it.SendPointId,
                reachPointId = it.ReachPointId,
                orderDate = it.OrderDate,
                orderStatus = SqlFunc.ToString(it.OrderStatus),
                remark = it.Remark,
                shipperName = it.ShipperName,
                shipperPhone = it.ShipperPhone,
                shipperAddress = it.ShipperAddress,
                recipientName = it.RecipientName,
                recipientPhone = it.RecipientPhone,
                recipientAddress = it.RecipientAddress,
            })
            .WhereIF(!string.IsNullOrEmpty(pointId), it => it.sendPointId.Equals(pointId))
            .WhereIF(!string.IsNullOrEmpty(input.orderNo), it => it.orderNo.Contains(input.orderNo))
            .WhereIF(!string.IsNullOrEmpty(input.sendPointId), it => it.sendPointId.Equals(input.sendPointId))
            .WhereIF(!string.IsNullOrEmpty(input.reachPointId), it => it.reachPointId.Equals(input.reachPointId))
            .WhereIF(queryOrderDate != null, it => SqlFunc.Between(it.orderDate, startOrderDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endOrderDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.orderStatus), it => it.orderStatus.Equals(input.orderStatus))
            .WhereIF(!string.IsNullOrEmpty(input.shipperName), it => it.shipperName.Contains(input.shipperName))
            .WhereIF(!string.IsNullOrEmpty(input.shipperPhone), it => it.shipperPhone.Contains(input.shipperPhone))
            .WhereIF(!string.IsNullOrEmpty(input.recipientName), it => it.recipientName.Contains(input.recipientName))
            .WhereIF(!string.IsNullOrEmpty(input.recipientPhone), it => it.recipientPhone.Contains(input.recipientPhone))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.orderNo.Contains(input.keyword)
                || it.shipperName.Contains(input.keyword)
                || it.shipperPhone.Contains(input.keyword)
                || it.recipientName.Contains(input.keyword)
                || it.recipientPhone.Contains(input.keyword))
            .OrderByDescending(it=>it.orderDate)
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.id)
            .OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogOrderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建订单管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogOrderCrInput input)
    {
        var entity = input.Adapt<LogOrderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.OrderNo = await _billRullService.GetBillNumber("LogOrder");
        // 计算金额
        if (input.logOrderDetailList.IsAny())
        {
            entity.Amount = input.logOrderDetailList.Sum(it => it.freight);
        }
        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            #region 订单附件
            var logOrderAttachmentList = input.logOrderAttachmentList.Adapt<List<LogOrderAttachmentEntity>>();
            if (logOrderAttachmentList != null)
            {
                foreach (var item in logOrderAttachmentList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.OrderId = newEntity.Id;
                }
                await _repository.Context.Insertable(logOrderAttachmentList).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
            }
            #endregion

            #region 订单明细
            var logOrderDetailEntityList = input.logOrderDetailList.Adapt<List<LogOrderDetailEntity>>();
            if (logOrderDetailEntityList != null)
            {
                foreach (var item in logOrderDetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.OrderId = newEntity.Id;
                }

                await _repository.Context.Insertable<LogOrderDetailEntity>(logOrderDetailEntityList).ExecuteCommandAsync();
            }
            #endregion

            #region 订单财务
            var logOrderFinancialEntityList = input.logOrderFinancialList.Adapt<List<LogOrderFinancialEntity>>();
            if (logOrderFinancialEntityList != null)
            {
                foreach (var item in logOrderFinancialEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.OrderId = newEntity.Id;
                }

                await _repository.Context.Insertable<LogOrderFinancialEntity>(logOrderFinancialEntityList).ExecuteCommandAsync();
            }
            #endregion

            #region 订单收款
            var logOrderCollectionEntityList = input.logOrderFinancialList.Adapt<List<LogOrderCollectionEntity>>();
            if (logOrderCollectionEntityList != null)
            {
                foreach (var item in logOrderCollectionEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.OrderId = newEntity.Id;
                }

                await _repository.Context.Insertable<LogOrderCollectionEntity>(logOrderCollectionEntityList).ExecuteCommandAsync();
            }
            #endregion

            // 创建会员
            await _eventPublisher.PublishAsync("Logistics:Order:CreateMember", newEntity);
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
    /// 更新订单管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogOrderUpInput input)
    {
        var entity = input.Adapt<LogOrderEntity>();

        // 计算金额
        entity.Amount = input.logOrderDetailList.IsAny() ? input.logOrderDetailList.Sum(it => it.freight) : 0;

        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 订单附件明细
            await _repository.Context.CUDSaveAsnyc<LogOrderAttachmentEntity>( it => it.OrderId == entity.Id, input.logOrderAttachmentList,it=> it.OrderId = entity.Id);

            // 订单物品明细
            await _repository.Context.CUDSaveAsnyc<LogOrderDetailEntity>(it => it.OrderId == entity.Id, input.logOrderDetailList, it => it.OrderId = entity.Id);

            // 订单财务明细
            await _repository.Context.CUDSaveAsnyc<LogOrderFinancialEntity>(it => it.OrderId == entity.Id, input.logOrderFinancialList, it => it.OrderId = entity.Id);

            // 订单收款明细
            await _repository.Context.CUDSaveAsnyc<LogOrderCollectionEntity>(it => it.OrderId == entity.Id, input.logOrderCollectionList, it => it.OrderId = entity.Id);

            // 创建会员
            await _eventPublisher.PublishAsync("Logistics:Order:CreateMember", entity);

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
    /// 删除订单管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        //if(!await _repository.Context.Queryable<LogOrderEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}

        var entity = await _repository.SingleAsync(it => it.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        // 订单物品明细表数据
        var logOrderDetailList = await _repository.Context.Queryable<LogOrderDetailEntity>().Where(it => it.OrderId == entity.Id).ToListAsync();

        // 订单附件表表数据
        var logOrderAttachmentList = await _repository.Context.Queryable<LogOrderAttachmentEntity>().Where(it => it.OrderId == entity.Id).ToListAsync();

        // 订单财务明细表数据
        var logOrderFinancialList = await _repository.Context.Queryable<LogOrderFinancialEntity>().Where(it => it.OrderId == entity.Id).ToListAsync();

        // 订单收款明细表数据
        var logOrderCollectionList = await _repository.Context.Queryable<LogOrderCollectionEntity>().Where(it => it.OrderId == entity.Id).ToListAsync();

        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogOrderEntity>(entity).CallEntityMethod(it => it.Delete())
                .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

            if (logOrderDetailList.IsAny())
            {
                await _repository.Context.Updateable<LogOrderDetailEntity>(logOrderDetailList).CallEntityMethod(it => it.Delete())
                    .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            }

            if (logOrderAttachmentList.IsAny())
            {
                await _repository.Context.Updateable<LogOrderAttachmentEntity>(logOrderDetailList).CallEntityMethod(it => it.Delete())
                    .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            }


            if (logOrderFinancialList.IsAny())
            {
                await _repository.Context.Updateable<LogOrderFinancialEntity>(logOrderDetailList).CallEntityMethod(it => it.Delete())
                    .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            }

            if (logOrderCollectionList.IsAny())
            {
                await _repository.Context.Updateable<LogOrderCollectionEntity>(logOrderCollectionList).CallEntityMethod(it => it.Delete())
                    .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            }


            //var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            //await _repository.Context.Deleteable<LogOrderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            //// 清空订单附件表表数据
            //await _repository.Context.Deleteable<LogOrderAttachmentEntity>().Where(it => it.OrderId.Equals(entity.Id)).ExecuteCommandAsync();

            //// 清空订单物品明细表数据
            //await _repository.Context.Deleteable<LogOrderDetailEntity>().Where(it => it.OrderId.Equals(entity.Id)).ExecuteCommandAsync();

            //// 清空订单财务明细表数据
            //await _repository.Context.Deleteable<LogOrderFinancialEntity>().Where(it => it.OrderId.Equals(entity.Id)).ExecuteCommandAsync();

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
}
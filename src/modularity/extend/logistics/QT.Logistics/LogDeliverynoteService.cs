using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Interfaces.System;
using QT.Logistics.Entitys.Dto.LogDeliverynote;
using QT.Logistics.Entitys.Dto.LogDeliverynoteOrder;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Logistics.Entitys.Dto.LogOrder;

namespace QT.Logistics;

/// <summary>
/// 业务实现：配送单.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "配送单管理", Name = "LogDeliverynote", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogDeliverynoteService : ILogDeliverynoteService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogDeliverynoteEntity> _repository;

    /// <summary>
    /// 单据规则服务.
    /// </summary>
    private readonly IBillRullService _billRullService;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogDeliverynoteService"/>类型的新实例.
    /// </summary>
    public LogDeliverynoteService(
        ISqlSugarRepository<LogDeliverynoteEntity> logDeliverynoteRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logDeliverynoteRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    /// <summary>
    /// 获取配送单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogDeliverynoteInfoOutput>();

        var logDeliverynoteOrderList = await _repository.Context.Queryable<LogDeliverynoteOrderEntity>().Where(w => w.NoteId == output.id).ToListAsync();
        output.logDeliverynoteOrderList = logDeliverynoteOrderList.Adapt<List<LogDeliverynoteOrderInfoOutput>>();

        if (output.logDeliverynoteOrderList.IsAny())
        {
            var orderIdList = output.logDeliverynoteOrderList.Select(it => it.orderId).ToArray();
            var orderList = await _repository.Context.Queryable<LogOrderEntity>()
                                               .Where(it => orderIdList.Contains(it.Id))
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
                                               .ToListAsync();

            output.logDeliverynoteOrderList.ForEach(x =>
            {
                x.logOrder = orderList.Find(it => it.id == x.orderId) ?? new LogOrderListOutput();
            });
        }       

        return output;
    }

    /// <summary>
    /// 获取配送单列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogDeliverynoteListQueryInput input)
    {
        List<DateTime> queryOrderDate = input.orderDate?.Split(',').ToObject<List<DateTime>>();
        DateTime? startOrderDate = queryOrderDate?.First();
        DateTime? endOrderDate = queryOrderDate?.Last();
        var data = await _repository.Context.Queryable<LogDeliverynoteEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.pointId), it => it.PointId.Contains(input.pointId))
            .WhereIF(!string.IsNullOrEmpty(input.orderNo), it => it.OrderNo.Contains(input.orderNo))
            .WhereIF(queryOrderDate != null, it => SqlFunc.Between(it.OrderDate, startOrderDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endOrderDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.OrderNo.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogDeliverynoteListOutput
            {
                id = it.Id,
                orderNo = it.OrderNo,
                orderDate = it.OrderDate,
                pointId = it.PointId
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogDeliverynoteListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建配送单.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogDeliverynoteCrInput input)
    {
        var entity = input.Adapt<LogDeliverynoteEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.OrderNo = await _billRullService.GetBillNumber("LogDeliverynote");

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<LogDeliverynoteEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var logDeliverynoteOrderEntityList = input.logDeliverynoteOrderList.Adapt<List<LogDeliverynoteOrderEntity>>();
            if(logDeliverynoteOrderEntityList != null)
            {
                foreach (var item in logDeliverynoteOrderEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.NoteId = newEntity.Id;
                }

                await _repository.Context.Insertable<LogDeliverynoteOrderEntity>(logDeliverynoteOrderEntityList).ExecuteCommandAsync();
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
    /// 更新配送单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogDeliverynoteUpInput input)
    {
        var entity = input.Adapt<LogDeliverynoteEntity>();
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogDeliverynoteEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空配送单订单明细原有数据
            await _repository.Context.Deleteable<LogDeliverynoteOrderEntity>().Where(it => it.NoteId == entity.Id).ExecuteCommandAsync();

            // 新增配送单订单明细新数据
            var logDeliverynoteOrderEntityList = input.logDeliverynoteOrderList.Adapt<List<LogDeliverynoteOrderEntity>>();
            if(logDeliverynoteOrderEntityList != null)
            {
                foreach (var item in logDeliverynoteOrderEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.NoteId = entity.Id;
                }

                await _repository.Context.Insertable<LogDeliverynoteOrderEntity>(logDeliverynoteOrderEntityList).ExecuteCommandAsync();
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
    /// 删除配送单.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if(!await _repository.Context.Queryable<LogDeliverynoteEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }       

        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<LogDeliverynoteEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

                // 清空配送单订单明细表数据
            await _repository.Context.Deleteable<LogDeliverynoteOrderEntity>().Where(it => it.NoteId.Equals(entity.Id)).ExecuteCommandAsync();

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
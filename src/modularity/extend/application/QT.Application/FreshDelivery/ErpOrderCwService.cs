using QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderCw;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderoperaterecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderremarks;
using QT.Application.Entitys.Enum.FreshDelivery;
using QT.Application.Entitys.FreshDelivery;
using QT.Application.Interfaces.FreshDelivery;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：订单信息.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "销售订单.财务", Name = "ErpOrderCw", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpOrderCwService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpOrderEntity> _repository;

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
    /// 初始化一个<see cref="ErpOrderCwService"/>类型的新实例.
    /// </summary>
    public ErpOrderCwService(
        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = erpOrderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    /// <summary>
    /// 获取订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpOrderCwInfoOutput>();

        var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((it, a) => it.Mid == a.Id)
            .InnerJoin<ErpProductEntity>((it, a, b) => a.Pid == b.Id)
            .Where(it => it.Fid == output.id)
            .Select((it, a, b) => new ErpOrderdetailInfoOutput
            {
                midName = a.Name,
                productName = b.Name,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
            }, true)
            .ToListAsync();
        output.erpOrderdetailList = erpOrderdetailList; // erpOrderdetailList.Adapt<List<ErpOrderdetailInfoOutput>>();

        var erpOrderoperaterecordList = await _repository.Context.Queryable<ErpOrderoperaterecordEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.erpOrderoperaterecordList = erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordInfoOutput>>();

        var erpOrderremarksList = await _repository.Context.Queryable<ErpOrderremarksEntity>().Where(w => w.OrderId == output.id).ToListAsync();
        output.erpOrderremarksList = erpOrderremarksList.Adapt<List<ErpOrderremarksInfoOutput>>();

        output.erpChildOrderList = await _repository.Context.Queryable<ErpOrderEntity>()
           .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Oid == output.id && ddd.Cid == it.Id).Any()) // 过滤掉拆单记录
           .OrderBy(it => it.CreatorTime, OrderByType.Asc)
           .Select(it => new ErpSubOrderListOutput
           {
               id = it.Id,
               no = it.No,
               createUid = it.CreateUid,
               createTime = it.CreateTime,
               creatorTime = it.CreatorTime,
               cid = it.Cid,
               posttime = it.Posttime,
               cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
               createUidName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.CreateUid).Select(d => d.RealName),
               amount = it.Amount,
               state = it.State ?? 0,
               diningType = it.DiningType,
               oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
           }).ToListAsync();

        if (output.erpChildOrderList.IsAny())
        {
            var subIdList = output.erpChildOrderList.Select(x => x.id).ToList();

            var erpSubOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
           .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
           .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
           .Where(w => subIdList.Contains(w.Fid))
           .Select((w, x, b) => new ErpOrderdetailInfoOutput()
           {
               midName = x.Name,
               productName = b.Name,
               rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
           }, true)
           .ToListAsync();

            foreach (var item in output.erpChildOrderList)
            {
                item.erpOrderdetailList = erpSubOrderdetailList.Where(x => x.fid == item.id).ToList() ?? new List<ErpOrderdetailInfoOutput>();
            }
        }


        return output;
    }

    /// <summary>
    /// 获取订单信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpOrderCwListQueryInput input)
    {
        if (input.states == null || !input.states.Any())
        {
            input.states = new List<OrderStateEnum>
            {
                 OrderStateEnum.Delivery,
                 OrderStateEnum.Receiving
            };
        }
        var oid = input.oid?.Split(',').ToList().Last();
        List<DateTime> queryCreateTime = input.createTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreateTime = queryCreateTime?.First();
        DateTime? endCreateTime = queryCreateTime?.Last();
        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            .Where(it => input.states.Contains(it.State.Value))
            .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid.Contains(oid))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.createUid), it => it.CreateUid.Contains(input.createUid))
            .WhereIF(queryCreateTime != null, it => SqlFunc.Between(it.CreateTime, startCreateTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreateTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid.Contains(input.cid))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Oid.Contains(input.keyword)
                || it.No.Contains(input.keyword)
                || it.CreateUid.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                )
            .Where(it => it.State ==  OrderStateEnum.Delivery || SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Cid == it.Id).NotAny()) // 过滤掉拆单记录，但是财务可以代收货
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpOrderCwListOutput
            {
                id = it.Id,
                oid = it.Oid,
                no = it.No,
                createUid = it.CreateUid,
                createTime = it.CreateTime,
                creatorTime = it.CreatorTime,
                cid = it.Cid,
                state = it.State ?? 0,
                posttime = it.Posttime,
                receiveTime = it.ReceiveTime,
                billDate = it.BillDate,
                invoiceDate = it.InvoiceDate,
                receiptsDate = it.ReceiptsDate,
                cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        if (data != null && data.list.IsAny())
        {
            foreach (var item in data.list)
            {
                if (item.posttime.HasValue)
                {
                    item.dayOfWeek = item.posttime.Value.ToString("dddd");
                }
            }
        }
        return PageResult<ErpOrderCwListOutput>.SqlSugarPageResult(data);
    }


    /// <summary>
    /// 代客户收货
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("Received/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Received([FromRoute] string id, [FromServices] IErpOrderService erpOrderService)
    {
        var entity = await _repository.SingleAsync(it => it.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.State !=  OrderStateEnum.Delivery)
        {
            throw Oops.Oh("订单状态异常，请刷新后再试！");
        }


        _repository.Context.Tracking(entity);
        entity.State =  OrderStateEnum.Receiving;
        entity.ReceiveState = "1";
        entity.ReceiveTime = DateTime.Now;
        entity.DeliveryToTime = DateTime.Now;


        await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        // 更新数据后再判断子单
        await erpOrderService.ProcessReceivingSubOrder(entity);

        //await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State, it.ReceiveState, it.ReceiveTime }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 代客户收货 - 批量
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    [HttpPost("Received")]
    [SqlSugarUnitOfWork]
    public async Task<int> Received([FromBody] ErpOrderStateBatchUpInput param, [FromServices] IErpOrderService erpOrderService)
    {
        if (!param.items.Any())
        {
            return 0;
        }
        var list = await _repository.Where(it => param.items.Contains(it.Id) && it.State ==  OrderStateEnum.Delivery)
            //.Select(it => it.Id)
            .ToListAsync();

        if (!list.Any())
        {
            return 0;
        }

        _repository.Context.Tracking(list);

        foreach (var item in list)
        {
            item.DeliveryToTime = param.date ?? DateTime.Now;
            item.State =  OrderStateEnum.Receiving;
            item.ReceiveState = "1";
            item.ReceiveTime = param.date;
        }

        var result = await _repository.Context.Updateable(list).ExecuteCommandAsync();

        foreach (var item in list)
        {
            // 更新数据库后，再检查子单记录
            await erpOrderService.ProcessReceivingSubOrder(item);
        }

        //var idList = list.Select(x => x.Id).ToList();
        //var result = await _repository.Context
        //    .Updateable<ErpOrderEntity>()
        //    .SetColumns(it => it.State == Extend.Entitys.Enums.OrderStateEnum.Receiving)
        //    .SetColumns(it => it.ReceiveState == "1")
        //    .SetColumns(it => it.ReceiveTime == param.date)
        //    .Where(it => idList.Contains(it.Id) && it.State == Extend.Entitys.Enums.OrderStateEnum.Delivery)
        //    .ExecuteCommandAsync();

        return result;
    }

    /// <summary>
    /// 已开对账单 - 批量
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    [HttpPost("BillDate")]
    public async Task<int> DoBillDate([FromBody] ErpOrderStateBatchUpInput param)
    {
        if (!param.items.Any())
        {
            return 0;
        }
        var list = await _repository
            .Where(it => param.items.Contains(it.Id) && !it.BillDate.HasValue)
            .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Cid == it.Id).NotAny()) // 过滤掉拆单记录
            .Select(it => it.Id)
            .ToListAsync();

        if (!list.Any())
        {
            return 0;
        }

        var result = await _repository.Context
            .Updateable<ErpOrderEntity>()
            .SetColumns(it => it.BillDate == param.date)
            .Where(it => list.Contains(it.Id) && !it.BillDate.HasValue)
            .ExecuteCommandAsync();

        return result;
    }

    /// <summary>
    /// 已开发票 - 批量
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    [HttpPost("Invoice")]
    public async Task<int> DoInvoiceDate([FromBody] ErpOrderStateBatchUpInput param)
    {
        if (!param.items.Any())
        {
            return 0;
        }
        var list = await _repository.Where(it => param.items.Contains(it.Id) && !it.InvoiceDate.HasValue)
            .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Cid == it.Id).NotAny()) // 过滤掉拆单记录
            .Select(it => it.Id)
            .ToListAsync();

        if (!list.Any())
        {
            return 0;
        }

        var result = await _repository.Context
            .Updateable<ErpOrderEntity>()
            .SetColumns(it => it.InvoiceDate == param.date)
            .Where(it => list.Contains(it.Id) && !it.InvoiceDate.HasValue)
            .ExecuteCommandAsync();

        return result;
    }

    /// <summary>
    /// 已收款 - 批量
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    [HttpPost("Receipts")]
    public async Task<int> DoReceiptsDate([FromBody] ErpOrderStateBatchUpInput param)
    {
        if (!param.items.Any())
        {
            return 0;
        }
        var list = await _repository.Where(it => it.State ==  OrderStateEnum.Receiving && param.items.Contains(it.Id) && !it.ReceiptsDate.HasValue)
            .Select(it => it.Id)
            .ToListAsync();

        if (!list.Any())
        {
            return 0;
        }

        var result = await _repository.Context
            .Updateable<ErpOrderEntity>()
            .SetColumns(it => it.ReceiptsDate == param.date)
            .SetColumns(it => it.State ==  OrderStateEnum.Paid)
            .Where(it => list.Contains(it.Id) && !it.ReceiptsDate.HasValue)
            .ExecuteCommandAsync();

        return result;
    }
}
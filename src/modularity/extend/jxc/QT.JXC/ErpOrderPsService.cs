using Mapster;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp.OrderFj;
using QT.JXC.Entitys.Dto.Erp.OrderPs;
using QT.JXC.Interfaces;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：订单信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOrderPs", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpOrderPsService : IErpOrderPsService, IDynamicApiController, ITransient
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
    //private readonly IErpOrderService _erpOrderService;

    /// <summary>
    /// 初始化一个<see cref="ErpOrderPsService"/>类型的新实例.
    /// </summary>
    public ErpOrderPsService(
        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = erpOrderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        //_erpOrderService = erpOrderService;
    }

    /// <summary>
    /// 获取订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpOrderPsInfoOutput>();

        var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
            .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
            .Where(w => w.Fid == output.id)
            .Select((w, x, b) => new ErpOrderdetailInfoOutput()
            {
                midName = x.Name,
                productName = b.Name,
                sorterUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == w.SorterUserId).Select(d => d.RealName),
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName),
                remark = w.Remark ?? ""
            }, true)
            .ToListAsync();
        output.erpOrderdetailList = erpOrderdetailList; //.Adapt<List<ErpOrderdetailInfoOutput>>();

        var erpOrderoperaterecordList = await _repository.Context.Queryable<ErpOrderoperaterecordEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.erpOrderoperaterecordList = erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordInfoOutput>>();

        var erpOrderremarksList = await _repository.Context.Queryable<ErpOrderremarksEntity>().Where(w => w.OrderId == output.id).ToListAsync();
        output.erpOrderremarksList = erpOrderremarksList.Adapt<List<ErpOrderremarksInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取订单信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpOrderPsListQueryInput input)
    {
        bool isAdmin = false;
        if (App.HttpContext.Items.TryGetValue(ClaimConst.CLAINM_JT_COMPANY_ACCOUNT, out var companyAccount) && companyAccount!=null && bool.TryParse(companyAccount?.ToString(), out bool result))
        {
            isAdmin = !result;
        }
        // 只过滤当前配送员
        string deliverymanId = string.Empty;
        List<string> deliverymanIds = new List<string>();
        if (!_userManager.IsAdministrator)
        {
            // 判断是否为车队长
            deliverymanIds = await _repository.Context.Queryable<ErpDeliverymanEntity>()
                .WhereIF(input.keyword.IsNotEmptyOrNull(), it => it.Name.Contains(input.keyword))
                .Where(it => it.CarCaptainId == _userManager.UserId).Select(it => it.Id).ToListAsync();
            deliverymanId = await _repository.Context.Queryable<ErpDeliverymanEntity>().Where(it => it.LoginId == _userManager.Account).Select(it => it.Id).FirstAsync() ?? _userManager.UserId;
        }

        List<DateTime> posttimeRange = input.posttimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPosttimeDate = posttimeRange?.First();
        DateTime? endPosttimeDate = posttimeRange?.Last();

        List<string> gidList = new List<string>();
        if (input.productName.IsNotEmptyOrNull())
        {
            gidList = await _repository.Context.Queryable<ErpProductmodelEntity>()
                .Where(it => SqlFunc.Subqueryable<ErpProductEntity>().Where(d => d.Id == it.Pid && d.Name.Contains(input.productName)).Any())
                .Select(it => it.Id)
                .Take(100)
                .ToListAsync();

            gidList.Add(input.productName);
        }

        List<string> cidList = new List<string>();
        if (input.tid.IsNotEmptyOrNull())
        {
            cidList = await _repository.Context.Queryable<ErpCustomerEntity>().Where(it => it.Type == input.tid).Select(it => it.Id)
                .Take(100).ToListAsync();

            cidList.Add(input.tid);
        }

        List<string> typeIds = new List<string>();
        if (input.rootTypeId.IsNotEmptyOrNull())
        {
            typeIds = await _repository.Context.Queryable<ViewErpProducttypeEx>().Where(x => x.RootId == input.rootTypeId).Select(x => x.Id).ToListAsync();
        }


        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            .WhereIF(!isAdmin,it=>it.Oid == _userManager.CompanyId)
            .Where(it => it.State == Entitys.Enums.OrderStateEnum.Delivery || it.State == Entitys.Enums.OrderStateEnum.Outbound)
            .WhereIF(input.beginDate.HasValue, it => it.Posttime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.Posttime <= input.endDate)
            .WhereIF(input.createTime.HasValue, it => it.CreateTime >= input.createTime && it.CreateTime < input.createTime.Value.AddDays(1))
            .WhereIF(input.posttime.HasValue, it => it.Posttime >= input.posttime && it.Posttime < input.posttime.Value.AddDays(1))
            .WhereIF(input.state.HasValue, it=> it.State == input.state)
            .WhereIF(deliverymanId.IsNotEmptyOrNull(),it=>it.DeliveryManId == deliverymanId /*|| deliverymanIds.Contains(it.DeliveryManId)*/)
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid.Contains(input.cid))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                || SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid && d.Name.Contains(input.keyword)).Any()
                || deliverymanIds.Contains(it.DeliveryManId)
                )
            .WhereIF(input.productName.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d1 => d1.Fid == it.Id
                    && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2 => d2.Id == d1.Mid
                    && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3 => d3.Id == d2.Pid && d3.Name.Contains(input.productName)).Any()).Any()).Any())
            .WhereIF(typeIds.IsAny(), it => SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d1 => d1.Fid == it.Id
                    && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2 => d2.Id == d1.Mid
                    && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3 => d3.Id == d2.Pid && typeIds.Contains(d3.Tid)).Any()).Any()).Any())
            .WhereIF(cidList.IsAny(), it => cidList.Contains(it.Cid))
            .WhereIF(posttimeRange != null, it => SqlFunc.Between(it.Posttime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpOrderPsListOutput
            {
                id = it.Id,
                oid = it.Oid,
                no = it.No,
                createUid = it.CreateUid,
                createTime = it.CreateTime,
                cid = it.Cid,
                posttime = it.Posttime,
                deliveryCar = it.DeliveryCar,
                state = it.State ?? 0,
                cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
                deliveryManIdName = SqlFunc.Subqueryable<ErpDeliverymanEntity>().Where(d => d.Id == it.DeliveryManId).Select(d => d.Name),
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
        return PageResult<ErpOrderPsListOutput>.SqlSugarPageResult(data);
    }



    /// <summary>
    /// 配送确认.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("toDelivery/{id}")]
    public async Task toDelivery(string id, [FromBody] ErpOrderPsAuditInput input, [FromServices]IErpOrderService _erpOrderService)
    {
        var entity = input.Adapt<ErpOrderEntity>();
        if (!input.files.Any())
        {
            throw Oops.Oh("缺少配送凭证！");
        }
        entity.DeliveryProof = input.files.ToJsonString();

       
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<ErpOrderEntity>(entity).UpdateColumns(x=>x.DeliveryProof).ExecuteCommandAsync();
            // 更新未送达的明细的复核数量默认为配送数量（分拣数量）
            var items = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => it.Fid == entity.Id && it.ReceiveState==0).ToListAsync();
            if (items.IsAny())
            {
                items.ForEach(x => x.Num2 = x.Num1);
                await _repository.Context.Updateable<ErpOrderdetailEntity>(items).UpdateColumns(x => x.Num2).ExecuteCommandAsync();
            }

            // 更新订单状态
            await _erpOrderService.ProcessOrder(entity.Id, Entitys.Enums.OrderStateEnum.Delivery);
            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception ex)
        {
            // 回滚事务
            _db.RollbackTran();

            if (ex is AppFriendlyException exception)
            {
                throw Oops.Oh(exception.Message);
            }
            throw Oops.Oh(ErrorCode.COM1001);
        }
    }

    /// <summary>
    /// 配送确认只更新凭证.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("toDelivery/{id}/Proof")]
    public async Task toDeliveryProof(string id, [FromBody] ErpOrderPsAuditInput input)
    {
        var entity = input.Adapt<ErpOrderEntity>();
        if (!input.files.Any())
        {
            throw Oops.Oh("缺少配送凭证！");
        }
        entity.DeliveryProof = input.files.ToJsonString();
        await _repository.Context.Updateable<ErpOrderEntity>(entity).UpdateColumns(x => x.DeliveryProof).ExecuteCommandAsync();
    }

    /// <summary>
    /// 送达确认.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("toReceiving/{id}")]
    public async Task toReceiving(string id, [FromBody] ErpOrderPsAuditInput input, [FromServices] IErpOrderService _erpOrderService)
    {
        var entity = input.Adapt<ErpOrderEntity>();
        if (!input.files.Any())
        {
            throw Oops.Oh("缺少送达凭证！");
        }
        entity.DeliveryToProof = input.files.ToJsonString();
        entity.ReceiveTime = DateTime.Now;
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<ErpOrderEntity>(entity).UpdateColumns(x => new { x.DeliveryToProof ,x.ReceiveTime}).ExecuteCommandAsync();

            //更新所有明细的送达状态
            var dbItems = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => it.Fid == entity.Id && it.ReceiveState == 0).ToListAsync();
            if (dbItems.IsAny())
            {
                _repository.Context.Tracking(dbItems);
                dbItems.ForEach(it =>
                {
                    it.ReceiveState = 1;
                    it.ReceiveTime = DateTime.Now;
                    var item = input.erpOrderdetailList?.Find(x => x.id == it.Id);
                    if (item!=null)
                    {
                        it.Num2 = item.num2;
                        it.Amount2 = item.num2 * it.SalePrice;
                        it.CheckTime = DateTime.Now;
                        it.CheckUser = _userManager.UserId;
                    }
                });

                await _repository.Context.Updateable<ErpOrderdetailEntity>(dbItems)
                    //.UpdateColumns(it => new { it.ReceiveState, it.ReceiveTime, it.Num2, it.Amount2 })
                    .ExecuteCommandAsync();
            }
            //await _repository.Context.Updateable<ErpOrderdetailEntity>()
            //    .SetColumns(x => new ErpOrderdetailEntity
            //    {
            //        ReceiveState = 1,
            //        ReceiveTime = DateTime.Now
            //    })
            //    .Where(x => x.Fid == entity.Id && x.ReceiveState == 0).ExecuteCommandAsync();

            // 更新订单状态
            await _erpOrderService.ProcessOrder(entity.Id, Entitys.Enums.OrderStateEnum.Receiving);
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
    /// 送达确认，只更新凭证.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("toReceiving/{id}/Proof")]
    public async Task toReceivingProof(string id, [FromBody] ErpOrderPsAuditInput input)
    {
        var entity = input.Adapt<ErpOrderEntity>();
        if (!input.files.Any())
        {
            throw Oops.Oh("缺少送达凭证！");
        }
        entity.DeliveryToProof = input.files.ToJsonString();
        await _repository.Context.Updateable<ErpOrderEntity>(entity).UpdateColumns(x => x.DeliveryToProof).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新送达确认明细
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("toReceivingDetail/{id}")]
    public async Task toReceivingDetail(string id, [FromBody] ErpOrderPsAuditDetailInput input)
    {
        var entity = await _repository.Context.Queryable<ErpOrderdetailEntity>().InSingleAsync(input.id); // input.Adapt<ErpOrderdetailEntity>();

        _repository.Context.Tracking(entity);
        entity.ReceiveState = input.receiveState;
        entity.ReceiveTime = entity.ReceiveState == 1 ? DateTime.Now : null;

        //如果是送达状态，更新复核数量和复核时间、复核金额
        if (entity.ReceiveState ==1)
        {
            if (input.num2 <= 0)
            {
                throw Oops.Oh("请输入复核数量！");
            }
            entity.Num2 = input.num2;
            entity.Amount2 = entity.SalePrice * input.num2;
            entity.CheckUser = _userManager.UserId;
            entity.CheckTime = DateTime.Now;
        }
        await _repository.Context.Updateable<ErpOrderdetailEntity>(entity).ExecuteCommandAsync();
        //await _repository.Context.Updateable<ErpOrderdetailEntity>(entity).UpdateColumns(x => new { x.ReceiveState ,x.ReceiveTime}).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取配送员关联的客户订单信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/CustomerOrderList")]
    public async Task<dynamic> GetList([FromQuery] ErpOrderFjListQueryInput input)
    {
        // 获取当前用户关联的配送员
        var deliveryManId = await _repository.Context.Queryable<ErpDeliverymanEntity>().Where(it => it.LoginId == _userManager.Account).Select(it => it.Id).FirstAsync() ?? "******";
        var qur = _repository.Context.Queryable<ErpOrderEntity>()
             .Where(it => it.State >= Entitys.Enums.OrderStateEnum.PendingApproval)
             .Where(it=> SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd=>ddd.Id == it.Cid && ddd.DeliveryManId == deliveryManId).Any())
             .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
             .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
             .WhereIF(input.createTime.HasValue, it => it.CreateTime >= input.createTime && it.CreateTime < input.createTime.Value.AddDays(1))
             .WhereIF(input.posttime.HasValue, it => it.Posttime >= input.posttime && it.Posttime < input.posttime.Value.AddDays(1))
             .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
             .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid.Contains(input.cid))
             .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                 it.No.Contains(input.keyword)
                 || it.Cid.Contains(input.keyword)
                 || SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid && d.Name.Contains(input.keyword)).Any()
                 )
             .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
             .Select(it => new ErpOrderFjListOutput
             {
                 id = it.Id,
                 no = it.No,
                 createUid = it.CreateUid,
                 createTime = it.CreateTime,
                 cid = it.Cid,
                 posttime = it.Posttime,
                 creatorTime = it.CreatorTime,
                 state = it.State ?? 0,
                 cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
                 createUidName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.CreateUid).Select(d => d.RealName)
             }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort);
        var data = await qur.ToPagedListAsync(input.currentPage, input.pageSize);

        await Wrapper(data?.list);

        return PageResult<ErpOrderFjListOutput>.SqlSugarPageResult(data);
    }

    #region Private Method
    /// <summary>
    /// 添加额外的数据
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    private async Task Wrapper(IEnumerable<ErpOrderFjListOutput>? list)
    {
        if (list.IsAny())
        {
            List<string> idList = new List<string>();
            foreach (var item in list)
            {
                if (item.posttime.HasValue)
                {
                    item.dayOfWeek = item.posttime.Value.ToString("dddd");
                }
                idList.Add(item.id);
            }

            /* 2024.4.10 注释
            var details = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => idList.Contains(x.Fid))
                .Where(x => SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(dd => dd.Id == x.Mid).Any())
                .GroupBy(x => x.Fid)
                .Select(x => new
                {
                    id = x.Fid,
                    count = SqlFunc.AggregateCount(x.Mid),
                    done = SqlFunc.AggregateSum(x.SorterState == "1" ? 1 : 0)
                }).ToListAsync();
            */
            var details = await _repository.Context.Queryable<ErpOrderdetailEntity>()
                .InnerJoin<ErpProductmodelEntity>((x, a) => x.Mid == a.Id)
                .Where(x => idList.Contains(x.Fid))
                //.Where(x => SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(dd => dd.Id == x.Mid).Any())
                .Select((x, a) => new
                {
                    id = x.Id,
                    fid = x.Fid,
                    done = x.SorterState == "1" ? 1 : 0,
                    unit = a.Unit,
                    customerUnit = a.Unit,
                    num = x.Num,
                }).ToListAsync();

            // 子单汇总
            var xidList = details.Select(x => x.id).ToList();
            var childrens = await _repository.Context.Queryable<ErpOrderdetailEntity>()
                .InnerJoin<ErpOrderRelationEntity>((a, b) => a.Id == b.Cid)
                .Where((a, b) => xidList.Contains(b.Oid) && b.Type == nameof(ErpOrderdetailEntity))
                .GroupBy((a, b) => b.Oid)
                .Select((a, b) => new
                {
                    id = b.Oid,
                    cnum1 = SqlFunc.AggregateSum(a.Num1),
                    cfjNum = SqlFunc.AggregateSum(a.FjNum ?? 0)
                })
                .ToListAsync();

            var leftJoinQuery = from d in details
                                join c in childrens on d.id equals c.id into orders
                                from co in orders.DefaultIfEmpty()
                                select new { detail = d, ynum = (string.IsNullOrEmpty(d.customerUnit) || d.customerUnit == d.unit ? co?.cnum1 : co?.cfjNum) ?? 0 };

            var items = leftJoinQuery.GroupBy(x => x.detail.fid)
                .Select(x => new
                {
                    id = x.Key,
                    count = x.Count(),
                    done = x.Sum(w => w.detail.done == 1 || w.ynum >= w.detail.num ? 1 : 0)
                })
                .ToList();


            foreach (var item in items)
            {
                var xitem = list.FirstOrDefault(x => x.id == item.id);
                if (xitem != null)
                {
                    xitem.detailCount = item.count;

                    xitem.progress = item.count > 0 ? Math.Round(item.done * 100.0 / item.count, 2) : 0;
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// 批量退回
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPost("actions/BatchBackToPicked")]
    [SqlSugarUnitOfWork]
    public async Task<int> BatchBackToPicked([FromBody] List<string> ids, [FromServices] IErpOrderService erpOrderService)
    {
        int i = 0; 


        var orders = await _repository.AsQueryable().Where(it=> ids.Contains(it.Id)).ToListAsync();

        foreach (var order in orders)
        {
            if (order.State != OrderStateEnum.Outbound && order.State != OrderStateEnum.Delivery)
            {
                continue;
            }

            _repository.Context.Tracking(order);
            order.State = OrderStateEnum.Picked;
            var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)order.State).ToString(),
                Fid = order.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"[{_userManager.RealName}]将订单状态改为[{(int)order.State}-{order.State.ToDescription()}]，订单退回"
            };

            // 更新状态
            await _repository.Context.Updateable<ErpOrderEntity>(order).UpdateColumns(nameof(ErpOrderEntity.State)).ExecuteCommandAsync();

            //写入订单日志
            await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntity).ExecuteCommandAsync();

            i++;
        }

        return i;
    }
}
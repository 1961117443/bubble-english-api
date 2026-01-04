using Microsoft.AspNetCore.Http;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderoperaterecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderremarks;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderXs;
using QT.Application.Entitys.Enum.FreshDelivery;
using QT.Application.Entitys.FreshDelivery;
using QT.Application.Interfaces.FreshDelivery;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.Erp;

/// <summary>
/// 业务实现：订单信息.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "销售订单.销售员", Name = "ErpOrderXs", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpOrderXsService : IDynamicApiController, ITransient
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
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 初始化一个<see cref="ErpOrderXsService"/>类型的新实例.
    /// </summary>
    public ErpOrderXsService(
        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IDictionaryDataService dictionaryDataService)
    {
        _repository = erpOrderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
    }

    /// <summary>
    /// 获取订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpOrderXsInfoOutput>();

        var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
            .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
            .Where(w => w.Fid == output.id)
            .Select((w, x, b) => new ErpOrderdetailXsInfoOutput()
            {
                midName = x.Name,
                productName = b.Name,
                minNum = x.MinNum,
                maxNum = x.MaxNum,
                midUnit = x.Unit,
                ratio = x.Ratio,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
            }, true)
            .ToListAsync();
        output.erpOrderdetailList = erpOrderdetailList.OrderBy(x => x.order ?? 99).ToList(); // erpOrderdetailList.Adapt<List<ErpOrderdetailInfoOutput>>();

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
    public async Task<dynamic> GetList([FromQuery] ErpOrderXsListQueryInput input)
    {
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
        if (input.customerType.IsNotEmptyOrNull())
        {
            cidList = await _repository.Context.Queryable<ErpCustomerEntity>()
                .Where(x => x.Type == input.customerType)
                .Select(it => it.Id)
                .Take(100)
                .ToListAsync();

            cidList.Add(input.customerType);
        }

        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
            .WhereIF(input.state.IsNotEmptyOrNull(), it => it.State == input.state.Adapt<OrderStateEnum>())
            .WhereIF(input.createTime.HasValue, it => it.CreateTime >= input.createTime && it.CreateTime < input.createTime.Value.AddDays(1))
            .WhereIF(input.posttime.HasValue, it => it.Posttime >= input.posttime && it.Posttime < input.posttime.Value.AddDays(1))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid == input.cid)
            .WhereIF(!string.IsNullOrEmpty(input.diningType), it => it.DiningType == input.diningType)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                )
            .WhereIF(posttimeRange != null, it => SqlFunc.Between(it.Posttime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Cid == it.Id).NotAny()) // 过滤掉拆单记录
            .WhereIF(gidList.IsAny(), it => SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d => d.Fid == it.Id && gidList.Contains(d.Mid) && d.DeleteTime == null).Any())
            .WhereIF(cidList.IsAny(), it => cidList.Contains(it.Cid))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.CreatorTime, OrderByType.Desc)
            .Select(it => new ErpOrderXsListOutput
            {
                id = it.Id,
                no = it.No,
                cid = it.Cid,
                posttime = it.Posttime,
                creatorTime = it.CreatorTime,
                state = it.State ?? 0,
                createTime = it.CreateTime,
                cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
                diningType = it.DiningType,
                deliveryManIdName = SqlFunc.Subqueryable<ErpDeliverymanEntity>().Where(d => d.Id == it.DeliveryManId).Select(d => d.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        if (data != null && data.list.IsAny())
        {
            foreach (var item in data.list)
            {
                if (item.posttime.HasValue)
                {
                    item.dayOfWeek = item.posttime.Value.ToString("dddd");
                }
                if (item.createTime.HasValue)
                {
                    item.createDayOfWeek = item.createTime.Value.ToString("dddd");
                }
            }
        }
        return PageResult<ErpOrderXsListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建订单信息.
    /// 返回订单号
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task<string> Create([FromBody] ErpOrderXsCrInput input, [FromServices] IErpConfigService erpConfigService)
    {
        await erpConfigService.ValidCustomerOrder();

        var entity = input.Adapt<ErpOrderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpOrder");
        entity.State = OrderStateEnum.Draft;
        entity.Amount = input.erpOrderdetailList?.Sum(a => a.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        var newEntity = await _repository.Context.Insertable<ErpOrderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

        var erpOrderdetailEntityList = input.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();
        if (erpOrderdetailEntityList != null)
        {
            foreach (var item in erpOrderdetailEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.Fid = newEntity.Id;
            }

            await _repository.Context.Insertable<ErpOrderdetailEntity>(erpOrderdetailEntityList).ExecuteCommandAsync();
        }

        //var erpOrderoperaterecordEntityList = input.erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordEntity>>();
        //if(erpOrderoperaterecordEntityList != null)
        //{
        //    foreach (var item in erpOrderoperaterecordEntityList)
        //    {
        //        item.Id = SnowflakeIdHelper.NextId();
        //        item.Fid = newEntity.Id;
        //    }

        //    await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntityList).ExecuteCommandAsync();
        //}

        var erpOrderremarksEntityList = input.erpOrderremarksList.Adapt<List<ErpOrderremarksEntity>>();
        if (erpOrderremarksEntityList != null)
        {
            foreach (var item in erpOrderremarksEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.OrderId = newEntity.Id;
            }

            await _repository.Context.Insertable<ErpOrderremarksEntity>(erpOrderremarksEntityList).ExecuteCommandAsync();
        }

        //    // 关闭事务
        //    _db.CommitTran();

        return entity.Id;
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1000);
        //}
    }

    /// <summary>
    /// 更新订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpOrderXsUpInput input)
    {
        var oldEntity = await _repository.Context.Queryable<ErpOrderEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var entity = input.Adapt<ErpOrderEntity>();
        input.erpOrderdetailList?.ForEach(x =>
        {
            if (x.rejectNum > 0)
            {
                x.amount = (x.num1 - x.rejectNum) * x.salePrice;
            }
        });
        entity.Amount = input.erpOrderdetailList?.Sum(a => a.amount) ?? 0;
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        await _repository.Context.Updateable<ErpOrderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).EnableDiffLogEvent().ExecuteCommandAsync();

        entity = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(entity.Id);
        List<string> addItems = new List<string>();
        List<string> updateItems = new List<string>();
        List<string> delItems = new List<string>();
        await _repository.Context.CUDSaveAsnyc<ErpOrderdetailEntity>(it => it.Fid == entity.Id, input.erpOrderdetailList,
            it =>
            {
                it.Fid = entity.Id;
                addItems.Add(it.Mid);
            }, it => updateItems.Add(it.Mid), it => delItems.Add(it.Mid));

        List<ErpOrderoperaterecordEntity> erpOrderoperaterecordEntities = new List<ErpOrderoperaterecordEntity>();
        if (addItems.IsAny())
        {
            var addlist = await _repository.Context.Queryable<ErpProductEntity>().InnerJoin<ErpProductmodelEntity>((a, b) => a.Id == b.Pid)
                .Where(((a, b) => addItems.Contains(b.Id))).Select(a => a.Name).ToListAsync();

            erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)entity.State).ToString(),
                Fid = entity.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"订单添加商品明细：{string.Join(",", addlist.Distinct().ToArray())}"

            });
        }

        if (updateItems.IsAny())
        {
            var addlist = await _repository.Context.Queryable<ErpProductEntity>().InnerJoin<ErpProductmodelEntity>((a, b) => a.Id == b.Pid)
                .Where(((a, b) => updateItems.Contains(b.Id))).Select(a => a.Name).ToListAsync();

            erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)entity.State).ToString(),
                Fid = entity.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"订单修改商品明细：{string.Join(",", addlist.Distinct().ToArray())}"

            });
        }


        if (delItems.IsAny())
        {
            var addlist = await _repository.Context.Queryable<ErpProductEntity>().InnerJoin<ErpProductmodelEntity>((a, b) => a.Id == b.Pid)
                .Where(((a, b) => delItems.Contains(b.Id))).Select(a => a.Name).ToListAsync();

            erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)entity.State).ToString(),
                Fid = entity.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"订单删除商品明细：{string.Join(",", addlist.Distinct().ToArray())}"

            });
        }

        // 判断主表变更记录
        if (oldEntity.Posttime != entity.Posttime)
        {
            erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)entity.State).ToString(),
                Fid = entity.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"约定配送时间由[{oldEntity.Posttime?.ToString("yyyy-MM-dd")}]变更为[{entity.Posttime?.ToString("yyyy-MM-dd")}]"
            });
        }
        if (oldEntity.DiningType != entity.DiningType)
        {
            //餐别字典
            var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");
            erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)entity.State).ToString(),
                Fid = entity.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"餐别由[{diningTypeOptions?.Find(x => x.EnCode == oldEntity.DiningType)?.FullName}]变更为[{diningTypeOptions?.Find(x => x.EnCode == entity.DiningType)?.FullName}]"
            });
        }
        if (oldEntity.Remark != entity.Remark)
        {
            erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)entity.State).ToString(),
                Fid = entity.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"备注由[{oldEntity.Remark}]变更为[{entity.Remark}]"
            });
        }
        if (oldEntity.Cid != entity.Cid)
        {
            var cidList = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == oldEntity.Cid || x.Id == entity.Cid)
                 .Select(x => new ErpCustomerEntity
                 {
                     Id = x.Id,
                     Name = x.Name
                 })
                 .ToListAsync();
            erpOrderoperaterecordEntities.Add(new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)entity.State).ToString(),
                Fid = entity.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"客户由[{cidList.Find(x => x.Id == oldEntity.Cid)?.Name}]变更为[{cidList.Find(x => x.Id == entity.Cid)?.Name}]"
            });
        }

        if (erpOrderoperaterecordEntities.IsAny())
        {
            await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntities).ExecuteCommandAsync();
        }

        //// 清空订单商品表原有数据
        //await _repository.Context.Deleteable<ErpOrderdetailEntity>().Where(it => it.Fid == entity.Id).EnableDiffLogEvent().ExecuteCommandAsync();

        //// 新增订单商品表新数据
        //var erpOrderdetailEntityList = input.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();
        //if(erpOrderdetailEntityList != null)
        //{
        //    foreach (var item in erpOrderdetailEntityList)
        //    {
        //        item.Id ??= SnowflakeIdHelper.NextId();
        //        item.Fid = entity.Id;
        //    }

        //    await _repository.Context.Insertable<ErpOrderdetailEntity>(erpOrderdetailEntityList).EnableDiffLogEvent().ExecuteCommandAsync();
        //}
        //// 新增订单备注记录新数据 调用ErpOrderService.UpdateMemo直接添加
        ///
        // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();
        //    throw Oops.Oh(ErrorCode.COM1001);
        //}
    }

    /// <summary>
    /// 删除订单信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        // 软删除
        var entity = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        // 分拣状态，判断是否有已分拣的数据，不允许删除
        if (entity.State == OrderStateEnum.PendingApproval)
        {
            if (await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == id && x.SorterTime.HasValue).AnyAsync())
            {
                throw Oops.Oh("订单已分拣不允许删除！");
            }
        }
        else if (entity.State != OrderStateEnum.Draft)
        {
            throw Oops.Oh("当前订单状态不允许删除！");
        }


        _repository.Context.Tracking(entity);
        entity.DeleteTime = DateTime.Now;
        entity.DeleteUserId = _userManager.UserId;
        await _repository.Context.Updateable(entity).EnableDiffLogEvent().ExecuteCommandAsync();

        // 判断是否有子单，删除子单
        var subList = await _repository.Context.Queryable<ErpOrderRelationEntity>().Where(x => x.Oid == entity.Id).ToListAsync();

        if (subList.IsAny())
        {
            var subOrders = await _repository.Context.Queryable<ErpOrderEntity>().Where(it => subList.Any(x => x.Cid == it.Id)).ToListAsync();

            if (subOrders.IsAny())
            {
                // 如果子单已分拣不允许删除
                if (await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => subOrders.Any(x => x.Id == it.Fid) && it.SorterState == "1").AnyAsync())
                {
                    throw Oops.Oh("子单已分拣，请退回主单后再删除！");
                }

                foreach (var item in subOrders)
                {
                    _repository.Context.Tracking(item);
                    item.DeleteTime = DateTime.Now;
                    item.DeleteUserId = _userManager.UserId;
                }
                await _repository.Context.Updateable(subOrders).EnableDiffLogEvent().ExecuteCommandAsync();
            }
        }
        //if (!await _repository.Context.Queryable<ErpOrderEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}       

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        //    await _repository.Context.Deleteable<ErpOrderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        //        // 清空订单商品表表数据
        //    await _repository.Context.Deleteable<ErpOrderdetailEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

        //        // 清空订单处理记录表表数据
        //    await _repository.Context.Deleteable<ErpOrderoperaterecordEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

        //        // 清空订单备注记录表数据
        //    await _repository.Context.Deleteable<ErpOrderremarksEntity>().Where(it => it.OrderId.Equals(entity.Id)).ExecuteCommandAsync();

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1002);
        //}
    }

    /// <summary>
    /// 销售代客下单导入
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/ImportData")]
    public Task ImportOrder(IFormFile file)
    {
        throw new NotImplementedException();
    }
}
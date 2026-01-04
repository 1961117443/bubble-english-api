//using QT.Common.Core.Manager;
//using QT.Common.Core.Security;
//using QT.Common.Enum;
//using QT.Common.Extension;
//using QT.Common.Filter;
//using QT.Common.Security;
//using QT.DependencyInjection;
//using QT.DynamicApiController;
//using QT.FriendlyException;
//using QT.Systems.Interfaces.System;
////using QT.Erp.Entitys.Dto.ErpOrder;
//using QT.Erp.Entitys.Dto.ErpOrderKh;
//using QT.Erp.Entitys.Dto.ErpOrderdetail;
//using QT.Erp.Entitys.Dto.ErpOrderoperaterecord;
//using QT.Erp.Entitys.Dto.ErpOrderremarks;
//using QT.Erp.Entitys;
//using QT.Erp.Interfaces;
//using Mapster;
//using Microsoft.AspNetCore.Mvc;
//using SqlSugar;
//using QT.Extend.Entitys.Enums;
//using QT.Extend.Entitys.Views;
//using QT.Erp.Entitys.Dto.ErpOrder;
//using QT.Systems.Entitys.Permission;

//namespace QT.Application.FreshDelivery;
///// <summary>
///// 业务实现：订单信息.
///// </summary>
//[ApiDescriptionSettings("生鲜配送", Tag = "Erp", Name = "ErpOrderKh", Order = 200)]
//[Route("api/apply/FreshDelivery/[controller]")]
//[Obsolete]
//public class ErpOrderKhService : IErpOrderKhService, IDynamicApiController, ITransient
//{
//    /// <summary>
//    /// 服务基础仓储.
//    /// </summary>
//    private readonly ISqlSugarRepository<ErpOrderEntity> _repository;

//    /// <summary>
//    /// 单据规则服务.
//    /// </summary>
//    private readonly IBillRullService _billRullService;

//    /// <summary>
//    /// 多租户事务.
//    /// </summary>
//    private readonly ITenant _db;

//    /// <summary>
//    /// 用户管理.
//    /// </summary>
//    private readonly IUserManager _userManager;

//    /// <summary>
//    /// 初始化一个<see cref="ErpOrderKhService"/>类型的新实例.
//    /// </summary>
//    public ErpOrderKhService(
//        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
//        IBillRullService billRullService,
//        ISqlSugarClient context,
//        IUserManager userManager)
//    {
//        _repository = erpOrderRepository;
//        _billRullService = billRullService;
//        _db = context.AsTenant();
//        _userManager = userManager;
//    }

//    /// <summary>
//    /// 获取订单信息.
//    /// </summary>
//    /// <param name="id">主键值.</param>
//    /// <returns></returns>
//    [HttpGet("{id}")]
//    public async Task<dynamic> GetInfo(string id)
//    {
//        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpOrderKhInfoOutput>();

//        var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
//            .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
//            .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
//            .Where(w => w.Fid == output.id)
//            .Select((w, x,b) => new ErpOrderdetailInfoOutput()
//            {
//                midName = x.Name,
//                productName = b.Name,
//                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
//            }, true)
//            .ToListAsync();
//        output.erpOrderdetailList = erpOrderdetailList;

//        var erpOrderoperaterecordList = await _repository.Context.Queryable<ErpOrderoperaterecordEntity>().Where(w => w.Fid == output.id).ToListAsync();
//        output.erpOrderoperaterecordList = erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordInfoOutput>>();

//        var erpOrderremarksList = await _repository.Context.Queryable<ErpOrderremarksEntity>().Where(w => w.OrderId == output.id).ToListAsync();
//        output.erpOrderremarksList = erpOrderremarksList.Adapt<List<ErpOrderremarksInfoOutput>>();

//        output.erpChildOrderList = await _repository.Context.Queryable<ErpOrderEntity>()
//           .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Oid == output.id && ddd.Cid == it.Id).Any()) // 过滤掉拆单记录
//           .OrderBy(it => it.CreatorTime, OrderByType.Asc)
//           .Select(it => new ErpSubOrderListOutput
//           {
//               id = it.Id,
//               no = it.No,
//               createUid = it.CreateUid,
//               createTime = it.CreateTime,
//               creatorTime = it.CreatorTime,
//               cid = it.Cid,
//               posttime = it.Posttime,
//               cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
//               createUidName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.CreateUid).Select(d => d.RealName),
//               amount = it.Amount,
//               state = it.State ?? 0,
//               diningType = it.DiningType,
//               oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
//           }).ToListAsync();

//        if (output.erpChildOrderList.IsAny())
//        {
//            var subIdList = output.erpChildOrderList.Select(x => x.id).ToList();

//            var erpSubOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
//           .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
//           .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
//           .Where(w => subIdList.Contains(w.Fid))
//           .Select((w, x, b) => new ErpOrderdetailInfoOutput()
//           {
//               midName = x.Name,
//               productName = b.Name,
//               rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
//           }, true)
//           .ToListAsync();

//            foreach (var item in output.erpChildOrderList)
//            {
//                item.erpOrderdetailList = erpSubOrderdetailList.Where(x => x.fid == item.id).ToList() ?? new List<ErpOrderdetailInfoOutput>();
//            }
//        }
//        return output;
//    }

//    /// <summary>
//    /// 获取订单信息列表.
//    /// </summary>
//    /// <param name="input">请求参数.</param>
//    /// <returns></returns>
//    [HttpGet("")]
//    public async Task<dynamic> GetList([FromQuery] ErpOrderKhListQueryInput input)
//    {
//        var data = await _repository.Context.Queryable<ErpOrderEntity>()
//            .Where(it=>it.Cid == input.cid)
//            .WhereIF(input.beginDate.HasValue, it=>it.CreateTime >= input.beginDate)
//            .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
//            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
//            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
//                it.No.Contains(input.keyword)
//                )
//            .Where(it => SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(ddd => ddd.Cid == it.Id).NotAny()) // 过滤掉拆单记录
//            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
//            .Select(it => new ErpOrderKhListOutput
//            {
//                id = it.Id,
//                no = it.No,
//                posttime = it.Posttime,
//                state = it.State ?? 0,
//                creatorTime = it.CreatorTime,
//                createTime = it.CreateTime,
//                diningType = it.DiningType,
//            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

//        if (data!=null &&data.list.IsAny())
//        {
//            foreach (var item in data.list)
//            {
//                if (item.posttime.HasValue)
//                {
//                    item.dayOfWeek = item.posttime.Value.ToString("dddd");
//                }
//            }
//        }
//        return PageResult<ErpOrderKhListOutput>.SqlSugarPageResult(data);
//    }

//    /// <summary>
//    /// 新建订单信息.
//    /// </summary>
//    /// <param name="input">参数.</param>
//    /// <returns></returns>
//    [HttpPost("")]
//    public async Task Create([FromBody] ErpOrderKhCrInput input)
//    {
//        var entity = input.Adapt<ErpOrderEntity>();
//        entity.Id = SnowflakeIdHelper.NextId();
//        entity.No = await _billRullService.GetBillNumber("QTErpOrder");
//        entity.State = OrderStateEnum.Draft;
//        entity.Amount = input.erpOrderdetailList?.Sum(a => a.amount) ?? 0;
//        try
//        {
//            // 开启事务
//            _db.BeginTran();

//            var newEntity = await _repository.Context.Insertable<ErpOrderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

//            var erpOrderdetailEntityList = input.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();
//            if(erpOrderdetailEntityList != null)
//            {
//                foreach (var item in erpOrderdetailEntityList)
//                {
//                    item.Id = SnowflakeIdHelper.NextId();
//                    item.Fid = newEntity.Id;
//                }

//                await _repository.Context.Insertable<ErpOrderdetailEntity>(erpOrderdetailEntityList).ExecuteCommandAsync();
//            }

//            //var erpOrderoperaterecordEntityList = input.erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordEntity>>();
//            //if(erpOrderoperaterecordEntityList != null)
//            //{
//            //    foreach (var item in erpOrderoperaterecordEntityList)
//            //    {
//            //        item.Id = SnowflakeIdHelper.NextId();
//            //        item.Fid = newEntity.Id;
//            //    }

//            //    await _repository.Context.Insertable<ErpOrderoperaterecordEntity>(erpOrderoperaterecordEntityList).ExecuteCommandAsync();
//            //}

//            var erpOrderremarksEntityList = input.erpOrderremarksList.Adapt<List<ErpOrderremarksEntity>>();
//            if(erpOrderremarksEntityList != null)
//            {
//                foreach (var item in erpOrderremarksEntityList)
//                {
//                    item.Id = SnowflakeIdHelper.NextId();
//                    item.OrderId = newEntity.Id;
//                }

//                await _repository.Context.Insertable<ErpOrderremarksEntity>(erpOrderremarksEntityList).ExecuteCommandAsync();
//            }

//            // 关闭事务
//            _db.CommitTran();
//        }
//        catch (Exception)
//        {
//            // 回滚事务
//            _db.RollbackTran();

//            throw Oops.Oh(ErrorCode.COM1000);
//        }
//    }

//    /// <summary>
//    /// 更新订单信息.
//    /// </summary>
//    /// <param name="id">主键值.</param>
//    /// <param name="input">参数.</param>
//    /// <returns></returns>
//    [HttpPut("{id}")]
//    public async Task Update(string id, [FromBody] ErpOrderKhUpInput input)
//    {
//        var entity = input.Adapt<ErpOrderEntity>();
//        input.erpOrderdetailList?.ForEach(x =>
//        {
//            if (x.rejectNum > 0)
//            {
//                x.amount = (x.num1 - x.rejectNum) * x.salePrice;
//            }
//        });
//        entity.Amount = input.erpOrderdetailList?.Sum(a => a.amount) ?? 0;
//        try
//        {
//            // 开启事务
//            _db.BeginTran();

//            await _repository.Context.Updateable<ErpOrderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).EnableDiffLogEvent().ExecuteCommandAsync();

//            // 订单明细
//            await _repository.Context.CUDSaveAsnyc<ErpOrderdetailEntity>(it => it.Fid == entity.Id, input.erpOrderdetailList, it => it.Fid = entity.Id);

//            //// 清空订单商品表原有数据
//            //await _repository.Context.Deleteable<ErpOrderdetailEntity>().Where(it => it.Fid == entity.Id).EnableDiffLogEvent().ExecuteCommandAsync();

//            //// 新增订单商品表新数据
//            //var erpOrderdetailEntityList = input.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();
//            //if(erpOrderdetailEntityList != null)
//            //{
//            //    foreach (var item in erpOrderdetailEntityList)
//            //    {
//            //        item.Id ??= SnowflakeIdHelper.NextId();
//            //        item.Fid = entity.Id;
//            //    }

//            //    await _repository.Context.Insertable<ErpOrderdetailEntity>(erpOrderdetailEntityList).EnableDiffLogEvent().ExecuteCommandAsync();
//            //}
//            //// 新增订单备注记录新数据 调用ErpOrderService.UpdateMemo直接添加

//            // 关闭事务
//            _db.CommitTran();
//        }
//        catch (Exception)
//        {
//            // 回滚事务
//            _db.RollbackTran();
//            throw Oops.Oh(ErrorCode.COM1001);
//        }
//    }

//    /// <summary>
//    /// 删除订单信息.
//    /// </summary>
//    /// <returns></returns>
//    [HttpDelete("{id}")]
//    public async Task Delete(string id)
//    {
//        // 软删除
//        var entity = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

//        // 分拣状态，判断是否有已分拣的数据，不允许删除
//        if (entity.State == OrderStateEnum.PendingApproval)
//        {
//            if (await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == id && x.SorterTime.HasValue).AnyAsync())
//            {
//                throw Oops.Oh("订单已分拣不允许删除！");
//            }
//        }
//        else if (entity.State != OrderStateEnum.Draft)
//        {
//            throw Oops.Oh("当前订单状态不允许删除！");
//        }

//        _repository.Context.Tracking(entity);
//        entity.DeleteTime = DateTime.Now;
//        entity.DeleteUserId = _userManager.UserId;
//        await _repository.Context.Updateable(entity).EnableDiffLogEvent().ExecuteCommandAsync();

//        //if (!await _repository.Context.Queryable<ErpOrderEntity>().AnyAsync(it => it.Id == id))
//        //{
//        //    throw Oops.Oh(ErrorCode.COM1005);
//        //}       

//        //try
//        //{
//        //    // 开启事务
//        //    _db.BeginTran();

//        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
//        //    await _repository.Context.Deleteable<ErpOrderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

//        //        // 清空订单商品表表数据
//        //    await _repository.Context.Deleteable<ErpOrderdetailEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

//        //        // 清空订单处理记录表表数据
//        //    await _repository.Context.Deleteable<ErpOrderoperaterecordEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

//        //        // 清空订单备注记录表数据
//        //    await _repository.Context.Deleteable<ErpOrderremarksEntity>().Where(it => it.OrderId.Equals(entity.Id)).ExecuteCommandAsync();

//        //    // 关闭事务
//        //    _db.CommitTran();
//        //}
//        //catch (Exception)
//        //{
//        //    // 回滚事务
//        //    _db.RollbackTran();

//        //    throw Oops.Oh(ErrorCode.COM1002);
//        //}
//    }
//}
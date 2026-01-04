using Mapster;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp.OrderCg;
using QT.JXC.Interfaces;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：订单信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOrderCg", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpOrderCgService : IErpOrderCgService, IDynamicApiController, ITransient
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
    private readonly IErpOrderTraceService _erpOrderTraceService;

    /// <summary>
    /// 初始化一个<see cref="ErpOrderCgService"/>类型的新实例.
    /// </summary>
    public ErpOrderCgService(
        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IDictionaryDataService dictionaryDataService,
        IErpOrderTraceService erpOrderTraceService)
    {
        _repository = erpOrderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
        _erpOrderTraceService = erpOrderTraceService;
    }

    /// <summary>
    /// 获取订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id);

        // 如果送货人没有，默认带出客户关联的客户
        if (string.IsNullOrEmpty(entity.DeliveryManId))
        {
            _repository.Context.Tracking(entity);
            entity.DeliveryManId = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == entity.Cid).Select(x => x.DeliveryManId).FirstAsync();

            if (string.IsNullOrEmpty(entity.DeliveryCar) && !string.IsNullOrEmpty(entity.DeliveryManId))
            {
                entity.DeliveryCar = await _repository.Context.Queryable<ErpDeliverymanEntity>().Where(x => x.Id == entity.DeliveryManId).Select(x => x.DeliveryCar).FirstAsync();
            }

            await _repository.Context.Updateable<ErpOrderEntity>(entity).ExecuteCommandAsync();
        }

        var output = entity.Adapt<ErpOrderCgInfoOutput>();

        var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((it, a) => it.Mid == a.Id)
            .InnerJoin<ErpProductEntity>((it, a, b) => a.Pid == b.Id)
            .Where(it => it.Fid == output.id)
            .Select((it, a, b) => new ErpOrderdetailInfoOutput
            {
                midName = a.Name,
                productName = b.Name,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName),
                midUnit = a.Unit
            }, true)
            .ToListAsync();
        output.erpOrderdetailList = erpOrderdetailList.OrderBy(x => x.order ?? 99).ToList(); // erpOrderdetailList.Adapt<List<ErpOrderdetailInfoOutput>>();

        // 判断关联特殊入库
        var detailIds = output.erpOrderdetailList.Select(x => x.id).ToArray();
        var tempList = await _repository.Context.Queryable<ErpInrecordEntity>().InnerJoin<ErpInorderEntity>((a, b) => a.InId == b.Id)
            .InnerJoin<ErpOutdetailRecordEntity>((a, b, c) => c.InId == a.Id)
            .Where((a, b, c) => b.InType == "5" && detailIds.Contains(c.OutId))
            .Select((a, b, c) => new
            {
                id = a.Id,
                outId = c.OutId
            })
            .ToListAsync();

        if (tempList.IsAny())
        {
            var inids =tempList.Select(x => x.id).ToArray();
            var tsList = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => inids.Contains(x.InId)).ToListAsync();

            foreach (var item in tempList)
            {
                var data = output.erpOrderdetailList.Find(x => x.id == item.outId);
                if (data!=null)
                {
                    data.tsNum = tsList.Where(x => x.InId == item.id).Sum(x => x.Num);
                }
            }
        }

        // 获取采购信息
        var inlist = await _repository.Context.Queryable<ErpOutdetailRecordEntity>()
            .InnerJoin<ErpInrecordEntity>((a, b) => a.InId == b.Id)
            .Where((a,b)=> output.erpOrderdetailList.Any(x=>x.id == a.OutId))
            .Select((a, b) => new
            {
                a.OutId,
                b.ProductionDate,
                b.Retention
            })
            .ToListAsync();

        foreach (var item in output.erpOrderdetailList)
        {
            var inlist2 = inlist.Where(x => x.OutId == item.id).ToList();
            if (inlist2.IsAny())
            {
                item.productionDate = inlist2.OrderByDescending(x => x.ProductionDate).FirstOrDefault()?.ProductionDate;
                item.retention = inlist2.OrderByDescending(x => x.Retention).FirstOrDefault()?.Retention;
            }
        }
            


        var erpOrderoperaterecordList = await _repository.Context.Queryable<ErpOrderoperaterecordEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.erpOrderoperaterecordList = erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordInfoOutput>>();

        var erpOrderremarksList = await _repository.Context.Queryable<ErpOrderremarksEntity>().Where(w => w.OrderId == output.id).ToListAsync();
        output.erpOrderremarksList = erpOrderremarksList.Adapt<List<ErpOrderremarksInfoOutput>>();

        //foreach (var item in output.erpOrderdetailList)
        //{
        //    var outlist = await _erpOrderTraceService.GetOrderOutList(item.id);
        //    if (outlist.IsAny())
        //    {
        //        var outdto = outlist.Where(x=>x.productDate.IsNotEmptyOrNull()).OrderByDescending(x => x.id).FirstOrDefault();
        //        if (outdto!=null)
        //        {
        //            item.productDate = outdto.productDate;
        //            item.retention = outdto.retention;
        //        }               
        //    }
        //}
        return output;
    }

    /// <summary>
    /// 获取订单信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpOrderCgListQueryInput input)
    {
        List<DateTime> createTimeRange = input.createTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreateTimeDate = createTimeRange?.First();
        DateTime? endCreateTimeDate = createTimeRange?.Last();

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

        var data = await _repository.Context.Queryable<ErpOrderEntity>()
            .Where(it=>it.State == OrderStateEnum.Picked || it.State == OrderStateEnum.PendingApproval)
            .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
            .WhereIF(input.createTime.HasValue, it => it.CreateTime >= input.createTime && it.CreateTime < input.createTime.Value.AddDays(1))
            .WhereIF(input.posttime.HasValue, it => it.Posttime >= input.posttime && it.Posttime < input.posttime.Value.AddDays(1))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid.Contains(input.cid))
            .WhereIF(cidList.IsAny(), it=> cidList.Contains(it.Cid))
            .WhereIF(input.diningType.IsNotEmptyOrNull(),it=>it.DiningType == input.diningType)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                )
            .WhereIF(createTimeRange != null, it => SqlFunc.Between(it.CreateTime, startCreateTimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreateTimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(posttimeRange != null, it => SqlFunc.Between(it.Posttime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(gidList.IsAny(), it=> SqlFunc.Subqueryable<ErpOrderdetailEntity>().Where(d=> d.Fid == it.Id && gidList.Contains(d.Mid)).Any())
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpOrderCgListOutput
            {
                id = it.Id,
                no = it.No,
                createUid = it.CreateUid,
                createTime = it.CreateTime,
                cid = it.Cid,
                posttime = it.Posttime,
                state = it.State ?? 0,
                cidName= SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
                deliveryCar= it.DeliveryCar,
                deliveryManIdName = SqlFunc.Subqueryable<ErpDeliverymanEntity>().Where(d => d.Id == it.DeliveryManId).Select(d => d.Name),
                amount =it.Amount,
                diningType = it.DiningType,
                isSub = SqlFunc.Subqueryable<ErpOrderRelationEntity>().Where(d=>d.Cid == it.Id ).Any(),
                remark = it.Remark
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);

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
        return PageResult<ErpOrderCgListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 仓库任务订单 更新订单信息.
    /// 只更新主表的送货人和送货车辆
    /// 还要更新订单的单价和金额
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpOrderCgUpInput input)
    {
        var entity = input.Adapt<ErpOrderEntity>();

        // 计算总价
        var erpOrderdetailList = input.erpOrderdetailList.Adapt<List<ErpOrderdetailEntity>>();

        // 判断分拣数量是否有变动，变动的话，禁止保存
        var dbOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => it.Fid == id).ToListAsync();
        foreach (var item in dbOrderdetailList)
        {
            var row = erpOrderdetailList.Find(x => x.Id == item.Id);
            if (row!=null)
            {
                if (row.Num1 != item.Num1)
                {
                    throw Oops.Oh("订单分拣数据发生变动，请重新打开订单再修改单价！");
                }
            }
        }

        // 判断是否有子单，更新子单的单价
        var detailIdList = erpOrderdetailList.Select(x => x.Id).ToList();
        var relations = await _repository.Context.Queryable<ErpOrderRelationEntity>().Where(w => detailIdList.Contains(w.Oid) && w.Type == nameof(ErpOrderdetailEntity)).ToListAsync();

        if (relations.Any())
        {
            detailIdList = relations.Select(x => x.Cid).ToList();
            var subOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
           .Where(x => detailIdList.Contains(x.Id))
           .ToListAsync();

            if (subOrderdetailList.IsAny())
            {
                _repository.Context.Tracking(subOrderdetailList);

                foreach (var item in subOrderdetailList)
                {
                    var detail = erpOrderdetailList.Find(w => relations.Any(x => x.Oid == w.Id && x.Cid == item.Id));
                    if (detail != null)
                    {
                        item.SalePrice = detail.SalePrice;
                        item.Amount = (item.Num1 - (item.RejectNum ?? 0)) * item.SalePrice;
                        item.Amount1 = (item.Num1 - (item.RejectNum ?? 0)) * item.SalePrice;
                    }
                }

                // 更新子单数据
                await _repository.Context.Updateable<ErpOrderdetailEntity>(subOrderdetailList).ExecuteCommandAsync();

                // 获取子单的主表
                detailIdList = subOrderdetailList.Select(x => x.Fid).ToList();
                var subOrderList = await _repository.Context.Queryable<ErpOrderEntity>().Where(it => detailIdList.Contains(it.Id)).ToListAsync();
                _repository.Context.Tracking(subOrderList);

                // 汇总子表订单数据
                var list2 = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => detailIdList.Contains(it.Fid))
                    .GroupBy(x => x.Fid)
                    .Select(x => new ErpOrderEntity
                    {
                        //Id = x.Id,
                        Id = x.Fid,
                        Amount = SqlFunc.AggregateSum(x.Amount),
                        Amount1 = SqlFunc.AggregateSum(x.Amount1),
                        Amount2 = SqlFunc.AggregateSum(x.Amount2),
                        //Num1 = x.Num1,
                        //Num2 = x.Num2,
                        //RejectNum = x.RejectNum,
                        //FjNum = x.FjNum
                    }).ToListAsync();

                foreach (var g in list2)
                {
                    var item = subOrderList.Find(x => x.Id == g.Id);
                    if (item != null)
                    {
                        item.Amount = g.Amount;
                        item.Amount1 = g.Amount1;
                        item.Amount2 = g.Amount2;
                    }
                }


                await _repository.Context.Updateable(subOrderList).ExecuteCommandAsync();

                /*
              foreach (var g in list2.GroupBy(x => x.Fid))
              {
                  var item = subOrderList.Find(x => x.Id == g.Key);
                  if (item!=null)
                  {
                      item.Amount = g.Sum(w => w.Amount);
                      item.Amount1 = g.Sum(w => w.Amount1);
                      item.Amount2 = g.Sum(w => w.Amount2);
                  }
              }



              foreach (var item in erpOrderdetailList)
              {
                  var xitems = list2.Where(x => relations.Any(w => w.Cid == x.Id && w.Oid == item.Id)).ToList();
                  if (xitems.Any())
                  {
                      item.Amount = xitems.Sum(w => w.Amount);
                      item.Amount1 = xitems.Sum(w => w.Amount1);
                      item.Amount2 = xitems.Sum(w => w.Amount2);
                      // 2024.4.11 整单完成才更新主单数据
                      //item.Num1 = xitems.Sum(w => w.Num1);
                      //item.Num2 = xitems.Sum(w => w.Num2);
                      //item.RejectNum = xitems.Sum(w => w.RejectNum);
                      //item.FjNum = xitems.Sum(w => w.FjNum);
                  }
              }
              */
            }


        }


        entity.Amount = erpOrderdetailList.Sum(x => x.Amount);

        await _repository.Context.Updateable<ErpOrderEntity>(entity).UpdateColumns(x => new { x.DeliveryManId, x.DeliveryCar, x.Amount }).ExecuteCommandAsync();


        // 更新单价
        if (erpOrderdetailList.IsAny())
        {
            _repository.Context.ClearTracking();
            await _repository.Context.Updateable(erpOrderdetailList).UpdateColumns(x => new { x.SalePrice, x.Amount, x.Amount1, x.PrintAlais, x.Remark,x.ProductionDate,x.Retention }).EnableDiffLogEvent().ExecuteCommandAsync();

            // 更新销售出库单的金额
            var detailIds = erpOrderdetailList.Select(it => it.Id).ToArray();
            var erpOutrecordEntitys = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(it => detailIds.Contains(it.OrderId)).ToListAsync();
            foreach (var item in erpOutrecordEntitys)
            {
                var orderdetailEntity = erpOrderdetailList.Find(it => it.Id == item.OrderId);
                if (orderdetailEntity != null)
                {
                    item.Price = orderdetailEntity.SalePrice;
                    item.Amount = orderdetailEntity.Amount1;
                }
            }

            await _repository.Context.Updateable<ErpOutrecordEntity>(erpOutrecordEntitys).UpdateColumns(x => new { x.Price, x.Amount }).ExecuteCommandAsync();

            // 更新出库主单的金额
            detailIds = erpOutrecordEntitys.Select(it => it.OutId).ToArray();
            var erpOutorderEntitys = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(it => detailIds.Contains(it.OutId)).GroupBy(it => it.OutId).Select(it => new ErpOutorderEntity
            {
                Id = it.OutId,
                Amount = SqlFunc.AggregateSum(it.Amount)
            }).ToListAsync();

            if (erpOutorderEntitys.IsAny())
            {
                await _repository.Context.Updateable<ErpOutorderEntity>(erpOutorderEntitys).UpdateColumns(x => new { x.Amount }).ExecuteCommandAsync();
            }
        }

        //2024.9.2 取消功能 #340
        //2024.9.2 设置客户类型是市场客户的可以自动识别，其他的客户类型不能识别 #356
        // 同步到客户类型定价
        // 获取客户类型
        var customer = await _repository.Context.Queryable<ErpCustomerEntity>()
            .Where(it => SqlFunc.Subqueryable<ErpOrderEntity>().Where(x => x.Cid == it.Id && x.Id == id).Any())
            .FirstAsync();

        //2025.10.14 不需要更新客户定价
        if (customer != null && customer.Type.IsNotEmptyOrNull() && false)
        {
            var typeOptions = await _dictionaryDataService.GetList("QTErpCustomType");
            if (typeOptions.Find(x => x.EnCode == customer.Type)?.FullName == "市场客户")
            {
                var midList = erpOrderdetailList.Select(it => it.Mid).Distinct().ToArray();
                var dbList = await _repository.Context.Queryable<ErpProductcustomertypepriceEntity>().Where(it => it.Tid == customer.Type && midList.Contains(it.Gid)).ToListAsync();

                List<ErpProductcustomertypepriceEntity> updateList = new List<ErpProductcustomertypepriceEntity>();
                List<ErpProductcustomertypepriceEntity> insertList = new List<ErpProductcustomertypepriceEntity>();
                foreach (var item in erpOrderdetailList)
                {
                    if (item.SalePrice == 0)
                    {
                        continue;
                    }
                    var data = dbList.Find(x => x.Oid == customer.Oid && x.Gid == item.Mid && x.Tid == customer.Type);
                    if (data != null)
                    {
                        //如果是按折扣的或者价格相等的，不处理
                        if (data.PricingType == 1 || data.Price == item.SalePrice)
                        {
                            continue;
                        }
                        data.Price = item.SalePrice;
                        updateList.Add(data);
                    }
                    else
                    {
                        insertList.Add(new ErpProductcustomertypepriceEntity
                        {
                            Price = item.SalePrice,
                            Gid = item.Mid,
                            Discount = 0,
                            Id = SnowflakeIdHelper.NextId(),
                            PricingType = 2,
                            Tid = customer.Type,
                            Oid = customer.Oid
                        });
                    }
                }

                if (updateList.IsAny())
                {
                    await _repository.Context.Updateable(updateList).UpdateColumns(it => it.Price).ExecuteCommandAsync();
                }
                if (insertList.IsAny())
                {
                    await _repository.Context.Insertable(insertList).ExecuteCommandAsync();
                }
            }            
        }
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpOrderCgListQueryInput input, [FromServices]IFileManager fileManager, [FromServices] IDictionaryDataService dictionaryDataService)
    {
        input.pageSize = 10000;
        List<string> items = new List<string>();
        if (input.items.IsNotEmptyOrNull())
        {
            items = input.items.Split(",", true).ToList();
        }
        if (input.dataType == 1)
        {
            items.Clear();
        }
        else
        {
            items.Add(SnowflakeIdHelper.NextId()); //防止查询全部
        }
        List<DateTime> createTimeRange = input.createTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreateTimeDate = createTimeRange?.First();
        DateTime? endCreateTimeDate = createTimeRange?.Last();

        List<DateTime> posttimeRange = input.posttimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPosttimeDate = posttimeRange?.First();
        DateTime? endPosttimeDate = posttimeRange?.Last();

       

        var list = await _repository.Context.Queryable<ErpOrderEntity>()
            .LeftJoin<ErpOrderdetailEntity>((it,itd)=>it.Id == itd.Fid)
            .InnerJoin<ErpProductmodelEntity>((it, itd,a) => itd.Mid == a.Id)
          .InnerJoin<ErpProductEntity>((it,itd, a, b) => a.Pid == b.Id)
            .WhereIF(input.activedTab == "1", it => it.State == OrderStateEnum.Picked || it.State == OrderStateEnum.PendingApproval)
            .WhereIF(items.IsAny(), it => items.Contains(it.Id))
            .WhereIF(input.beginDate.HasValue, it => it.CreateTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreateTime <= input.endDate)
            .WhereIF(input.createTime.HasValue, it => it.CreateTime >= input.createTime && it.CreateTime < input.createTime.Value.AddDays(1))
            .WhereIF(input.posttime.HasValue, it => it.Posttime >= input.posttime && it.Posttime < input.posttime.Value.AddDays(1))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cid), it => it.Cid.Contains(input.cid))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.Cid.Contains(input.keyword)
                )
            .WhereIF(createTimeRange != null, it => SqlFunc.Between(it.CreateTime, startCreateTimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreateTimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(posttimeRange != null, it => SqlFunc.Between(it.Posttime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select((it,itd,a,b) => new ErpOrderCgListExportOutput
            {
                id = itd.Id,
                no = it.No,
                createUid = it.CreateUid,
                createTime = it.CreateTime,
                cid = it.Cid,
                posttime = it.Posttime,
                state = it.State ?? 0,
                cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Name),
                deliveryCar = it.DeliveryCar,
                deliveryManIdName = SqlFunc.Subqueryable<ErpDeliverymanEntity>().Where(d => d.Id == it.DeliveryManId).Select(d => d.Name),
                //
                midName = a.Name,
                productName = b.Name,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName),
                damount = itd.Amount,
                midUnit = a.Unit,
                remark = it.Remark,
                itRemark = itd.Remark,
                num1 = itd.Num1,
                diningType = it.DiningType,
                cidType = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == it.Cid).Select(d => d.Type),
            },true).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .Take(input.pageSize)
            .ToListAsync();

        if (list.IsAny())
        {
            var unitOptions = await dictionaryDataService.GetList("JLDW");
            var erpOrderDiningTypeOptions = await dictionaryDataService.GetList("ErpOrderDiningType");
            var cidTypeOptions = await dictionaryDataService.GetList("QTErpCustomType");
            foreach (var item in list)
            {
                if (item.posttime.HasValue)
                {
                    item.dayOfWeek = item.posttime.Value.ToString("dddd");
                }
                var unit = unitOptions.Find(x => x.EnCode == item.midUnit);
                if (unit != null)
                {
                    item.midUnit = unit.FullName;
                }

                item.diningType = erpOrderDiningTypeOptions.Find(x => x.EnCode == item.diningType)?.FullName ?? "";
                item.cidType = cidTypeOptions.Find(x => x.EnCode == item.cidType)?.FullName ?? "";

                if (item.rejectNum>0)
                {
                    item.rejectPrice = item.salePrice;
                    item.rejectAmount = Math.Round(item.salePrice * item.rejectNum, 2);
                }
            }
        }

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_{1}.xls", DateTime.Now, input.activedTab == "1" ? "仓管任务":"历史订单" ));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOrderCgListExportData>();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpOrderCgListExportData>.ExportMemoryStream(list.Adapt<List<ErpOrderCgListExportData>>(), excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }


        return new { name = excelconfig.FileName, url = flag.Item2 };
    }


    /// <summary>
    /// 批量提交
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPost("actions/auditBatch")]
    [SqlSugarUnitOfWork]
    public async Task<int> AuditBatch([FromBody]List<string> ids)
    {
        // 找出符合条件的订单
        var orders = await _repository
            .Where(x => ids.Contains(x.Id) && x.State == OrderStateEnum.Picked)
            .Where(x=> SqlFunc.HasValue(x.DeliveryManId) && SqlFunc.HasValue(x.DeliveryCar))
            .ToListAsync();

        var state = OrderStateEnum.Outbound;
        List<ErpOrderoperaterecordEntity> erpOrderoperaterecordEntities = new List<ErpOrderoperaterecordEntity>();
        foreach (var order in orders)
        {
            _repository.Context.Tracking(order);
            order.State = state;

            var erpOrderoperaterecordEntity = new ErpOrderoperaterecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                State = ((int)state).ToString(),
                Fid = order.Id,
                Time = DateTime.Now,
                UserId = _userManager.RealName,
                Remark = $"[{_userManager.RealName}]将订单状态改为[{(int)state}-{state.ToDescription()}]"
            };

            erpOrderoperaterecordEntities.Add(erpOrderoperaterecordEntity);
        }

        if (erpOrderoperaterecordEntities.IsAny())
        {
            await _repository.Context.Updateable<ErpOrderEntity>(orders).ExecuteCommandAsync();

            //写入订单日志
            await _repository.Context.Insertable(erpOrderoperaterecordEntities).ExecuteCommandAsync();
        }

        return orders.Count;
    }

    /// <summary>
    /// 根据订单id更新从表的打印次数
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Print")]
    public async Task UpdatePrintCount(string id)
    {
        await _repository.Context.Updateable<ErpOrderdetailEntity>()
            .SetColumns(it=> it.PrintCount == (it.PrintCount ?? 0) + 1)
            .Where(it=> it.Fid == id)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 根据订单明细id更新从表的打印次数
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/Detail/Print")]
    public async Task UpdatePrintCount(List<string> input)
    {
        if (input.IsAny())
        {
            await _repository.Context.Updateable<ErpOrderdetailEntity>()
            .SetColumns(it => it.PrintCount == (it.PrintCount ?? 0) + 1)
            .Where(it => input.Contains(it.Id))
            .ExecuteCommandAsync();
        }
        
    }
}

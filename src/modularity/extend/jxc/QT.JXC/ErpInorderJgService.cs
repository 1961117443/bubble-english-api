using Mapster;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp.BuyOrder;
using QT.JXC.Entitys.Dto.ErpOrderGj;
using QT.JXC.Interfaces;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：加工入库表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpInorderJg", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpInorderJgService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpInorderEntity> _repository;

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
    private readonly IErpStoreService _erpStoreService;
    private readonly IErpOutorderService _erpOutorderService;

    /// <summary>
    /// 初始化一个<see cref="ErpInorderService"/>类型的新实例.
    /// </summary>
    public ErpInorderJgService(
        ISqlSugarRepository<ErpInorderEntity> erpInorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IErpStoreService erpStoreService,
        IErpOutorderService erpOutorderService)
    {
        _repository = erpInorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _erpStoreService = erpStoreService;
        _erpOutorderService = erpOutorderService;
    }

    /// <summary>
    /// 获取入库订单表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpInorderJgInfoOutput>();

        var erpInrecordList = await _repository.Context.Queryable<ErpInrecordEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
            .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
            .Where(w => w.InId == output.id)
            .Select((w, a, b) => new ErpInrecordInfoOutput
            {
                gidName = a.Name,
                productName = b.Name,
                productPrice = a.SalePrice
            }, true)
            .ToListAsync();
        output.erpInrecordList = erpInrecordList.Adapt<List<ErpInrecordJgInfoOutput>>();

        var erpOutrecordList = await _repository.Context.Queryable<ErpOutrecordEntity>()
           .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
           .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
           .Where(w => w.OutId == output.id)
           .Select((w, a, b) => new ErpOutrecordInfoOutput
           {
               gidName = a.Name,
               productName = b.Name
           }, true).ToListAsync();

        var ids = erpInrecordList.Select(x => x.id).ToArray();
        if (ids.IsAny())
        {
            var list = await _repository.Context.Queryable<ErpOutdetailRecordEntity>()
                .InnerJoin<ErpOrderdetailEntity>((a, b) => a.OutId == b.Id)
                .InnerJoin<ErpOrderEntity>((a, b, c) => b.Fid == c.Id)
                .InnerJoin<ErpInrecordEntity>((a, b, c, d) => a.InId == d.Id)
                .LeftJoin<ErpCustomerEntity>((a, b, c, d, e) => c.Cid == e.Id)
                .Where((a, b, c, d) => ids.Contains(d.Id))
                .Select((a, b, c, d, e) => new
                {
                    no = c.No,
                    id = d.Id,
                    cidName = e.Name
                }).ToListAsync();

            // 获取特殊入库
            var tsList = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                .Where(it => ids.Contains(it.InId)  )
                .ToListAsync();


            foreach (var item in output.erpInrecordList)
            {
                var q = list.Where(it => it.id == item.id);
                var xlist = q.Select(it => it.no).Distinct().ToArray();
                if (xlist.IsAny())
                {
                    item.orderNo = string.Join("，", xlist);
                }
                item.cidName = string.Join("，", q.Select(it => it.cidName).Distinct().ToArray());

                item.erpTsInrecordList = tsList.Where(x => x.InId == item.id).Select(x => new ErpInrecordTransferInputV2Item
                {
                    id = x.TsId,
                    num = x.Num
                }).ToList();

                item.tsNum = item.erpTsInrecordList.Sum(x => x.num);
            }
        }


        output.erpOutrecordList = erpOutrecordList;
        return output;
    }

    /// <summary>
    /// 新建调拨入库表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpInorderJgCrInput input)
    {
        var entity = input.Adapt<ErpInorderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpJGRK");

        //加工出库实体
        ErpOutorderEntity outEntity = new()
        {
            Id = entity.Id,
            InType = "3", //加工出库
            //No = entity.No, // 同一个流水号，方便查找 // await _billRullService.GetBillNumber("QTErpJGCK"),
            No = await _billRullService.GetBillNumber("QTErpJGCK"),
            Oid = input.oid,
            Remark = $"由加工入库单[{entity.No}]生成"
        };


        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        // 获取出库关联的入库订单
        string bid = string.Empty;

        var relationIds = input.erpOutrecordList.SelectMany(x => x.storeDetailList).Select(x => x.id).Where(x => x.IsNotEmptyOrNull()).ToList();
        if (relationIds.IsAny())
        {
            bid = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => relationIds.Contains(x.Id)).Where(x => !SqlFunc.IsNullOrEmpty(x.Bid)).Select(x => x.Bid).FirstAsync();

            if (bid.IsNotEmptyOrNull())
            {
                entity.CgNo = await _repository.Context.Queryable<ErpBuyorderEntity>().Where(g=> SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == g.Id && x.Id == bid).Any()).Select(x => x.No).FirstAsync();
            }
        }



        entity.Amount = input.erpInrecordList.Sum(a => a.amount);
        var newEntity = await _repository.Context.Insertable<ErpInorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

     
        var erpInrecordEntityList = new List<ErpInrecordEntity>();
        var erpOutrecordEntityList = new List<ErpOutrecordEntity>();
        var tsNum = 0m;
        if (input.erpInrecordList.IsAny())
        {
            List<ErpInrecordTsEntity> erpTsInrecordList = new List<ErpInrecordTsEntity>();
            foreach (var x in input.erpInrecordList)
            {
                var item = x.Adapt<ErpInrecordEntity>();
                item.Id = SnowflakeIdHelper.NextId();
                item.InId = newEntity.Id;
                item.Num = item.InNum;
                if (!string.IsNullOrEmpty(newEntity.Oid))
                {
                    item.Oid = newEntity.Oid;
                }
                erpInrecordEntityList.Add(item);

                if (x.erpTsInrecordList.IsAny())
                {
                    tsNum = x.erpTsInrecordList.Sum(z => z.num);

                    erpTsInrecordList.AddRange(x.erpTsInrecordList.Select(x => new ErpInrecordTsEntity
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        InId = item.Id,
                        TsId = x.id,
                        Num = x.num
                    }));
                }
            }
            //erpInrecordEntityList[0].Bid = bid;// 2025.8.18 不记得为什么要赋值，暂时去掉，赋值后会关联不上加工任务

            await _repository.Context.Insertable(erpInrecordEntityList).ExecuteCommandAsync();

            // 插入特殊关联 

            if (erpTsInrecordList.IsAny())
            {
                await _repository.Context.Insertable(erpTsInrecordList).ExecuteCommandAsync();

                // 更新关联的特殊入库单价
                await _erpOutorderService.CheckTsRecord(erpInrecordEntityList[0].Id);
            }
        }

        // 生成出库记录
        if (input.erpOutrecordList.IsAny())
        {  
            //加工出库实体
            var outNewEntity = await _repository.Context.Insertable<ErpOutorderEntity>(outEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            foreach (var x in input.erpOutrecordList)
            {
                // 生成调拨出库
                var outItem = x.Adapt<ErpOutrecordCrInput>().Adapt<ErpOutrecordEntity>();
                outItem.Id = SnowflakeIdHelper.NextId();
                outItem.OutId = outNewEntity.Id;
                if (!string.IsNullOrEmpty(newEntity.Oid))
                {
                    outItem.Oid = newEntity.Oid;
                }
                erpOutrecordEntityList.Add(outItem);

                if (x.storeDetailList != null && x.storeDetailList.Any())
                {
                    var cost = await _erpStoreService.Reduce(new ErpOutdetailRecordUpInput
                    {
                        id = outItem.Id,
                        num = outItem.Num,
                        records = x.storeDetailList.Adapt<List<ErpOutdetailRecordInInput>>()
                    });

                    outItem.CostAmount = cost.CostAmount;
                }
            }


            //插入出库明细
            await _repository.Context.Insertable(erpOutrecordEntityList).ExecuteCommandAsync();

            // 更新入库明细的入库单价和金额
            var costAmount = erpOutrecordEntityList.Sum(x => x.CostAmount);
            if (costAmount > 0)
            {
                var inrecordEntity = erpInrecordEntityList[0];
                var totalNum = inrecordEntity.InNum + tsNum;
                inrecordEntity.Price = Math.Round(costAmount / totalNum, 2);
                if (inrecordEntity.InNum == 0)
                {
                    inrecordEntity.Amount = 0;
                }
                else if (tsNum == 0)
                {
                    inrecordEntity.Amount = costAmount;
                }
                else
                {
                    inrecordEntity.Amount = inrecordEntity.Price  * inrecordEntity.InNum;
                }

                    await _repository.Context.Updateable<ErpInrecordEntity>(inrecordEntity).UpdateColumns(new string[] { "Price", "Amount" }).ExecuteCommandAsync();
            }
        }

       

        //    // 关闭事务
        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    // 回滚事务
        //    _db.RollbackTran();

        //    throw Oops.Oh(ErrorCode.COM1000);
        //}
    }

    /// <summary>
    /// 删除入库订单表.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        if (!await _repository.Context.Queryable<ErpInorderEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        // 判断明细是否已经做过出库
        var inidList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(it => it.InId.Equals(id)).Select(it => it.Id).ToArrayAsync();
        if (await _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(it => inidList.Contains(it.InId)).AnyAsync())
        {
            throw Oops.Oh("明细已做出库记录，不允许删除");
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        await _repository.Context.Deleteable<ErpInorderEntity>().Where(it => it.Id.Equals(id)).EnableDiffLogEvent().ExecuteCommandAsync();

        //删除加工出库主表记录
        await _repository.Context.Deleteable<ErpOutorderEntity>().Where(it => it.Id.Equals(id)).EnableDiffLogEvent().ExecuteCommandAsync();


        // 找出出库记录明细，然后恢复库存
        var records = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(it => it.OutId.Equals(id)).Select(x => new ErpOutrecordEntity { Id = x.Id }).ToListAsync();
        foreach (var record in records)
        {
            await _erpStoreService.Restore(record.Id);
        }


        // 清空商品入库记录表数据
        await _repository.Context.Deleteable<ErpInrecordEntity>().Where(it => it.InId.Equals(entity.Id)).EnableDiffLogEvent().ExecuteCommandAsync();

        // 清空商品出库记录表数据
        await _repository.Context.Deleteable<ErpOutrecordEntity>(records).EnableDiffLogEvent().ExecuteCommandAsync();

        // 删除关联特殊入库记录
        await _repository.Context.Deleteable<ErpInrecordTsEntity>().Where(x => inidList.Contains( x.InId)).ExecuteCommandAsync();

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
    /// 获取加工任务订单列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Task")]
    public async Task<dynamic> GetTaskList([FromQuery] ErpOrderGjListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ViewErpProductJgTask>()
            .WhereIF(input.status == "1", it => it.WaitJgNum > 0)
            .WhereIF(input.status == "2", it => it.WaitJgNum <= 0)
            .Where(it => it.Oid == _userManager.CompanyId)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it => it.GidName.Contains(input.keyword) || it.ProductName.Contains(input.keyword))
            .OrderByDescending(it => it.CreatorTime)
            .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<ViewErpProductJgTask>.SqlSugarPageResult(data);
    }


    /// <summary>
    /// 初始化完工数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("Task/{id}")]
    public async Task<dynamic> InitTask(string id)
    {
        var data = await _repository.Context.Queryable<ViewErpProductJgTask>()
             .Where(it => it.Oid == _userManager.CompanyId)
             .Where(it => it.Id == id)
             .FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);

        var relation = await _repository.Context.Queryable<ErpProductEntity>()
            .InnerJoin<ErpProductmodelEntity>((a, b) => a.Id == b.Pid)
            .Where((a, b) => b.Id == data.Rid)
            .Select((a, b) => new
            {
                productName = a.Name,
                name = b.Name
            }).FirstAsync() ?? throw Oops.Oh("缺少关联产品");

        return new
        {
            inType = "3",
            oid = _userManager.CompanyId,
            erpInrecordList = new[]
            {
                new
                {
                    gid = data.Gid,
                    gidName = data.GidName,
                    productName = data.ProductName,
                    storeRomeId= "",
                    storeRomeAreaId= "",
                    waitJgNum = data.WaitJgNum,
                    planNum = data.PlanNum,
                    inNum= "", // data.WaitJgNum,
            //"bid": "532348460809086213",
                    price= 0,
                    bid = data.Id
                }
            },
            erpOutrecordList = new[]
            {
                new
                {
                    gid= data.Rid,
                    gidName= relation.name,
                    num= 0,
                    relation.productName,
                    storeDetailList= new object[0],
                    amount= 0
                }
            }
        };
    }

    /// <summary>
    /// 根据入库明细id获取待选记录
    /// </summary>
    /// <param name="inid">入库明细id</param>
    /// <param name="mid">规格id</param>
    /// <returns></returns>
    [HttpGet("Actions/Transfer/v2/{mid}")]
    public async Task<dynamic> GetTransferListV2([FromQuery]string inid, string mid)
    {
        var qur = _repository.Context.Queryable<ErpInrecordTsEntity>().GroupBy(c => c.TsId).Select(c => new
        {
            c.TsId,
            Num = SqlFunc.AggregateSum(c.Num)
        });
        // 当前选中的记录
        var value = inid.IsNotEmptyOrNull() ?
            await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId == inid).Select(x => x.TsId).ToListAsync()
            : new List<string>();
        var list = await _repository.Context.Queryable<ErpInorderEntity>()
            .InnerJoin<ErpInrecordEntity>((a, b) => a.Id == b.InId)
            .LeftJoin(qur, (a, b, c) => b.Id == c.TsId)
            .Where((a, b) => a.InType == "5" /*&& (a.SpecialState ?? "") == ""*/ && b.Gid == mid)  // 未完成的特殊入库记录
                                                                                                   //.Where((a, b) => SqlFunc.Subqueryable<ErpInrecordTsEntity>().Where(ddd => ddd.TsId == b.Id).NotAny() || value.Contains(b.Id))
            .Where((a, b, c) => b.InNum > SqlFunc.IsNull(c.Num, 0) || value.Contains(b.Id))
            .OrderBy((a, b) => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new ErpBuyorderTransferListOutput
            {
                id = b.Id,
                creatorTime = a.CreatorTime,
                num = b.InNum,
                no = a.No
            })
            .ToListAsync();

        var inDetailIds = list.Select(x => x.id).ToList();
        var relationOrder = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().InnerJoin<ErpOrderdetailEntity>((a, b) => a.OutId == b.Id)
                   .InnerJoin<ErpOrderEntity>((a, b, c) => b.Fid == c.Id)
                   .InnerJoin<ErpInrecordEntity>((a, b, c, d) => a.InId == d.Id)
                   .Where((a, b, c, d) => inDetailIds.Contains(d.Id))
                   .Select((a, b, c, d) => new
                   {
                       no = c.No,
                       id = d.Id,
                       posttime = c.Posttime,
                       customerName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == c.Cid).Select(ddd => ddd.Name)
                   }).ToListAsync();

        foreach (var item in relationOrder)
        {
            var xitem = list.FirstOrDefault(x => x.id == item.id);
            if (xitem != null)
            {
                xitem.posttime = item.posttime;
                xitem.orderNo = item.no;
                xitem.customerName = item.customerName;
            }
        }

        // 汇总特殊入库的数量
        var tsids = list.Select(x => x.id);
        var tsAll = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(it => tsids.Contains(it.TsId)).ToListAsync();

        foreach (var item in list)
        {
            item.num1 = tsAll.Where(x => x.TsId == item.id && x.InId != inid).Sum(x => x.Num);
            item.num2 = tsAll.Where(x => x.TsId == item.id && x.InId == inid).Sum(x => x.Num);

            if (item.num2 > 0)
            {
                item.ifselected = true;
            }
        }

        return new
        {
            value = value ?? new List<string>(),
            data = list,
        };
    }
}

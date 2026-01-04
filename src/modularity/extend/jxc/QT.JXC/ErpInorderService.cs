using Mapster;
using Microsoft.AspNetCore.Authorization;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Service;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp;
using QT.JXC.Entitys.Dto.Erp.BuyOrder;
using QT.JXC.Entitys.Entity;
using QT.JXC.Entitys.Entity.ERP;
using QT.JXC.Entitys.Views;
using QT.JXC.Interfaces;
using QT.Logging.Attributes;
using QT.Reflection.Extensions;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using QT.UnifyResult;
using System.Linq.Expressions;

namespace QT.JXC;

/// <summary>
/// 业务实现：入库订单表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpInorder", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpInorderService : IErpInorderService, IDynamicApiController, ITransient
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
    private readonly IDictionaryDataService _dictionaryDataService;
    private readonly ICoreSysConfigService _coreSysConfigService;

    /// <summary>
    /// 初始化一个<see cref="ErpInorderService"/>类型的新实例.
    /// </summary>
    public ErpInorderService(
        ISqlSugarRepository<ErpInorderEntity> erpInorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IDictionaryDataService dictionaryDataService,
        ICoreSysConfigService coreSysConfigService)
    {
        _repository = erpInorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
        _coreSysConfigService = coreSysConfigService;
    }

    /// <summary>
    /// 获取入库订单表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        _repository.Context.QueryFilter.Clear<ICompanyEntity>();
        var output = (await _repository.AsQueryable().ClearFilter<ICompanyEntity>().FirstAsync(x => x.Id == id)).Adapt<ErpInorderInfoOutput>() ?? throw Oops.Oh(ErrorCode.COM1005);


        var erpInrecordList = await _repository.Context.Queryable<ErpInrecordEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
            .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
            .Where(w => w.InId == output.id)
            .Select((w, a, b) => new ErpInrecordInfoOutput
            {
                gidName = a.Name,
                productName = b.Name,
                tsNum = SqlFunc.Subqueryable<ErpInrecordEntity>().LeftJoin<ErpInrecordTsEntity>((z,dd)=>dd.TsId == z.Id)
                .Where((z,dd)=>  dd.InId == w.Id).Sum((z,dd)=>dd.Num)
            }, true)
            .ToListAsync();

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

            foreach (var item in erpInrecordList)
            {
                var q = list.Where(it => it.id == item.id);
                var xlist = q.Select(it => it.no).Distinct().ToArray();
                if (xlist.IsAny())
                {
                    item.orderNo = string.Join("，", xlist);
                }
                item.cidName = string.Join("，", q.Select(it => it.cidName).Distinct().ToArray());
            }
        }


        output.erpInrecordList = erpInrecordList; //.Adapt<List<ErpInrecordInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取入库订单表列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<PageResult<ErpInorderListOutput>> GetList([FromQuery] ErpInorderListQueryInput input)
    {
        List<DateTime> creatorTimeRange = input.creatorTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreatorTimeDate = creatorTimeRange?.First();
        DateTime? endCreatorTimeDate = creatorTimeRange?.Last();

        List<string> cgIds = new List<string>();
        if (input.orderNo.IsNotEmptyOrNull())
        {
            cgIds = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().InnerJoin<ErpOrderdetailEntity>((a, b) => a.OutId == b.Id)
                    .InnerJoin<ErpOrderEntity>((a, b, c) => b.Fid == c.Id)
                    .InnerJoin<ErpInrecordEntity>((a, b, c, d) => a.InId == d.Id)
                    .Where((a, b, c, d) => c.No.Contains(input.orderNo))
                    .Select((a, b, c, d) => d.InId)
                    .Distinct()
                    .Take(input.pageSize)
                    .ToListAsync();
        }

        if (input.postTimeRange.IsNotEmptyOrNull())
        {
            List<DateTime> postTimeRange = input.postTimeRange?.Split(',').ToObject<List<DateTime>>();
            var startPostTimeDate = postTimeRange?.First();
            var endPostTimeDate = postTimeRange?.Last().ToString("yyyy-MM-dd 23:59:59");

            cgIds = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().InnerJoin<ErpOrderdetailEntity>((a, b) => a.OutId == b.Id)
                    .InnerJoin<ErpOrderEntity>((a, b, c) => b.Fid == c.Id)
                    .InnerJoin<ErpInrecordEntity>((a, b, c, d) => a.InId == d.Id)
                    .Where((a, b, c, d) => SqlFunc.Between(c.Posttime, startPostTimeDate,endPostTimeDate))
                    .Select((a, b, c, d) => d.InId)
                    .Distinct()
                    .Take(input.pageSize)
                    .ToListAsync();
        }

        var items = input.items?.Split(",", true).ToList();

        var data = await _repository.Context.Queryable<ErpInorderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cgNo), it => it.CgNo.Contains(input.cgNo))
            .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
            .WhereIF(input.specialState == "0",it=>(it.SpecialState ?? "") == "")
            .WhereIF(input.specialState == "1", it => it.SpecialState == "1")
            .WhereIF(input.specialState == "2", it =>  SqlFunc.Subqueryable<ErpInrecordEntity>()
                                                                        .InnerJoin<ErpInrecordTsEntity>((a,b)=>a.Id == b.TsId)
                                                                        /*.InnerJoin<ErpBuyorderdetailEntity>((a, b,c) => b.InId== c.Id)*/
                                                                        .Where((a,b)=> a.InId == it.Id)
                                                                        .NotAny()) // 未关联，没有关联采购明细
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.InType.Contains(input.keyword)
                )
            .WhereIF(input.productName.IsNotEmptyOrNull(), it=> SqlFunc.Subqueryable<ErpInrecordEntity>().Where(d1=>d1.InId == it.Id 
                    && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2=>d2.Id==d1.Gid
                    && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3=>d3.Id == d2.Pid && d3.Name.Contains(input.productName)).Any() ).Any()).Any())
            .WhereIF(creatorTimeRange != null, it => SqlFunc.Between(it.CreatorTime, startCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(cgIds.IsAny(), it=> cgIds.Contains(it.Id))
            .WhereIF(input.hasNum == true, it=> (it.InType == "1" || it.InType == "5") && SqlFunc.Subqueryable<ErpInrecordEntity>().Where(ddd=>ddd.InId== it.Id && ddd.InNum>0).Any())
            .WhereIF(items.IsAny(), it=> items.Contains(it.Id))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpInorderListOutput
            {
                id = it.Id,
                oid = it.Oid,
                no = it.No,
                inType = it.InType,
                amount = it.Amount,
                cgNo = it.CgNo,
                specialState = it.SpecialState ?? "0",
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
                outOidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.OutOid).Select(d => d.FullName),
                num = SqlFunc.Subqueryable<ErpInrecordEntity>().Where(d=>d.InId == it.Id).Count(),
                productNames = SqlFunc.Subqueryable<ErpInrecordEntity>().LeftJoin<ErpProductmodelEntity>((a,b)=> a.Gid == b.Id)
                .LeftJoin<ErpProductEntity>((a,b,c)=>b.Pid == c.Id).Where((a,b,c)=>a.InId == it.Id)
                .SelectStringJoin((a,b,c)=>c.Name,","),
                totalNum = SqlFunc.Subqueryable<ErpInrecordEntity>().Where(d => d.InId == it.Id).Sum(ddd=>ddd.InNum),
                creatorTime = it.CreatorTime,
                supplierName = SqlFunc.Subqueryable<ErpInrecordEntity>().LeftJoin<ErpBuyorderdetailEntity>((a, b) => a.Bid == b.Id)
                .LeftJoin<ErpSupplierEntity>((a, b, c) => b.Supplier == c.Id).Where((a, b, c) => a.InId == it.Id)
                //.GroupBy((a, b, c) => c.Name)
                .Select((a,b,c)=> SqlFunc.MappingColumn<string>(" group_concat(DISTINCT `c`.`F_Name` SEPARATOR ',' ) ")),
                gidName = SqlFunc.Subqueryable<ErpInrecordEntity>().LeftJoin<ErpProductmodelEntity>((a, b) => a.Gid == b.Id)
                .Where((a, b) => a.InId == it.Id)
                .SelectStringJoin((a, b) => b.Name, ","),
                gidUnit = SqlFunc.Subqueryable<ErpInrecordEntity>().LeftJoin<ErpProductmodelEntity>((a, b) => a.Gid == b.Id)
                .Where((a, b) => a.InId == it.Id)
                .Select((a, b) => b.Unit),
                //.SelectStringJoin((a, b, c) => c.Name, ",")
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        //if (input.inType == "5" || input.inType == "1")
        {
            var ids = data.list.Select(x => x.id).ToArray();
            if (ids.IsAny())
            {
                var list = await _repository.Context.Queryable<ErpOutdetailRecordEntity>()
                    .InnerJoin<ErpOrderdetailEntity>((a, b) => a.OutId == b.Id)
                    .InnerJoin<ErpOrderEntity>((a, b, c) => b.Fid == c.Id)
                    .InnerJoin<ErpInrecordEntity>((a, b, c, d) => a.InId == d.Id)
                    .LeftJoin<ErpCustomerEntity>((a, b, c, d, e) => c.Cid == e.Id)
                    .Where((a, b, c, d) => ids.Contains(d.InId))
                    .Select((a, b, c, d, e) => new
                    {
                        no = c.No,
                        id = d.InId,
                        cidName = e.Name,
                    }).ToListAsync();

                foreach (var item in data.list)
                {
                    var q = list.Where(it => it.id == item.id);
                    var xlist = q.Select(it => it.no).Distinct().ToArray();
                    if (xlist.IsAny())
                    {
                        item.orderNo = string.Join(",", xlist);
                    }
                    item.cidName = string.Join(",", q.Select(it => it.cidName).Distinct().ToArray());
                }

                ids = data.list.Where(x => x.inType == "5").Select(x=>x.id).ToArray();
                if (ids.IsAny())
                {
                    // 获取采购单号
                    var cgNos = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                        .InnerJoin<ErpBuyorderdetailEntity>((a, b) => a.InId == b.Id)
                        .InnerJoin<ErpBuyorderEntity>((a, b, c) => b.Fid == c.Id)
                        .InnerJoin<ErpInrecordEntity>((a,b,c,d)=> d.Id == a.TsId)
                        .Where((a, b, c,d) => ids.Contains(d.InId))
                        .Select((a, b, c,d) => new
                        {
                            d.InId,
                            c.No
                        })
                        .ToListAsync();

                    

                    foreach (var item in data.list)
                    {
                        var cgno = cgNos.FirstOrDefault(x => x.InId == item.id);
                        if (cgno!=null && item.cgNo.IsNullOrEmpty() && item.inType == "5")
                        {
                            item.cgNo = cgno.No;
                        }
                    }

                    if (input.inType == "5")
                    {
                        var tsnumList = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                        .InnerJoin<ErpInrecordEntity>((x, a) => x.TsId == a.Id)
                        .Where((x, a) => ids.Contains(a.InId))
                          .GroupBy((x, a) => a.InId)
                          .Select((x, a) => new
                          {
                              Id = a.InId,
                              Num = SqlFunc.AggregateSum(x.Num)
                          }).ToListAsync();

                        foreach (var item in data.list)
                        {
                            var e = tsnumList.FirstOrDefault(x => x.Id == item.id);
                            if (e!=null)
                            {
                                item.tsnum = e.Num;
                                item.untsnum = item.totalNum - item.tsnum;
                            }
                        }
                    }

                }
            }


        }
        return PageResult<ErpInorderListOutput>.SqlSugarPagedList(data);
    }

    /// <summary>
    /// 新建入库订单表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpInorderCrInput input)
    {
        var entity = input.Adapt<ErpInorderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpInOrder");

        var newEntity = await _repository.Context.Insertable<ErpInorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

        var erpInrecordEntityList = input.erpInrecordList.Adapt<List<ErpInrecordEntity>>();
        if (erpInrecordEntityList != null)
        {
            foreach (var item in erpInrecordEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.InId = newEntity.Id;
                item.Num = item.InNum;

                if (!string.IsNullOrEmpty(newEntity.Oid))
                {
                    item.Oid = newEntity.Oid;
                }
            }

            await _repository.Context.Insertable(erpInrecordEntityList).ExecuteCommandAsync();
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var newEntity = await _repository.Context.Insertable<ErpInorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

        //    var erpInrecordEntityList = input.erpInrecordList.Adapt<List<ErpInrecordEntity>>();
        //    if (erpInrecordEntityList != null)
        //    {
        //        foreach (var item in erpInrecordEntityList)
        //        {
        //            item.Id = SnowflakeIdHelper.NextId();
        //            item.InId = newEntity.Id;
        //            item.Num = item.InNum;

        //            if (!string.IsNullOrEmpty(newEntity.Oid))
        //            {
        //                item.Oid = newEntity.Oid;
        //            }
        //        }

        //        await _repository.Context.Insertable<ErpInrecordEntity>(erpInrecordEntityList).ExecuteCommandAsync();
        //    }

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
    /// 更新入库订单表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpInorderUpInput input)
    {
        var entity = input.Adapt<ErpInorderEntity>();

        var items = await _repository.Context.Queryable<ErpInrecordEntity>().Where(it => it.InId == entity.Id).ToListAsync();

        foreach (var item in items)
        {
            //判断明细是否已出库而且还修改数量
            var inNum = input.erpInrecordList?.Find(x => x.id == item.Id)?.inNum ?? 0; //本次入库数量

            //变动了数量，并且已经出库的话，不允许保存，抛出异常
            if (inNum != item.InNum && item.Num != item.InNum)
            {
                throw Oops.Oh("订单已出库，不允许修改！");
            }
        }

        //if (await _repository.Context.Queryable<ErpInrecordEntity>().AnyAsync(it => it.InId == entity.Id && it.Num != it.InNum))
        //{
        //    throw Oops.Oh("订单已出库，不允许修改！");
        //}
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<ErpInorderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 新增商品入库记录新数据
            var erpInrecordEntityList = input.erpInrecordList.Adapt<List<ErpInrecordEntity>>();
            if (erpInrecordEntityList != null)
            {
                foreach (var item in erpInrecordEntityList)
                {
                    item.Id ??= SnowflakeIdHelper.NextId();
                    item.InId = entity.Id;
                    item.Num = item.InNum;

                    //判断是否有出库
                    var xitem = items.Find(x => x.Id == item.Id);
                    if (xitem != null && xitem.Num != xitem.InNum)
                    {
                        var use = xitem.InNum - xitem.Num;
                        item.Num = item.InNum - use;

                        if (item.Num < 0)
                        {
                            throw Oops.Oh($"订单已出库，入库数量不能少于出库数量[{use}]！");
                        }
                    }
                }
                // 清空商品入库记录原有数据
                await _repository.Context.Deleteable<ErpInrecordEntity>().Where(it => it.InId == entity.Id).ExecuteCommandAsync();
                await _repository.Context.Insertable(erpInrecordEntityList).ExecuteCommandAsync();
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

        // 判断特殊入库是否关联采购入库
        if (await _repository.Context.Queryable<ErpInrecordTsEntity>()
            .Where(it => inidList.Contains(it.TsId) && SqlFunc.Subqueryable<ErpInrecordEntity>().Where(x=>x.InId == it.InId).Any())
            .AnyAsync())
        {
            throw Oops.Oh(ErrorCode.D1007);
        }

        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        await _repository.Context.Deleteable<ErpInorderEntity>().Where(it => it.Id.Equals(id)).EnableDiffLogEvent().ExecuteCommandAsync();

        // 清空商品入库记录表数据
        await _repository.Context.Deleteable<ErpInrecordEntity>().Where(it => it.InId.Equals(entity.Id)).EnableDiffLogEvent().ExecuteCommandAsync();

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        //    await _repository.Context.Deleteable<ErpInorderEntity>().Where(it => it.Id.Equals(id)).EnableDiffLogEvent().ExecuteCommandAsync();

        //    // 清空商品入库记录表数据
        //    await _repository.Context.Deleteable<ErpInrecordEntity>().Where(it => it.InId.Equals(entity.Id)).EnableDiffLogEvent().ExecuteCommandAsync();

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
    /// 根据采购单号，获取待入库明细
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpGet("Buyorder")]
    public async Task<dynamic> GetBuyorder([FromQuery] string cgNo)
    {
        var cgentity = await _repository.Context.Queryable<ErpBuyorderEntity>().FirstAsync(x => x.No == cgNo && x.State == "2") ?? throw Oops.Oh(ErrorCode.COM1005);

        var dbList = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == cgentity.Id).ToListAsync();

        var erpBuyorderdetailList = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
           .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
           .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
           .Where(w => w.Fid == cgentity.Id)
           .Select((w, a, b) => new ErpBuyorderdetailInfoOutput
           {
               gidName = a.Name,
               productName = b.Name,
           }, true)
           .ToListAsync();

        //统计已入库数量
        var ids = erpBuyorderdetailList.Select(x => x.id).ToArray();
        var erpBuyorderdetailInList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => ids.Contains(x.Bid))
            .GroupBy(x => x.Bid)
            .Select(x => new
            {
                id = x.Bid,
                num = SqlFunc.AggregateSum(x.InNum)
            })
            .ToListAsync();

        return erpBuyorderdetailList.Select(x =>
        {
            var entity = erpBuyorderdetailInList.Find(it => it.id == x.id);
            var info = new ErpInrecordInfoOutput
            {
                productName = x.productName,
                gidName = x.gidName,
                gid = x.gid,
                orderNum = x.planNum,
                inNum = entity == null ? x.planNum : x.planNum - entity.num,
                bid = x.id
            };
            return info;
        }).Where(x => x.inNum > 0).ToList();


    }

    /// <summary>
    /// 完成订单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("complete/{id}")]
    public async Task Complete([FromRoute] string id)
    {
        var entity = await _repository.Context.Queryable<ErpInorderEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.SpecialState == "1")
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        _repository.Context.Tracking(entity);
        entity.SpecialState = "1";
        entity.SpecialUserId = _userManager.UserId;
        await _repository.Context.Updateable(entity).ExecuteCommandAsync();
    }


    /// <summary>
    /// 根据商品规格id，获取库存记录
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpGet("Store/{gid}")]
    public async Task<List<ErpStoreInfoOutput>> GetStore([FromRoute] string gid, [FromQuery] ErpStoreListQueryInput input)
    {
        var query = _repository.AsQueryable()
            .InnerJoin<ErpInrecordEntity>((x, w) => x.Id == w.InId)
       .InnerJoin<ErpProductmodelEntity>((x, w, a) => w.Gid == a.Id)
       .InnerJoin<ErpProductEntity>((x, w, a, b) => a.Pid == b.Id);

        if (!string.IsNullOrEmpty(input.oid))
        {
            query = query
                .ClearFilter<ICompanyEntity>().Where((x, w, a, b) => x.Oid == input.oid);
        }

        string pid = string.Empty;
        if (input.otherSpec)
        {
            pid = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it => it.Id == gid).Select(it => it.Pid).FirstAsync();
        }

        var erpInrecordList = await query
            .LeftJoin<ErpBuyorderdetailEntity>((x,w,a,b,c)=> c.Id == w.Bid)
            .Where((x, w, a, b,c) => w.Num > 0)
            .WhereIF(!input.otherSpec, (x, w, a, b,c) => w.Gid == gid)
            .WhereIF(input.otherSpec && !string.IsNullOrEmpty(pid), (x, w, a, b,c) => w.Gid != gid && b.Id == pid)
            .WhereIF(input.cidType.IsNotEmptyOrNull(), (x, w, a, b, c) => SqlFunc.IsNullOrEmpty(c.CustomerType) || QTSqlFunc.FIND_IN_SET(input.cidType, c.CustomerType))
       .OrderBy((x, w, a, b, c) => w.CreatorTime)
       .Select((x, w, a, b,c) => new ErpStoreInfoOutput
       {
           id = w.Id,
           gid = w.Gid,
           num = w.Num,
           checkTime = a.CheckTime,
           creatorTime = w.CreatorTime,
           gidName = a.Name,
           productName = b.Name,
           storeRome = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(d => d.Id == w.StoreRomeId).Select(d => d.Name),
           storeRomeArea = SqlFunc.Subqueryable<ErpStoreareaEntity>().Where(e => e.Id == w.StoreRomeAreaId).Select(e => e.Name),
           price = w.Price,
           productId = b.Id,
           storeRomeAreaId = w.StoreRomeAreaId,
           storeRomeId = w.StoreRomeId,
           inType = x.InType,
           billId = x.Id,
           cgNo = x.CgNo,
           productionDate = w.ProductionDate,
           retention = w.Retention,
           supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(d => d.Id == c.Supplier).Select(d => d.Name),
           oid = w.Oid,
           oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == w.Oid).Select(d => d.FullName),
       })
       .ToListAsync();

        if (!input.otherSpec)
        {
            // 判断关联库存是否有数据
            var config = await _coreSysConfigService.GetConfig<SysErpConfigOutput>();

            if (config.erpShareStock.IsNotEmptyOrNull())
            {
                var oid = input.oid ?? _userManager.CompanyId;
                var companys = config.erpShareStock.Split(",", true);
                if (companys.IsAny() && companys.Contains(oid))
                {
                    var erpInrecordList2 = await _repository.AsQueryable()
                           .InnerJoin<ErpInrecordEntity>((x, w) => x.Id == w.InId)
                           .InnerJoin<ErpProductmodelEntity>((x, w, a) => w.Gid == a.Id)
                           .InnerJoin<ErpProductEntity>((x, w, a, b) => a.Pid == b.Id)
                           .LeftJoin<ErpBuyorderdetailEntity>((x, w, a, b, c) => c.Id == w.Bid)
                           .ClearFilter<ICompanyEntity>().Where((x, w, a, b,c) => x.Oid != oid && companys.Contains(x.Oid))
                           .Where((x, w, a, b, c) => w.Num > 0)
                           .WhereIF(!input.otherSpec, (x, w, a, b, c) => w.Gid == gid)
                           .WhereIF(input.otherSpec && !string.IsNullOrEmpty(pid), (x, w, a, b, c) => w.Gid != gid && b.Id == pid)
                           .WhereIF(input.cidType.IsNotEmptyOrNull(), (x, w, a, b, c) => SqlFunc.IsNullOrEmpty(c.CustomerType) || QTSqlFunc.FIND_IN_SET(input.cidType, c.CustomerType))
                      .OrderBy((x, w, a, b, c) => w.CreatorTime)
                      .Select((x, w, a, b, c) => new ErpStoreInfoOutput
                      {
                          id = w.Id,
                          gid = w.Gid,
                          num = w.Num,
                          checkTime = a.CheckTime,
                          creatorTime = w.CreatorTime,
                          gidName = a.Name,
                          productName = b.Name,
                          storeRome = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(d => d.Id == w.StoreRomeId).Select(d => d.Name),
                          storeRomeArea = SqlFunc.Subqueryable<ErpStoreareaEntity>().Where(e => e.Id == w.StoreRomeAreaId).Select(e => e.Name),
                          price = w.Price,
                          productId = b.Id,
                          storeRomeAreaId = w.StoreRomeAreaId,
                          storeRomeId = w.StoreRomeId,
                          inType = x.InType,
                          billId = x.Id,
                          cgNo = x.CgNo,
                          productionDate = w.ProductionDate,
                          retention = w.Retention,
                          supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(d => d.Id == c.Supplier).Select(d => d.Name),
                          oid = w.Oid,
                          oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == w.Oid).Select(d => d.FullName),
                      })
                      .ToListAsync();

                    if (erpInrecordList2.IsAny())
                    {
                        erpInrecordList.AddRange(erpInrecordList2);
                    }                    
                }
            }
        }
           

            return erpInrecordList;

    }

    /// <summary>
    /// 获取库存记录
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpGet("Store")]
    public async Task<dynamic> GetStoreList([FromQuery] ErpStorePageListQueryInput input)
    {
        var erpInrecordList= await GetStoreListQuery(input);
        return PageResult<ErpStoreInfoOutput>.SqlSugarPageResult(erpInrecordList);

    }

    /// <summary>
    /// 获取库存汇总(根据规格汇总)
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpGet("Store/SumByGid")]
    public async Task<List<ErpStoreInfoOutput>> GetStoreSumList([FromQuery] ErpStorePageListQueryInput input)
    {
        //var erpInrecordList = await GetStoreListQuery(input);
        var qur = await getQuery(input);
        var list = await qur.MergeTable()
            .GroupBy(x => new { x.oid, x.gid })
            .Select(x => new ErpStoreInfoOutput
            {
                id = SqlFunc.MergeString(x.oid, "_", x.gid),
                gid = x.gid,
                num = SqlFunc.AggregateSum(x.num),
                checkTime = null,
                creatorTime = SqlFunc.AggregateMin(x.creatorTime),
                gidName = SqlFunc.AggregateMax(x.gidName),
                productName = SqlFunc.AggregateMax(x.productName),
                storeRome = SqlFunc.AggregateMax(x.storeRome),
                storeRomeArea = SqlFunc.AggregateMax(x.storeRomeArea),
                oid = x.oid,
                oidName = SqlFunc.AggregateMax(x.oidName),
                tid = SqlFunc.AggregateMax(x.tid),
                tidName = SqlFunc.AggregateMax(x.tidName),
                price = SqlFunc.AggregateMax(x.price),
                //retention =b.Retention,
                retention = SqlFunc.AggregateMax(x.retention),
                ratio = SqlFunc.AggregateMax(x.ratio),
                productionDate = SqlFunc.AggregateMax(x.productionDate),
                batchNumber = SqlFunc.AggregateMax(x.batchNumber),
                gidUnit = SqlFunc.AggregateMax(x.gidUnit),
                inType = SqlFunc.AggregateMax(x.inType),
                billId = SqlFunc.AggregateMax(x.billId),
                storeRomeAreaId = SqlFunc.AggregateMax(x.storeRomeAreaId),
                storeRomeId = SqlFunc.AggregateMax(x.storeRomeId),
                productId = SqlFunc.AggregateMax(x.productId),
                cgNo = SqlFunc.AggregateMax(x.cgNo),
                no = SqlFunc.AggregateMax(x.no),
                supplierName = SqlFunc.AggregateMax(x.supplierName),
                qualityReportProof = SqlFunc.AggregateMax(x.qualityReportProof),
                amount = SqlFunc.AggregateSum(SqlFunc.IIF(x.num ==0,0,x.num * x.price)),
                rootProducttype = SqlFunc.AggregateMax(x.rootProducttype)
            })
            .ToListAsync();

        foreach (var item in list)
        {
            if (item.creatorTime.HasValue)
            {
                item.days = (DateTime.Now - item.creatorTime).Value.Days;
            }

            if (item.num>0 && item.amount>0)
            {
                item.price = Math.Round(item.amount / item.num, 2);
            }
            else
            {
                item.price = 0;
            }
        }

        return list;

    }

    /// <summary>
    /// 获取库存历史记录
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpGet("Store/SumByGid/History")]
    public async Task<PageResult<ErpStoreHistoryInfoOutput>> GetStoreSumListToHistory([FromQuery] ErpStorePageListQueryInput input)
    {
        // 找出最后的时间戳
        var lastTime = await _repository.Context.Queryable<ErpStoreHistoryEntity>()
            .WhereIF(input.cutDate.HasValue, (x) => SqlFunc.Between(x.cutDate, input.cutDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), input.cutDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderByDescending(x => x.cutDate)
            .Select(x=>x.cutDate)
            .FirstAsync();

        var query = _repository.Context.Queryable<ErpStoreHistoryEntity>();

        //if (!string.IsNullOrEmpty(input.oid))
        //{
        //    query = query.ClearFilter<ICompanyEntity>().Where((x, w, a, b) => x.Oid == input.oid);
        //}

        Expression<Func<ErpStoreHistoryEntity, bool>> where = null;
        if (!string.IsNullOrEmpty(input.gid))
        {
            where = (x) => x.gid == input.gid;
            if (input.whetherRelation)
            {
                var rid = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it => it.Id == input.gid).Select(it => it.Rid).FirstAsync();
                if (!string.IsNullOrEmpty(rid))
                {
                    where = (x) => x.gid == input.gid || x.gid == rid;
                }
            }
        }

        List<DateTime> inttimeRange = input.intimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = inttimeRange?.First();
        DateTime? endDate = inttimeRange?.Last();

        var data = await query.WhereIF(where != null, where)
            .WhereIF(lastTime.HasValue , x=>x.cutDate == lastTime)
            .WhereIF(!string.IsNullOrEmpty(input.tid), (x) => x.tid == input.tid)
            .WhereIF(!string.IsNullOrEmpty(input.oid), (x) => x.oid == input.oid)
            .WhereIF(input.rootTypeId.IsNotEmptyOrNull(), (x) => SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == x.tid && ddd.RootId == input.rootTypeId).Any())
            .WhereIF(input.cutDate.HasValue, (x) => SqlFunc.Between(x.cutDate, input.cutDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), input.cutDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (x) => x.gidName.Contains(input.keyword) || x.productName.Contains(input.keyword))
            .WhereIF(inttimeRange != null, (x) => SqlFunc.Between(x.creatorTime, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
           .OrderByDescending((x) => new { x.oid, x.gid, x.creatorTime })
           .Select<ErpStoreHistoryInfoOutput>(x=> new ErpStoreHistoryInfoOutput
           {
               rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == x.tid).Select(ddd => ddd.RootName)
           },true)
           .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<ErpStoreHistoryInfoOutput>.SqlSugarPagedList(data);
    }

    /// <summary>
    /// 库存汇总(根据规格汇总) 导入历史记录
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpPost("Store/SumByGid/History")]
    public async Task<int> PostStoreSumListToHistory()
    {
        var list = await GetStoreSumList(new ErpStorePageListQueryInput
        {
            allOid = true
        });

        if (list.IsAny())
        {
            var cutDate = DateTime.Now; // 确保同一个时间戳
            var entitys = list.Adapt<List<ErpStoreHistoryEntity>>();
            entitys.ForEach(x =>
            {
                x.cutDate = cutDate;
                x.CreatorUserId = _userManager.UserId;
            });
           return await _repository.Context.Insertable(entitys).ExecuteCommandAsync();
        }
        return 0;
    }

    /// <summary>
    /// 库存汇总(根据规格汇总) 导入历史记录
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpPost("Store/SumByGid/History/Auto")]
    [AllowAnonymous]
    public async Task PostStoreSumListToHistoryAuto()
    {
        // 每天11.50分定时调用该接口，保存历史数据，需要先判断当前是否有历史数据，如果有，则不保存
        var dt = DateTime.Now;
        var ifExists = await _repository.Context.Queryable<ErpStoreHistoryEntity>()
            .Where(it => SqlFunc.Between(it.cutDate, dt.ParseToDateTime("yyyy-MM-dd 00:00:00"), dt.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .AnyAsync();

        if (!ifExists)
        {
           await PostStoreSumListToHistory();
        }
    }

    private async Task<ISugarQueryable<ErpStoreInfoOutput>> getQuery(ErpStorePageListQueryInput input)
    {
        var query = _repository.AsQueryable()
          .InnerJoin<ErpInrecordEntity>((x, w) => x.Id == w.InId)
     .InnerJoin<ErpProductmodelEntity>((x, w, a) => w.Gid == a.Id)
     .InnerJoin<ErpProductEntity>((x, w, a, b) => a.Pid == b.Id)
           .LeftJoin<ErpBuyorderdetailEntity>((x, w, a, b, c) => c.Id == w.Bid);
        //.LeftJoin<ErpProducttypeEntity>((x, w, a, b, c) => b.Tid == c.Id);

        if (!string.IsNullOrEmpty(input.oid))
        {
            query = query
                .ClearFilter<ICompanyEntity>().Where((x, w, a, b) => x.Oid == input.oid);
        }

        if (input.allOid == true)
        {
            query = query.ClearFilter<ICompanyEntity>();
        }

        Expression<Func<ErpInorderEntity, ErpInrecordEntity, ErpProductmodelEntity, ErpProductEntity, bool>> where = null;
        if (!string.IsNullOrEmpty(input.gid))
        {
            where = (x, w, a, b) => a.Id == input.gid;
            if (input.whetherRelation)
            {
                var rid = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it => it.Id == input.gid).Select(it => it.Rid).FirstAsync();
                if (!string.IsNullOrEmpty(rid))
                {
                    where = (x, w, a, b) => a.Id == input.gid || a.Id == rid;
                }
            }
        }

        List<DateTime> inttimeRange = input.intimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = inttimeRange?.First();
        DateTime? endDate = inttimeRange?.Last();

        var qur = query
            .Where((x, w, a, b) => w.Num > 0)
            .WhereIF(where != null, where)
            .WhereIF(!string.IsNullOrEmpty(input.tid), (x, w, a, b) => b.Tid == input.tid)
            .WhereIF(input.rootTypeId.IsNotEmptyOrNull(), (x, w, a, b) => SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid && ddd.RootId == input.rootTypeId).Any())
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (x, w, a, b) => a.Name.Contains(input.keyword) || b.Name.Contains(input.keyword))
            .WhereIF(inttimeRange != null, (x, w, a, b) => SqlFunc.Between(x.CreatorTime, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
           .OrderByDescending((x, w, a, b) => new { x.Oid, w.Gid, w.CreatorTime })
           .Select((x, w, a, b, c) => new ErpStoreInfoOutput
           {
               id = w.Id,
               gid = w.Gid,
               num = w.Num,
               checkTime = a.CheckTime,
               creatorTime = w.CreatorTime,
               gidName = a.Name,
               productName = b.Name,
               storeRome = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(d => d.Id == w.StoreRomeId).Select(d => d.Name),
               storeRomeArea = SqlFunc.Subqueryable<ErpStoreareaEntity>().Where(e => e.Id == w.StoreRomeAreaId).Select(e => e.Name),
               oid = w.Oid,
               oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == w.Oid).Select(d => d.FullName),
               tid = b.Tid,
               tidName = SqlFunc.Subqueryable<ErpProducttypeEntity>().Where(d => d.Id == b.Tid).Select(d => d.Name),
               price = w.Price,
               //retention =b.Retention,
               retention = w.Retention,
               ratio = a.Ratio,
               productionDate = w.ProductionDate,
               batchNumber = w.BatchNumber,
               gidUnit = a.Unit,
               inType = x.InType,
               billId = x.Id,
               cgNo = x.CgNo,
               no = x.No,
               supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(d => d.Id == c.Supplier).Select(d => d.Name),
               qualityReportProof = w.QualityReportProof,
               amount = x.Amount,
               storeRomeAreaId = w.StoreRomeAreaId,
               storeRomeId = w.StoreRomeId,
               productId = a.Pid,
               rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
           });


        return qur;
    }

    private async Task<SqlSugarPagedList<ErpStoreInfoOutput>> GetStoreListQuery(ErpStorePageListQueryInput input)
    {
        var query = _repository.AsQueryable()
           .InnerJoin<ErpInrecordEntity>((x, w) => x.Id == w.InId)
      .InnerJoin<ErpProductmodelEntity>((x, w, a) => w.Gid == a.Id)
      .InnerJoin<ErpProductEntity>((x, w, a, b) => a.Pid == b.Id)
            .LeftJoin<ErpBuyorderdetailEntity>((x, w, a, b, c) => c.Id == w.Bid);
        //.LeftJoin<ErpProducttypeEntity>((x, w, a, b, c) => b.Tid == c.Id);

        if (!string.IsNullOrEmpty(input.oid))
        {
            query = query
                .ClearFilter<ICompanyEntity>().Where((x, w, a, b) => x.Oid == input.oid);
        }

        Expression<Func<ErpInorderEntity, ErpInrecordEntity, ErpProductmodelEntity, ErpProductEntity, bool>> where = null;
        if (!string.IsNullOrEmpty(input.gid))
        {
            where = (x, w, a, b) => a.Id == input.gid;
            if (input.whetherRelation)
            {
                var rid = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it => it.Id == input.gid).Select(it => it.Rid).FirstAsync();
                if (!string.IsNullOrEmpty(rid))
                {
                    where = (x, w, a, b) => a.Id == input.gid || a.Id == rid;
                }
            }
        }

        List<DateTime> inttimeRange = input.intimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = inttimeRange?.First();
        DateTime? endDate = inttimeRange?.Last();

        var qur = query
            .Where((x, w, a, b) => w.Num > 0)
            .WhereIF(where != null, where)
            .WhereIF(!string.IsNullOrEmpty(input.tid), (x, w, a, b,c) => b.Tid == input.tid)
            .WhereIF(input.rootTypeId.IsNotEmptyOrNull(), (x, w, a, b) => SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid && ddd.RootId == input.rootTypeId).Any())
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (x, w, a, b,c) => a.Name.Contains(input.keyword) || b.Name.Contains(input.keyword))
            .WhereIF(inttimeRange != null, (x, w, a, b,c) => SqlFunc.Between(x.CreatorTime, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
           .OrderByDescending((x, w, a, b,c) => new { x.Oid, w.Gid, w.CreatorTime })
           .Select((x, w, a, b, c) => new ErpStoreInfoOutput
           {
               id = w.Id,
               gid = w.Gid,
               num = w.Num,
               checkTime = a.CheckTime,
               creatorTime = w.CreatorTime,
               gidName = a.Name,
               productName = b.Name,
               storeRome = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(d => d.Id == w.StoreRomeId).Select(d => d.Name),
               storeRomeArea = SqlFunc.Subqueryable<ErpStoreareaEntity>().Where(e => e.Id == w.StoreRomeAreaId).Select(e => e.Name),
               oid = w.Oid,
               oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == w.Oid).Select(d => d.FullName),
               tid = b.Tid,
               tidName = SqlFunc.Subqueryable<ErpProducttypeEntity>().Where(d => d.Id == b.Tid).Select(d => d.Name),
               price = w.Price,
               //retention =b.Retention,
               retention = w.Retention,
               ratio = a.Ratio,
               productionDate = w.ProductionDate,
               batchNumber = w.BatchNumber,
               gidUnit = a.Unit,
               inType = x.InType,
               billId = x.Id,
               cgNo = x.CgNo,
               no = x.No,
               supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(d => d.Id == c.Supplier).Select(d => d.Name),
               qualityReportProof = w.QualityReportProof,
               rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
           });

        var erpInrecordList = await qur.ToPagedListAsync(input.currentPage, input.pageSize);

        foreach (var item in erpInrecordList.list)
        {
            if (item.creatorTime.HasValue)
            {
                item.days = (DateTime.Now - item.creatorTime).Value.Days;
            }
            item.amount = item.num == 0 ? 0 : Math.Round(item.num * item.price, 2);
        }

        // 合计金额和数量
        var summary= await  query.Clone()
            .Where((x, w, a, b) => w.Num > 0)
            .WhereIF(where != null, where)
            .WhereIF(!string.IsNullOrEmpty(input.tid), (x, w, a, b) => b.Tid == input.tid)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (x, w, a, b) => a.Name.Contains(input.keyword) || b.Name.Contains(input.keyword))
            .WhereIF(inttimeRange != null, (x, w, a, b) => SqlFunc.Between(x.CreatorTime, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select((x, w) => new
            {
                num = SqlFunc.AggregateSum(w.Num),
                amount = SqlFunc.Round(SqlFunc.AggregateSum(SqlFunc.IIF(w.Num == 0, 0, w.Num * w.Price)),2)
            })
            .FirstAsync();

        UnifyContext.Fill(summary);


        return erpInrecordList;
    }

    /// <summary>
    /// 获取库存记录
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpGet("Store/ExportExcel")]
    public async Task<dynamic> StoreExportExcel([FromQuery] ErpStorePageListQueryInput input, [FromServices]IFileManager fileManager)
    {
        input.pageSize = 10000;
        var data = await GetStoreListQuery(input);

        var list = data.list ?? new List<ErpStoreInfoOutput>();

        // 单位转换
        var unitOptioins = await _dictionaryDataService.GetList("JLDW");
        if (list.IsAny())
        {
            foreach (var item in list)
            {
                if (item.gidUnit.IsNotEmptyOrNull())
                {
                    var unit = unitOptioins.Find(x => x.EnCode == item.gidUnit);
                    if (unit!=null)
                    {
                        item.gidUnit = unit.FullName;
                    }
                }

            }
        }

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_{1}.xls", DateTime.Now, typeof(ErpStoreInfoOutput).GetDescription()));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpStoreInfoOutput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpStoreInfoOutput>.ExportMemoryStream(list.Adapt<List<ErpStoreInfoOutput>>(), excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }


        return new  { name = excelconfig.FileName, url = flag.Item2 };

    }

    /// <summary>
    /// 导出Excel.
    /// 有固定的模板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpInorderListQueryInput input, [FromServices] IFileManager fileManager)
    {
        /*
        List<DateTime> queryTaskBuyTime = input.taskBuyTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startTaskBuyTime = queryTaskBuyTime?.First();
        DateTime? endTaskBuyTime = queryTaskBuyTime?.Last();

        List<string> suppliers1 = new List<string>();
        List<string> productNames = new List<string>();
        if (input.supplierName.IsNotEmptyOrNull())
        {
            suppliers1 = await _repository.Context.Queryable<ErpSupplierEntity>().Where(x => x.Name.Contains(input.supplierName)).Take(100).Select(x => x.Id).ToListAsync();
        }

        if (input.productName.IsNotEmptyOrNull())
        {
            productNames = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it => SqlFunc.Subqueryable<ErpProductEntity>().Where(x => x.Id == it.Pid && x.Name.Contains(input.productName)).Any())
                .Take(100).Select(x => x.Id).ToListAsync();
        }*/

        List<string> ids = new List<string>();

        if (input.dataType == 2)
        {
            ids = input.items.Split(",", true).ToList();
        }
        else
        {
            List<DateTime> creatorTimeRange = input.creatorTimeRange?.Split(',').ToObject<List<DateTime>>();
            DateTime? startCreatorTimeDate = creatorTimeRange?.First();
            DateTime? endCreatorTimeDate = creatorTimeRange?.Last();

            List<string> cgIds = new List<string>();
            if (input.orderNo.IsNotEmptyOrNull())
            {
                cgIds = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().InnerJoin<ErpOrderdetailEntity>((a, b) => a.OutId == b.Id)
                        .InnerJoin<ErpOrderEntity>((a, b, c) => b.Fid == c.Id)
                        .InnerJoin<ErpInrecordEntity>((a, b, c, d) => a.InId == d.Id)
                        .Where((a, b, c, d) => c.No.Contains(input.orderNo))
                        .Select((a, b, c, d) => d.InId)
                        .Distinct()
                        .Take(input.pageSize)
                        .ToListAsync();
            }

            var tempQuery =  _repository.Context.Queryable<ErpInorderEntity>()
                .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
                .WhereIF(!string.IsNullOrEmpty(input.cgNo), it => it.CgNo.Contains(input.cgNo))
                .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
                .WhereIF(input.specialState == "0", it => (it.SpecialState ?? "") == "")
                .WhereIF(input.specialState == "1", it => it.SpecialState == "1")
                .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                    it.No.Contains(input.keyword)
                    || it.InType.Contains(input.keyword)
                    )
                .WhereIF(input.productName.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpInrecordEntity>().Where(d1 => d1.InId == it.Id
                        && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2 => d2.Id == d1.Gid
                        && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3 => d3.Id == d2.Pid && d3.Name.Contains(input.productName)).Any()).Any()).Any())
                .WhereIF(creatorTimeRange != null, it => SqlFunc.Between(it.CreatorTime, startCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
                .WhereIF(cgIds.IsAny(), it => cgIds.Contains(it.Id))
                .WhereIF(input.hasNum == true, it => (it.InType == "1" || it.InType == "5") && SqlFunc.Subqueryable<ErpInrecordEntity>().Where(ddd => ddd.InId == it.Id && ddd.InNum > 0).Any())
                .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
                .Select(it => new ErpInorderListOutput
                {
                    id = it.Id
                })
                .OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort);

            if (input.dataType == 0)
            {
               var tempData = await tempQuery.ToPagedListAsync(input.currentPage, input.pageSize);

                if (tempData.list.IsAny())
                {
                    ids = tempData.list.Select(it => it.id).ToList();
                }
            }
            else
            {
                var tempData = await tempQuery.Take(1000).ToListAsync();
                ids = tempData.Select(x => x.id).ToList();
            }

            if(!ids.IsAny())
            {
                ids.Add("-1");
            }
        }


            var query = _repository.Context.Queryable<ErpInorderEntity>()
               .InnerJoin<ErpInrecordEntity>((it, w) => it.Id == w.InId)
              .InnerJoin<ErpProductmodelEntity>((it, w, a) => w.Gid == a.Id)
              .InnerJoin<ErpProductEntity>((it, w, a, b) => a.Pid == b.Id)
              .WhereIF(ids.Any(), (it, w, a, b) => ids.Contains(it.Id))
              .Select((it, w, a, b) => new ErpInrecordInfoExportDetailOutput
              {
                  gidName = a.Name,
                  productName = b.Name,
                  amount = w.Amount,
                  inNum = w.InNum,
                  price = w.Price,
                  remark = w.Remark,
                  storeRomeAreaIdName = SqlFunc.Subqueryable<ErpStoreareaEntity>().Where(zzz => zzz.Id == w.StoreRomeAreaId).Select(zzz => zzz.Name),
                  storeRomeIdName = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(zzz => zzz.Id == w.StoreRomeId).Select(zzz => zzz.Name),
                  orderNo = "",
                  cidName = "",
                  id = w.Id,
                  oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(zzz => zzz.Id == w.Oid).Select(zzz => zzz.FullName),
                  no = it.No
              });

      

        //var data = input.dataType == 0 ? await query.ToPageListAsync(input.currentPage, input.pageSize) : await query.ToListAsync();
        var data = await query.ToListAsync();

        var list = await _repository.Context.Queryable<ErpOutdetailRecordEntity>()
                 .InnerJoin<ErpOrderdetailEntity>((a, b) => a.OutId == b.Id)
                 .InnerJoin<ErpOrderEntity>((a, b, c) => b.Fid == c.Id)
                 .InnerJoin<ErpInrecordEntity>((a, b, c, d) => a.InId == d.Id)
                 .LeftJoin<ErpCustomerEntity>((a, b, c,d,  e) => c.Cid == e.Id)
                 .Where((a, b, c, d,e) => ids.Contains(d.InId))
                 .Select((a, b, c, d, e) => new
                 {
                     no = c.No,
                     id = a.InId,
                     cidName = e.Name
                 }).ToListAsync();

        foreach (var item in data)
        {
            var q = list.Where(it => it.id == item.id);
            var xlist = q.Select(it => it.no).Distinct().ToArray();
            if (xlist.IsAny())
            {
                item.orderNo = string.Join("，", xlist);
            }
            item.cidName = string.Join("，", q.Select(it => it.cidName).Distinct().ToArray());
        }


        ExcelConfig excelconfig = ExcelConfig.Default($"入库明细_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xls");
        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);

        excelconfig.ColumnModel = Common.Extension.Extensions.GetExcelColumnModels<ErpInrecordInfoExportDetailOutput>();

        var fs = ExcelExportHelper<ErpInrecordInfoExportDetailOutput>.ExportMemoryStream(data, excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }
         

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT" + "|" + _userManager.TenantId) };
    }

    /// <summary>
    /// 导出Excel.
    /// 有固定的模板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("TS/MExportExcel")]
    public async Task<dynamic> TSMExportExcel([FromQuery] ErpInorderListQueryInput input, [FromServices] IFileManager fileManager)
    {
        List<ErpInorderListTSExportOutput> data = new List<ErpInorderListTSExportOutput>();
        List<string> ids = new List<string>();

        var rklxOptioins = await _dictionaryDataService.GetList("QTErpRklx");
        var jldwOptioins = await _dictionaryDataService.GetList("JLDW");

        if (input.dataType == 2)
        {
            ids = input.items.Split(",", true).ToList();
        }
        else
        {
            if (input.dataType == 1)
            {
                input.pageSize = 10000;
            }           
        }
        var list = await GetList(input);

        data = list.list.Adapt<List<ErpInorderListTSExportOutput>>();

        foreach (var item in data)
        {
            item.inType = rklxOptioins.Find(x => x.EnCode == item.inType)?.FullName;
            if (item.specialState == "0" || item.specialState.IsNullOrEmpty())
            {
                item.specialState = "未完成";
            }
            if (item.specialState == "1")
            {
                item.specialState = "已完成";
            }
            item.gidUnit = jldwOptioins.Find(x => x.EnCode == item.gidUnit)?.FullName;
        }

  

        ExcelConfig excelconfig = ExcelConfig.Default($"特殊入库订单_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xls");
        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);

        excelconfig.ColumnModel = Common.Extension.Extensions.GetExcelColumnModels<ErpInorderListTSExportOutput>();

        var fs = ExcelExportHelper<ErpInorderListTSExportOutput>.ExportMemoryStream(data, excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }


        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT" + "|" + _userManager.TenantId) };
    }

    /// <summary>
    /// 获取库存记录
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpGet("Store/SumByGid/ExportExcel")]
    public async Task<dynamic> StoreSumByGidExportExcel([FromQuery] ErpStorePageListQueryInput input, [FromServices] IFileManager fileManager)
    {
        var list = await GetStoreSumList(input)  ?? new List<ErpStoreInfoOutput>();

        // 单位转换
        var unitOptioins = await _dictionaryDataService.GetList("JLDW");
        if (list.IsAny())
        {
            foreach (var item in list)
            {
                if (item.gidUnit.IsNotEmptyOrNull())
                {
                    var unit = unitOptioins.Find(x => x.EnCode == item.gidUnit);
                    if (unit != null)
                    {
                        item.gidUnit = unit.FullName;
                    }
                }

            }
        }

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_{1}.xls", DateTime.Now, "库存汇总"));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpStoreInfoOutput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpStoreInfoOutput>.ExportMemoryStream(list.Adapt<List<ErpStoreInfoOutput>>(), excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }


        return new { name = excelconfig.FileName, url = flag.Item2 };

    }

    // <summary>
    /// 获取库存记录
    /// </summary>
    /// <param name="cgNo"></param>
    /// <returns>ErpInrecordInfoOutput</returns>
    [HttpGet("Store/SumByGid/His/ExportExcel")]
    public async Task<dynamic> StoreSumByGidHisExportExcel([FromQuery] ErpStorePageListQueryInput input, [FromServices] IFileManager fileManager)
    {
        // 找出最后的时间戳
        var lastTime = await _repository.Context.Queryable<ErpStoreHistoryEntity>()
            .WhereIF(input.cutDate.HasValue, (x) => SqlFunc.Between(x.cutDate, input.cutDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), input.cutDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .OrderByDescending(x => x.cutDate)
            .Select(x => x.cutDate)
            .FirstAsync();

        var query = _repository.Context.Queryable<ErpStoreHistoryEntity>();

        //if (!string.IsNullOrEmpty(input.oid))
        //{
        //    query = query.ClearFilter<ICompanyEntity>().Where((x, w, a, b) => x.Oid == input.oid);
        //}

        Expression<Func<ErpStoreHistoryEntity, bool>> where = null;
        if (!string.IsNullOrEmpty(input.gid))
        {
            where = (x) => x.gid == input.gid;
            if (input.whetherRelation)
            {
                var rid = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it => it.Id == input.gid).Select(it => it.Rid).FirstAsync();
                if (!string.IsNullOrEmpty(rid))
                {
                    where = (x) => x.gid == input.gid || x.gid == rid;
                }
            }
        }

        List<DateTime> inttimeRange = input.intimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = inttimeRange?.First();
        DateTime? endDate = inttimeRange?.Last();

        var list = await query.WhereIF(where != null, where)
            .WhereIF(lastTime.HasValue, x => x.cutDate == lastTime)
            .WhereIF(!string.IsNullOrEmpty(input.tid), (x) => x.tid == input.tid)
            .WhereIF(!string.IsNullOrEmpty(input.oid), (x) => x.oid == input.oid)
            .WhereIF(input.rootTypeId.IsNotEmptyOrNull(), (x) => SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == x.tid && ddd.RootId == input.rootTypeId).Any())
            .WhereIF(input.cutDate.HasValue, (x) => SqlFunc.Between(x.cutDate, input.cutDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), input.cutDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), (x) => x.gidName.Contains(input.keyword) || x.productName.Contains(input.keyword))
            .WhereIF(inttimeRange != null, (x) => SqlFunc.Between(x.creatorTime, startDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
           .OrderByDescending((x) => new { x.oid, x.gid, x.creatorTime })
           .Select<ErpStoreHistoryInfoOutput>(x => new ErpStoreHistoryInfoOutput
           {
               rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == x.tid).Select(ddd => ddd.RootName)
           }, true)
           .ToListAsync();

        // 单位转换
        var unitOptioins = await _dictionaryDataService.GetList("JLDW");
        if (list.IsAny())
        {
            foreach (var item in list)
            {
                if (item.gidUnit.IsNotEmptyOrNull())
                {
                    var unit = unitOptioins.Find(x => x.EnCode == item.gidUnit);
                    if (unit != null)
                    {
                        item.gidUnit = unit.FullName;
                    }
                }

            }
        }

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy_MM_dd_HH_mm_ss}_{1}.xls", DateTime.Now, "库存历史"));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpStoreHistoryInfoOutput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpStoreHistoryInfoOutput>.ExportMemoryStream(list, excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }


        return new { name = excelconfig.FileName, url = flag.Item2 };

    }

    ///// <summary>
    ///// 拆包入库 生成拆包入库单和拆包出库单
    ///// </summary>
    ///// <returns></returns>
    //[HttpPost("Unpacking")]
    //public Task Unpacking()
    //{

    //}


    [HttpPost("ext/auditTime")]
    public async Task UpdateExtAuditTime([FromBody]List<ErpInorderExtUpAuditTime> input)
    {
        if (input.IsAny())
        {
            var list = await _repository.Context.Queryable<ErpInrecordExtEntity>().Where(x => input.Any(a => a.id == x.Id)).ToListAsync();

            List<ErpInrecordExtEntity> insertList = new List<ErpInrecordExtEntity>();
            foreach (var item in input)
            {
                var entity = list.FirstOrDefault(x => x.Id == item.id);
                if (entity!=null)
                {
                    //更新
                    _repository.Context.Tracking(entity);
                    entity.AuditTime = item.auditTime;
                    entity.LastModifyTime = DateTime.Now;
                    entity.LastModifyUserId = _userManager.UserId;
                }
                else
                {
                    insertList.Add(new ErpInrecordExtEntity
                    {
                        Id = item.id,
                        AuditTime = item.auditTime,
                        CreatorTime = DateTime.Now,
                        CreatorUserId = _userManager.UserId
                    });
                }
            }
            if (list.IsAny())
            {
                await _repository.Context.Updateable<ErpInrecordExtEntity>(list).ExecuteCommandAsync();
            }
            if (insertList.IsAny())
            {
                await _repository.Context.Insertable(insertList).ExecuteCommandAsync();
            }
        }
    }


    [HttpPost("ext/expenseTime")]
    public async Task UpdateExtAuditTime([FromBody] List<ErpInorderExtUpExpenseTime> input)
    {
        if (input.IsAny())
        {
            var list = await _repository.Context.Queryable<ErpInrecordExtEntity>().Where(x => input.Any(a => a.id == x.Id)).ToListAsync();

            List<ErpInrecordExtEntity> insertList = new List<ErpInrecordExtEntity>();
            foreach (var item in input)
            {
                var entity = list.FirstOrDefault(x => x.Id == item.id);
                if (entity != null)
                {
                    //更新
                    _repository.Context.Tracking(entity);
                    entity.ExpenseTime = item.expenseTime;
                    entity.LastModifyTime = DateTime.Now;
                    entity.LastModifyUserId = _userManager.UserId;

                    _repository.Context.Updateable<ErpInrecordExtEntity>(entity).AddQueue();
                }
                else
                {
                    var newItem = new ErpInrecordExtEntity
                    {
                        Id = item.id,
                        ExpenseTime = item.expenseTime,
                        CreatorTime = DateTime.Now,
                        CreatorUserId = _userManager.UserId
                    };
                    insertList.Add(newItem);


                    _repository.Context.Insertable<ErpInrecordExtEntity>(newItem).AddQueue();
                }
            }
            //if (list.IsAny())
            //{
            //    await _repository.Context.Updateable<ErpInrecordExtEntity>(list).ExecuteCommandAsync();
            //}
            //if (insertList.IsAny())
            //{
            //    await _repository.Context.Insertable<ErpInrecordExtEntity>(insertList).ExecuteCommandAsync();
            //}

            await _repository.Context.SaveQueuesAsync();
        }
    }

    [HttpPost("UpdateStore")]
    [SqlSugarUnitOfWork]
    public async Task UpdateStoreInfo([FromBody] UpdateStoreInfoInput input, [FromServices] IErpBuyorderService erpBuyorderService)
    {
        var entity = await _repository.Context.Queryable<ErpInrecordEntity>().SingleAsync(x => x.Id == input.id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.Bid.IsNotEmptyOrNull())
        {
            await erpBuyorderService.UpdateOrderDetailByBuyorderdetailId(entity.Bid, new UpdateStoreInfoInput
            {
                id = entity.Bid,
                productionDate = input.productionDate,
                retention = input.retention
            });
        }
        else
        {
            _repository.Context.Tracking(entity);
            entity.ProductionDate = input.productionDate;
            entity.Retention = input.retention;
            await _repository.Context.Updateable<ErpInrecordEntity>(entity).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 获取未分拣订单汇总数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("ts/Sum")]
    public async Task<dynamic> GetTsSumList_API([FromQuery] ErpInorderListQueryInput input)
    {
        var list = await GetTsSumList(input);

        SqlSugarPagedList<ErpInorderSumListExportOutput> data = new SqlSugarPagedList<ErpInorderSumListExportOutput>
        {
            list = list,
            pagination = new PagedModel
            {
                PageIndex = 1,
                PageSize = list.Count,
                Total = list.Count
            }
        };
        return PageResult<ErpInorderSumListExportOutput>.SqlSugarPageResult(data);
    }

    private async Task<List<ErpInorderSumListExportOutput>> GetTsSumList(ErpInorderListQueryInput input)
    {
        List<DateTime> creatorTimeRange = input.creatorTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreatorTimeDate = creatorTimeRange?.First();
        DateTime? endCreatorTimeDate = creatorTimeRange?.Last();

        var q1 = _repository.Context.Queryable<ErpInrecordTsEntity>()
            .GroupBy(x => x.TsId)
            .Select(x => new
            {
                x.TsId,
                Num = SqlFunc.AggregateSum(x.Num)
            }).MergeTable();
        var list = await _repository.Context.Queryable<ErpInorderEntity>()
            .InnerJoin<ErpInrecordEntity>((a, b) => a.Id == b.InId)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Gid == c.Id)
            .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
            .LeftJoin(q1, (a, b, c, d, e) => b.Id == e.TsId)
            //.Where(a => a.State == Extend.Entitys.Enums.OrderStateEnum.PendingApproval)
            .WhereIF(!string.IsNullOrEmpty(input.no), a => a.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.cgNo), a => a.CgNo.Contains(input.cgNo))
            .WhereIF(!string.IsNullOrEmpty(input.inType), a => a.InType.Equals(input.inType))
            .WhereIF(input.specialState == "0", a => (a.SpecialState ?? "") == "")
            .WhereIF(input.specialState == "1", a => a.SpecialState == "1")
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a =>
                a.No.Contains(input.keyword)
                || a.InType.Contains(input.keyword)
                )
            .WhereIF(input.productName.IsNotEmptyOrNull(), a => SqlFunc.Subqueryable<ErpInrecordEntity>().Where(d1 => d1.InId == a.Id
                    && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2 => d2.Id == d1.Gid
                    && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3 => d3.Id == d2.Pid && d3.Name.Contains(input.productName)).Any()).Any()).Any())
            .WhereIF(creatorTimeRange != null, a => SqlFunc.Between(a.CreatorTime, startCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .GroupBy((a, b, c, d) => new { b.Gid, c.Pid, c.Name, pName = d.Name })
            .Select((a, b, c, d,e) => new ErpInorderSumListExportOutput
            {
                mid = b.Gid,
                pid = c.Pid,
                name = c.Name,
                productName = d.Name,
                unit = SqlFunc.AggregateMax(c.Unit),
                num = SqlFunc.AggregateSum(b.InNum),
                tsnum = SqlFunc.AggregateSum(e.Num),
            })
            .ToListAsync();

        // 单位转换
        var unitOptioins = await _dictionaryDataService.GetList("JLDW"); 

        foreach (var item in list)
        {
            item.untsnum = item.num - item.tsnum;

            if (item.unit.IsNotEmptyOrNull())
            {
                var unit = unitOptioins.Find(x => x.EnCode == item.unit);
                if (unit != null)
                {
                    item.unit = unit.FullName;
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 获取未分拣订单汇总数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("ts/Sum/ExportExcel")]
    public async Task<dynamic> GetTsSumListExportExcel([FromQuery] ErpInorderListQueryInput input, [FromServices]IFileManager fileManager)
    {
        var list = await GetTsSumList(input);

        var title = string.Format("{0:yyyyMMddHHmmss}_特殊入库汇总.xls", DateTime.Now);
        ExcelConfig excelconfig = ExcelConfig.Default(title);
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpInorderSumListExportOutput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpInorderSumListExportOutput>.ExportMemoryStream(list, excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    //private async Task UpdateOrderDetailByInrecordId(List<ErpInrecordEntity> inrecordList)
    //{
    //    if (inrecordList.IsAny())
    //    {
    //        var relations = await _repository.Context.Queryable<ErpOutdetailRecordEntity>()
    //             .Where(it => inrecordList.Any(x => x.Id == it.InId))
    //             .ToListAsync();

    //        // 关联订单
    //        var orderdetails = await _repository.Context.Queryable<ErpOrderdetailEntity>()
    //           .Where(it => relations.Any(x => x.OutId == it.Id))
    //           .Select(it => new ErpOrderdetailEntity
    //           {
    //               Id = it.Id,
    //               Bid = it.Bid
    //           })
    //           .ToListAsync();


    //        foreach (var inrecord in inrecordList)
    //        {
    //            // 入库信息
    //            inrecord.ProductionDate = item.ProductionDate;
    //            inrecord.Retention = item.Retention;
    //            await _repository.Context.Updateable<ErpInrecordEntity>(inrecord).UpdateColumns(new string[] { nameof(ErpInrecordEntity.ProductionDate), nameof(ErpInrecordEntity.Retention) }).ExecuteCommandAsync();

    //            // 订单信息
    //            foreach (var order in orderdetails.Where(x => relations.Any(it => it.OutId == x.Id)).ToList())
    //            {
    //                order.Bid = item.Id;
    //                await _repository.Context.Updateable<ErpOrderdetailEntity>(order).UpdateColumns(new string[] { nameof(ErpOrderdetailEntity.Bid) }).ExecuteCommandAsync();
    //
    //
    //      }
    //        }
    //    }
    //}

    /// <summary>
    /// 更新入库明细的单价金额
    /// </summary>
    /// <returns></returns>
    [HttpPut("actions/detail/{id}/amount")]
    [OperateLog("入库订单", "更新单价金额")]
    [SqlSugarUnitOfWork]
    public async Task UpdateDetailAmount(string id, [FromBody] ErpBuyorderdetailUpAmountInput input)
    {
        //var detail = await _repository.Context.Queryable<ErpInrecordEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        //_repository.Context.Tracking(detail);
        //detail.Price = input.price;
        //detail.Amount =  detail.InNum * input.price; // input.amount;
        //await _repository.Context.Updateable(detail).ExecuteCommandAsync();
        //更新采购单的单价和金额，采购单总金额
        //更新入库记录的单价和金额
        //更新特殊入库的单价和金额
        //更新出库成本

        ErpInrecordEntity detail = new ErpInrecordEntity
        {
            Id = id,
            Price = input.price,
            Amount = input.amount
        };

        //1、入库记录(采购入库和特殊入库)
        //var indetail = await _repository.Context.Queryable<ErpInrecordEntity>().SingleAsync(x => x.Id == detail.Id);
        var indetailList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => x.Id == detail.Id || SqlFunc.Subqueryable<ErpInrecordTsEntity>().Where(t => t.TsId == x.Id && t.InId == detail.Id).Any()).ToListAsync();

        if (indetailList.IsAny())
        {
            decimal total = 0;
            foreach (var item in indetailList)
            {
                _repository.Context.Tracking(item);
                item.Price = detail.Price;
                item.Amount = item.InNum * detail.Price;

                total += item.Amount;
            }
            // 把剩余的金额放到最后一条（入库数量不为0的）
            if (total != detail.Amount)
            {
                var last = indetailList.LastOrDefault(x => x.InNum > 0);
                if (last != null)
                {
                    last.Amount += detail.Amount - total;
                }
            }

            // 更新数据，获取主表记录
            await _repository.Context.Updateable<ErpInrecordEntity>(indetailList).ExecuteCommandAsync();

            var inmainList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => indetailList.Any(t => t.InId == x.InId))
                 .GroupBy(x => x.InId)
                 .Select(x => new ErpInorderEntity
                 {
                     Id = x.InId,
                     Amount = SqlFunc.AggregateSum(x.Amount)
                 })
                 .ToListAsync();
            foreach (var item in inmainList)
            {
                await _repository.Context.Updateable<ErpInorderEntity>(item).UpdateColumns(x => new { x.Amount }).ExecuteCommandAsync();
            }

        }

        //出库关系记录
        var outdetailRecordList = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(x => indetailList.Any(t => t.Id == x.InId)).ToListAsync();

        //出库记录
        var outdetailList = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(x => outdetailRecordList.Any(t => t.OutId == x.Id)).ToListAsync();

        if (outdetailList.IsAny())
        {
            foreach (var item in outdetailList)
            {
                decimal amount = 0;
                foreach (var record in outdetailRecordList.Where(x => x.OutId == item.Id))
                {
                    var indetail = indetailList.FirstOrDefault(x => x.Id == record.InId);
                    if (indetail != null)
                    {
                        amount += indetail.InNum == record.Num ? indetail.Amount : record.Num * indetail.Price;
                    }
                }
                _repository.Context.Tracking(item);
                item.CostAmount = amount;
            }

            await _repository.Context.Updateable<ErpOutrecordEntity>(outdetailList).ExecuteCommandAsync();
        }
    }

    #region 质检报告
    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("DetailProof/{id}")]
    public async Task<ErpBuyorderCkUpProofInput> GetDetailProof(string id)
    {
        var entity = await _repository.Context.Queryable<ErpInrecordEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        return entity.Adapt<ErpBuyorderCkUpProofInput>();
    }

    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("DetailProof/{id}")]
    [SqlSugarUnitOfWork]
    public async Task DetailProof(string id, [FromBody] ErpBuyorderCkUpProofInput input)
    {
        var entity = await _repository.Context.Queryable<ErpInrecordEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        var newEntity = input.Adapt<ErpInrecordEntity>();

        entity.QualityReportProof = newEntity.QualityReportProof;

        await _repository.Context.Updateable<ErpInrecordEntity>(entity).UpdateColumns(x => new { x.QualityReportProof }).ExecuteCommandAsync();

        if (entity.Bid.IsNotEmptyOrNull())
        {
            ErpBuyorderdetailEntity erpBuyorderdetailEntity = new ErpBuyorderdetailEntity
            {
                Id = entity.Bid,
                QualityReportProof = entity.QualityReportProof,
            };
            await _repository.Context.Updateable<ErpBuyorderdetailEntity>(erpBuyorderdetailEntity).UpdateColumns(x => new { x.QualityReportProof }).ExecuteCommandAsync();
        }
    }
    #endregion


    /// <summary>
    /// 获取入库记录流水
    /// </summary>
    /// <returns></returns>
    [HttpGet("actions/inout/query")]
    public async Task<PageResult<ErpInoutDetailQueryOutput>> InoutDetail( [FromQuery] ErpInoutDetailQueryInput input)
    {
        var list = await _repository.Context.Queryable<ErpInorderEntity>()
             .InnerJoin<ErpInrecordEntity>((a, b) => a.Id == b.InId)
             .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Gid == c.Id)
             .InnerJoin<ErpProductEntity>((a, b, c, d) => d.Id == c.Pid)
             .WhereIF(input.no.IsNotEmptyOrNull(), (a, b) => a.No.Contains(input.no) || a.CgNo.Contains(input.no))
             .WhereIF(input.productName.IsNotEmptyOrNull(), (a, b,c, d) => d.Name.Contains(input.productName))
             .OrderByDescending((a,b,c,d)=> a.CreatorTime)
             .Select((a, b, c, d) => new ErpInoutDetailQueryOutput
             {
                 id = b.Id,
                 amount = b.Amount,
                 date = a.CreatorTime,
                 gidName = c.Name,
                 inNum = b.InNum,
                 no = a.No,
                 productName = d.Name,
                 price = b.Price,
             })
             .ToPagedListAsync(input.currentPage, input.pageSize);

        // 找出出库记录

        if (list.list.Any())
        {
            var ids = list.list.Select(x => x.id);

           var list2 = await _repository.Context.Queryable<ErpOutdetailRecordEntity>()
                .InnerJoin<ErpOutrecordEntity>((a, b) => a.OutId == b.Id)
                .InnerJoin<ErpOutorderEntity>((a, b, c) => b.OutId == c.Id)
                .Where((a, b, c) => ids.Contains(a.InId))
                //.OrderBy((a,b,c)=>a.CreatorTime)
                .Select((a, b, c) => new ErpInoutDetailQueryOutput
                {
                    id = a.Id,
                    amount = b.CostAmount,
                    date = a.CreatorTime,
                    //gidName = c.Name,
                    //inNum = b.InNum,
                    no = c.No,
                    //productName = d.Name,
                    //price = b.Price,
                    outNum = a.Num,
                    inid = a.InId
                })
                .ToListAsync();

            foreach (var item in list.list)
            {
                decimal inNum = item.inNum ?? 0;

                item.children = list2.Where(x => x.inid == item.id).OrderBy(x=>x.date).ToList();

                if (item.children.IsAny())
                {
                    foreach (var xitem in item.children)
                    {
                        inNum -= xitem.outNum ?? 0;
                        xitem.num = inNum;
                    }
                }                
            }
        }

        return PageResult<ErpInoutDetailQueryOutput>.SqlSugarPagedList(list);
    }
}
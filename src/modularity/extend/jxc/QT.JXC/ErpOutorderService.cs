using Mapster;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Interfaces;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：出库订单表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOutorder", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpOutorderService : IErpOutorderService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpOutorderEntity> _repository;

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

    /// <summary>
    /// 初始化一个<see cref="ErpOutorderService"/>类型的新实例.
    /// </summary>
    public ErpOutorderService(
        ISqlSugarRepository<ErpOutorderEntity> erpOutorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IErpStoreService erpStoreService)
    {
        _repository = erpOutorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _erpStoreService = erpStoreService;
    }

    /// <summary>
    /// 获取出库订单表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpOutorderInfoOutput>();

        var erpOutrecordList = await _repository.Context.Queryable<ErpOutrecordEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
            .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
            .Where(w => w.OutId == output.id)
            .Select((w, a, b) => new ErpOutrecordInfoOutput
            {
                gidName = a.Name,
                productName = b.Name
            }, true).ToListAsync();

        // 关联入库记录
        var list = await _repository.Context.Queryable<ErpInrecordEntity>()
            .InnerJoin<ErpOutdetailRecordEntity>((a, b) => a.Id == b.InId)
            .LeftJoin<ErpBuyorderdetailEntity>((a, b, c) => a.Bid == c.Id)
            .Where((a, b) => erpOutrecordList.Any(x => x.id == b.OutId))
            .Select((a, b, c) => new ErpStoreInfoOutput
            {
                id = a.Id,
                price = a.Price,
                amount = SqlFunc.Round(a.Price * b.Num, 2),
                supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == c.Supplier).Select(ddd => ddd.Name),
                 billId = b.OutId,
                 num = b.Num,
                 no = SqlFunc.Subqueryable<ErpInorderEntity>().Where(ddd=>ddd.Id == a.InId).Select(ddd=>ddd.No)
            })
            .ToListAsync();

        foreach (var item in erpOutrecordList)
        {
            item.storeDetailList = list.Where(x => x.billId == item.id).ToList();
        }



        output.erpOutrecordList = erpOutrecordList; // erpOutrecordList.Adapt<List<ErpOutrecordInfoOutput>>();
        return output;
    }

    /// <summary>
    /// 获取出库订单表列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpOutorderListQueryInput input)
    {
        List<DateTime> createTimeRange = input.createTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPosttimeDate = createTimeRange?.First();
        DateTime? endPosttimeDate = createTimeRange?.Last();

        List<string> rkList = new List<string>();
        if (input.rkNo.IsNotEmptyOrNull())
        {
            rkList = await _repository.Context.Queryable<ErpInrecordEntity>()
                .InnerJoin<ErpOutdetailRecordEntity>((a, b) => a.Id == b.InId)
                .LeftJoin<ErpBuyorderdetailEntity>((a, b, c) => a.Bid == c.Id)
                .Where((a, b) => SqlFunc.Subqueryable<ErpInorderEntity>().Where(ddd => ddd.Id == a.InId && ddd.No == input.rkNo).Any())
                .Select((a, b, c) =>  b.OutId)
                .ToListAsync();

            if (!rkList.IsAny())
            {
                rkList.Add(input.rkNo);
            }
        }
        List<string> typeIds = new List<string>();
        if (input.rootTypeId.IsNotEmptyOrNull())
        {
            typeIds = await _repository.Context.Queryable<ViewErpProducttypeEx>().Where(x => x.RootId == input.rootTypeId).Select(x => x.Id).ToListAsync();
        }

        var data = await _repository.Context.Queryable<ErpOutorderEntity>()
            .WhereIF(input.beginDate.HasValue, it => it.CreatorTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreatorTime <= input.endDate)
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
            .WhereIF(!string.IsNullOrEmpty(input.inOidName), it => SqlFunc.Subqueryable<OrganizeEntity>().Where(zzz=>zzz.Id == it.InOid && zzz.FullName.Contains(input.inOidName)).Any())
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.InType.Contains(input.keyword)
                )
            .WhereIF(rkList.IsAny(), it=> SqlFunc.Subqueryable<ErpOutrecordEntity>().Where(d1 => d1.OutId == it.Id && rkList.Contains(d1.Id)).Any())
            .WhereIF(createTimeRange != null, it => SqlFunc.Between(it.CreatorTime, startPosttimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(input.productName.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpOutrecordEntity>().Where(d1 => d1.OutId == it.Id
                    && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2 => d2.Id == d1.Gid
                    && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3 => d3.Id == d2.Pid && d3.Name.Contains(input.productName)).Any()).Any()).Any())
            .WhereIF(typeIds.IsAny(), it => SqlFunc.Subqueryable<ErpOutrecordEntity>().Where(d1 => d1.OutId == it.Id
                    && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2 => d2.Id == d1.Gid
                    && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3 => d3.Id == d2.Pid && typeIds.Contains(d3.Tid)).Any()).Any()).Any())
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpOutorderListOutput
            {
                id = it.Id,
                oid = it.Oid,
                no = it.No,
                inType = it.InType,
                amount = it.Amount,
                xsNo = it.XsNo,
                creatorTime = it.CreatorTime,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
                inOidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.InOid).Select(d => d.FullName),
                creator = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.CreatorUserId).Select(d => d.RealName),
                state = it.State,
                costAmount = SqlFunc.Subqueryable<ErpOutrecordEntity>().Where(ddd=>ddd.OutId == it.Id).Sum(ddd=>ddd.CostAmount)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        // 退货出库
        if (input.inType == "5")
        {
            var idList = data.list.Select(x => x.id).ToList();
            var erpOutrecordList = await _repository.Context.Queryable<ErpOutrecordEntity>()
           .Where(w => idList.Contains( w.OutId))
           .Select((w) => new ErpOutrecordEntity
           {
               Id = w.Id,
               OutId = w.OutId
           }).ToListAsync();

            // 关联入库记录
            var list = await _repository.Context.Queryable<ErpInrecordEntity>()
                .InnerJoin<ErpOutdetailRecordEntity>((a, b) => a.Id == b.InId)
                .LeftJoin<ErpBuyorderdetailEntity>((a, b, c) => a.Bid == c.Id)
                .Where((a, b) => erpOutrecordList.Any(x => x.Id == b.OutId))
                .Select((a, b, c) => new ErpStoreInfoOutput
                {
                    id = a.Id,
                    price = a.Price,
                    amount = SqlFunc.Round(a.Price * b.Num, 2),
                    supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == c.Supplier).Select(ddd => ddd.Name),
                    billId = b.OutId,
                    num = b.Num,
                    no = SqlFunc.Subqueryable<ErpInorderEntity>().Where(ddd=>ddd.Id == a.InId).Select(ddd=>ddd.No)
                })
                .ToListAsync();

            foreach (var item in data.list)
            {
                item.supplierName = string.Join(",", list.Where(x => erpOutrecordList.Where(d => d.OutId == item.id && d.Id == x.billId).Any()).Select(x => x.supplierName).Distinct());
                item.inNo = string.Join(",", list.Where(x => erpOutrecordList.Where(d => d.OutId == item.id && d.Id == x.billId).Any()).Select(x => x.no).Distinct());
            }
        }
        return PageResult<ErpOutorderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取销售出库订单表列表.
    /// 按销售订单编号分组
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Xs")]
    public async Task<dynamic> GetListXs([FromQuery] ErpOutorderXsListQueryInput input)
    {
        input.inType = "1";
        var query = _repository.Context.Queryable<ErpOutorderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.xsNo), it => it.XsNo.Contains(input.xsNo))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
            .WhereIF(input.productName.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpOutrecordEntity>().Where(d1 => d1.OutId == it.Id
                    && SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(d2 => d2.Id == d1.Gid
                    && SqlFunc.Subqueryable<ErpProductEntity>().Where(d3 => d3.Id == d2.Pid && d3.Name.Contains(input.productName)).Any()).Any()).Any())
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.InType.Contains(input.keyword)
                );
        var data = await query.Clone().GroupBy(it => it.XsNo).Select(it => new ErpOutorderXsListOutput
        {
            id = it.XsNo,
            xsNo = it.XsNo
        }).ToPagedListAsync(input.currentPage, input.pageSize);

        if (data.list.IsAny())
        {
            var pageList = data.list.Select(x => x.xsNo).ToList();
            var list = await query.Where(it => pageList.Contains(it.XsNo))
                .Select(it => new ErpOutorderListOutput
                {
                    id = it.Id,
                    oid = it.Oid,
                    no = it.No,
                    inType = it.InType,
                    amount = it.Amount,
                    xsNo = it.XsNo,
                    creatorTime = it.CreatorTime,
                    oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
                }).ToListAsync();

            if (list.IsAny())
            {
                var idList = list.Select(x => x.id).ToArray();
                var details = await _repository.Context.Queryable<ErpOutrecordEntity>()
                    .InnerJoin<ErpProductmodelEntity>((it, b) => it.Gid == b.Id)
                    .InnerJoin<ErpProductEntity>((it, b, c) => b.Pid == c.Id)
                    .Where(it => idList.Contains(it.OutId))
                    .Select((it, b, c) => new
                    {
                        id = it.OutId,
                        name = b.Name,
                        num = it.Num,
                        productName = c.Name
                    })
                    .ToListAsync();

                foreach (var item in list)
                {
                    var array = details.Where(x => x.id == item.id && !string.IsNullOrEmpty(x.productName))
                        .Select(x => $"{x.productName}({x.name}):{float.Parse(x.num.ToString())}").ToArray();
                    item.items = string.Join(";", array);
                }
            }

            foreach (var item in data.list)
            {
                item.children = list.Where(x => x.xsNo == item.xsNo).ToList();

                item.amount = item.children.Sum(x => x.amount);
            }
        }


        return PageResult<ErpOutorderXsListOutput>.SqlSugarPageResult(data);
        /*
        var data = await _repository.Context.Queryable<ErpOutorderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.InType.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpOutorderListOutput
            {
                id = it.Id,
                oid = it.Oid,
                no = it.No,
                inType = it.InType,
                amount = it.Amount,
                xsNo = it.XsNo,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        
        return PageResult<ErpOutorderListOutput>.SqlSugarPageResult(data);
        */
    }


    #region 调出、调入
    ///// <summary>
    ///// 获取出库订单表列表.
    ///// </summary>
    ///// <param name="input">请求参数.</param>
    ///// <returns></returns>
    //[HttpGet("Db")]
    //public async Task<dynamic> GetDbList([FromQuery] ErpOutorderListQueryInput input)
    //{
    //    if (_repository.Context.QueryFilter.GeFilterList.IsAny())
    //    {
    //        _repository.Context.QueryFilter.Clear<ICompanyEntity>();
    //    }        
    //    return await GetList(input);
    //}
    /// <summary>
    /// 获取调拨出库入库审核列表.
    /// 获取当前登录用户所属公司的 待调入记录
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Db/InAudit")]
    public async Task<dynamic> GetInAuditList([FromQuery] ErpOutorderListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpOutorderEntity>()
            .ClearFilter<ICompanyEntity>()
            .Where(it => it.State == "1" && it.InOid == _userManager.CompanyId)
            .WhereIF(input.beginDate.HasValue, it => it.CreatorTime >= input.beginDate)
            .WhereIF(input.endDate.HasValue, it => it.CreatorTime <= input.endDate)
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.InType.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpOutorderListOutput
            {
                id = it.Id,
                oid = it.Oid,
                no = it.No,
                inType = it.InType,
                amount = it.Amount,
                xsNo = it.XsNo,
                creatorTime = it.CreatorTime,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
                inOidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.InOid).Select(d => d.FullName),
                state = it.State
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpOutorderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取出库订单表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Db/InAudit/{id}")]
    public async Task<dynamic> GetDbInfo(string id)
    {
        if (_repository.Context.QueryFilter.GeFilterList.IsAny())
        {
            _repository.Context.QueryFilter.Clear<ICompanyEntity>();
        }
        if (! await _repository.AnyAsync(it => it.Id == id))
        {
            return null;
        }
        var dto = await GetInfo(id) as ErpOutorderInfoOutput;

        ErpOutorderDbAuditInfoOutput output = dto.Adapt<ErpOutorderDbAuditInfoOutput>();
        if (output != null && output.erpOutrecordList.IsAny())
        {
            var list = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                .InnerJoin<ErpInrecordEntity>((w,a)=>w.TsId == a.Id)
                .Where(w => output.erpOutrecordList.Any(x => x.id == w.InId))
                .Select((w,a)=> new
                {
                    w.InId,
                    w.Num,
                    a.Amount
                })
                .ToListAsync();
            foreach (var item in output.erpOutrecordList)
            {
                item.outNum = item.num;
                item.tsNum = list.Where(x => x.InId == item.id).Sum(x => x.Num);
                item.tsAmount = list.Where(x => x.InId == item.id).Sum(x => x.Amount);
                //if (item.tsNum>0)
                //{
                //    item.num = item.outNum - item.tsNum;
                //    item.amount = Math.Round(item.num * item.price, 2);
                //}

                if (item.tsNum>0)
                {
                    item.num = Math.Round(item.outNum - item.tsNum,2);
                }

                if (item.num>0 && item.num != item.outNum)
                {
                    item.amount = Math.Round(item.num * item.price,2);
                }
            }
        }
        return output;
    }


    /// <summary>
    /// 调拨待入库提交.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Db/InAudit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpDbInAduitCrInput input)
    {
        _repository.Context.QueryFilter.Clear<ICompanyEntity>();
        //0、判断调拨出库状态是否正常(state=="1")
        //1、生成调拨入库记录
        //2、更新调拨出库的备注和调出单的状态

        var outEntity = _repository.Single(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (outEntity.State != "1")
        {
            throw Oops.Oh("调出订单状态异常！");
        }
        var outItems = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(x => x.OutId == outEntity.Id).ToListAsync();
        ErpInorderEntity inEntity = new ErpInorderEntity
        {
            Id = outEntity.Id,
            InType = "2",
            No = await _billRullService.GetBillNumber("QTErpInOrder"),
            Oid = outEntity.InOid,
            OutOid = outEntity.Oid,
        };

        var erpInrecordEntityList = new List<ErpInrecordEntity>();
        if (input.erpOutrecordList != null && input.erpOutrecordList.Any())
        {
            foreach (var x in input.erpOutrecordList)
            {
                var outItem = outItems.Find(it => it.Id == x.id) ?? throw Oops.Oh("调拨明细不存在，请刷新数据！");
                ErpInrecordEntity inrecordEntity = new ErpInrecordEntity
                {
                    Id = x.id,
                    InId = inEntity.Id,
                    Gid = x.gid,
                    InNum = x.num,
                    Amount = x.num ==0? 0 : outItem?.CostAmount ?? 0,
                    Num = x.num,
                    Remark = x.remark,
                    StoreRomeAreaId = x.storeRomeAreaId,
                    StoreRomeId = x.storeRomeId
                };

                // 找出关联的采购单
                var relationEntity = await _repository.Context.Queryable<ErpInrecordEntity>()
                    .InnerJoin<ErpInorderEntity>((a, b) => a.InId == b.Id)
                    .Where((a, b) => SqlFunc.Subqueryable<ErpOutdetailRecordEntity>().Where(x => x.InId == a.Id && x.OutId == outItem.Id).Any())
                    .Select((a, b) => new
                    {
                        a.Bid,
                        b.CgNo
                    })
                    .FirstAsync();

                if (relationEntity!=null)
                {
                    inrecordEntity.Bid = relationEntity.Bid;
                    if (inEntity.CgNo.IsNullOrEmpty())
                    {
                        inEntity.CgNo = relationEntity.CgNo;
                    }
                }

                if (x.amount>0)
                {
                    inrecordEntity.Amount = x.amount;
                }
                //计算单价
                if (inrecordEntity.InNum > 0)
                {
                    inrecordEntity.Price = Math.Round(inrecordEntity.Amount / inrecordEntity.InNum, 2);
                }
                else
                {
                    inrecordEntity.Price = outItem.Price;
                }
                erpInrecordEntityList.Add(inrecordEntity);

                if (outItem != null)
                {
                    outItem.Remark = x.remark;
                }
            }

            // 创建调入明细
            await _repository.Context.Insertable(erpInrecordEntityList).ExecuteCommandAsync();

            // 更新调出备注
            if (outItems.IsAny())
            {
                foreach (var item in outItems)
                {
                    await _repository.Context.Updateable(item).UpdateColumns(it => it.Remark).ExecuteCommandAsync();
                }
            }           

            // 更新调出单的状态
            outEntity.State = "2";
            await _repository.Context.Updateable(outEntity).UpdateColumns(it => it.State).ExecuteCommandAsync();
        }

        // 更新数据库
        await _repository.Context.Insertable<ErpInorderEntity>(inEntity).ExecuteCommandAsync();


        // 更新关联的特殊入库单价
        var tsEntitys = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => input.erpOutrecordList.Any(z => z.id == x.InId)).ToListAsync();
        if (tsEntitys.IsAny())
        {
            foreach (var item in input.erpOutrecordList)
            {
                //foreach (var xitem in tsEntitys.Where(x => x.InId == item.id))
                //{
                //    await _repository.Context.Updateable<ErpInrecordEntity>()
                //                   .SetColumns(it => new ErpInrecordEntity() { Price = item.price, Amount = item.price * it.InNum })
                //                   .Where(it => it.Id == xitem.TsId)
                //                   .ExecuteCommandAsync();
                //}

                await CheckTsRecord(item.id);
            }
        }

        
    }

    [NonAction]
    public async Task CheckTsRecord(string inid)
    {
        // 获取特殊入库的记录
        var tsList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => SqlFunc.Subqueryable<ErpInrecordTsEntity>().Where(t => t.TsId == x.Id && t.InId == inid).Any()).ToListAsync();
        if (tsList.IsAny())
        {
            var tsIdList = tsList.Select(x => x.Id).ToArray();
            // 合计特殊入库的金额
            var ts_amount_sum = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                .InnerJoin<ErpInrecordEntity>((a, b) => a.InId == b.Id)
                .Where((a, b) => tsIdList.Contains(a.TsId))
                .Select((a, b) => new
                {
                    a.TsId,
                    a.Num,
                    Amount = SqlFunc.IIF(a.Num == b.InNum, b.Amount, a.Num * b.Price),
                    b.Bid
                }).ToListAsync();

            _repository.Context.Tracking(tsList);
            foreach (var item in tsList)
            {
                var totalNum = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Num);

                item.Amount = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Amount);
                item.Price = item.InNum > 0 ? Math.Round(item.Amount / item.InNum, 2) : 0;

                if (totalNum >= item.InNum)
                {
                    item.IsSpecial = "1";
                }
                else
                {
                    item.IsSpecial = "";
                }

                if (item.Bid.IsNullOrEmpty())
                {
                    item.Bid = ts_amount_sum.Where(x => x.TsId == item.Id).FirstOrDefault()?.Bid;
                }
            }



            await _repository.Context.Updateable(tsList).ExecuteCommandAsync();



            //更新特殊入库主表的完成状态
            var tsMainIdList = tsList.Select(x => x.InId).Distinct().ToArray();
            // 未完成的特殊入库记录
            var list = await _repository.Context.Queryable<ErpInrecordEntity>()
                .Where(x => tsMainIdList.Contains(x.InId) && (x.IsSpecial ?? "") == "")
                .Select(x => x.InId)
                .Distinct()
                .ToListAsync();

            var mainList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => tsMainIdList.Contains(x.InId))
                .GroupBy(x => x.InId)
                .Select(x => new ErpInorderEntity
                {
                    Id = x.InId,
                    Amount = SqlFunc.AggregateSum(x.Amount)
                })
                .ToListAsync();

            // 更新主表的金额
            foreach (var item in mainList)
            {
                await _repository.Context.Updateable<ErpInorderEntity>(item).UpdateColumns(x => x.Amount).ExecuteCommandAsync();
            }


            if (list.IsAny())
            {
                await _repository.Context.Updateable<ErpInorderEntity>()
                   .SetColumns(x => new ErpInorderEntity
                   {
                       SpecialState = "",
                       SpecialUserId = ""
                   })
                   .Where(x => list.Contains(x.Id) && x.SpecialState == "1")
                   .ExecuteCommandAsync();
            }

            tsMainIdList = tsMainIdList.Except(list).ToArray();
            if (tsMainIdList.IsAny())
            {
                await _repository.Context.Updateable<ErpInorderEntity>()
                    .SetColumns(x => new ErpInorderEntity
                    {
                        SpecialState = "1",
                        SpecialUserId = _userManager.UserId
                    })
                    .Where(x => tsMainIdList.Contains(x.Id))
                    .ExecuteCommandAsync();
            }


            // 更新出库单的成本
            await this.UpdateOutrecordCost(tsList);
        }
    }

    /// <summary>
    /// 更新出库记录的金额
    /// </summary>
    /// <returns></returns>
    private async Task UpdateOutrecordCost(List<ErpInrecordEntity> indetailList)
    {
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

    /// <summary>
    /// 调拨出库同意（调入方确定后再同意）.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Db/Audit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task DbAudit(string id)
    {
        var entity = _repository.Single(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (entity.State != "2")
        {
            throw Oops.Oh("调拨记录状态异常，请稍后再尝试！");
        }

        _repository.Context.Tracking(entity);
        entity.State = "0"; //状态改回正常
        entity.OutCheckUserId = _userManager.UserId;
        entity.StateTime = DateTime.Now;
        await _repository.UpdateAsync(entity);
    }

    /// <summary>
    /// 导出Excel.
    /// 导出调出数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Db/ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpOutorderListQueryInput input, [FromServices]IFileManager fileManager)
    {
        var items = input.items.IsNotEmptyOrNull() ? input.items.Split(",", true) : new string[0];
        var list = await _repository.Context.Queryable<ErpOutorderEntity>()
            .LeftJoin<ErpOutrecordEntity>((it, a) => it.Id == a.OutId)
            .LeftJoin<ErpProductmodelEntity>((it, a, b) => a.Gid == b.Id)
            .LeftJoin<ErpProductEntity>((it, a, b,c) => c.Id == b.Pid)
          .WhereIF(input.beginDate.HasValue, it => it.CreatorTime >= input.beginDate)
          .WhereIF(input.endDate.HasValue, it => it.CreatorTime <= input.endDate)
          .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
          .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
          .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
              it.No.Contains(input.keyword)
              || it.InType.Contains(input.keyword)
              )
          .WhereIF(items.IsAny(), it => items.Contains(it.Id))
          .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
          .Select((it, a, b,c) => new ErpOutorderListDbExportOutput
          {
              //id = it.Id,
              //oid = it.Oid,
              no = it.No,
              //inType = it.InType,
              //amount = it.Amount,
              //xsNo = it.XsNo,
              //creatorTime = it.CreatorTime,
              oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
              inOidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.InOid).Select(d => d.FullName),
              //state = it.State
              createTime = it.CreatorTime.Value.ToString("yyyy-MM-dd"),
              amount = a.Amount,
              price = a.Price,
              num = a.Num,
              remark = a.Remark,
              name = b.Name,
              productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(ddd => ddd.Id == b.Pid).Select(ddd => ddd.Name),
              rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == c.Tid).Select(ddd => ddd.RootName)
          })
          .ToListAsync();

        var title = string.Format("{0:yyyyMMddHHmmss}_调拨出库.xls", DateTime.Now);
        ExcelConfig excelconfig = ExcelConfig.Default(title);
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOutorderListDbExportOutput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpOutorderListDbExportOutput>.ExportMemoryStream(list, excelconfig);
        var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT" + "|" + _userManager.TenantId) };
    }

    /// <summary>
    /// 自动调拨
    /// </summary>
    /// <returns></returns>
    [HttpPost("Db/Auto")]
    [SqlSugarUnitOfWork]
    public async Task<ErpDbInAduitCrInput> DbAuto([FromBody] ErpOutorderCrInput input)
    {
        // 清除公司过滤
        _repository.Context.QueryFilter.Clear<ICompanyEntity>();

        var id = await Create(input);
        var entity = await _repository.Context.Queryable<ErpOutorderEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var erpOutrecordList =await  _repository.Context.Queryable<ErpOutrecordEntity>().Where(x => x.OutId == id).Select<ErpDbrecordInAduitCrInput>().ToListAsync();
        var result = new ErpDbInAduitCrInput
        {
            id = id,
            oid = input.oid,
            inOid = input.inOid,
            inType = input.inType,
            no = entity.No,
            erpOutrecordList = erpOutrecordList
        };
        // 自动审核
        await Update(id, result);

        return result;
    }
    #endregion
    /// <summary>
    /// 新建出库订单表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task<string> Create([FromBody] ErpOutorderCrInput input)
    {
        var entity = input.Adapt<ErpOutorderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpOutOrder");
        // 状态默认为正常
        if (input.state.IsNullOrEmpty())
        {
            input.state = "0";
        }
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<ErpOutorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();
        var e = await _repository.AsQueryable().Where(x => x.Id == newEntity.Id).FirstAsync();
        if (input.erpOutrecordList != null)
            {
                var erpOutrecordEntityList = new List<ErpOutrecordEntity>();
                foreach (var item in input.erpOutrecordList)
                {
                    var newItem = item.Adapt<ErpOutrecordEntity>();
                    newItem.Id = SnowflakeIdHelper.NextId();
                    newItem.OutId = newEntity.Id;

                    if (!string.IsNullOrEmpty(newEntity.Oid))
                    {
                        newItem.Oid = newEntity.Oid;
                    }

                    erpOutrecordEntityList.Add(newItem);

                    //更新库存
                    if (item.storeDetailList != null && item.storeDetailList.Any())
                    {
                        // 扣减库存
                        var cost = await _erpStoreService.Reduce(new ErpOutdetailRecordUpInput
                        {
                            id = newItem.Id,
                            num = newItem.Num,
                            records = item.storeDetailList.Adapt<List<ErpOutdetailRecordInInput>>()
                        });
                        newItem.CostAmount = cost.CostAmount;
                    }
                }
                await _repository.Context.Insertable(erpOutrecordEntityList).ExecuteCommandAsync();
            }
      
        return newEntity.Id;

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
    /// 更新出库订单表.
    /// 不支持修改数据，只能删除重做
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpOutorderUpInput input)
    {
        throw Oops.Oh("不允许更新数据！");
        var entity = input.Adapt<ErpOutorderEntity>();
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<ErpOutorderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空商品出库记录原有数据
            await _repository.Context.Deleteable<ErpOutrecordEntity>().Where(it => it.OutId == entity.Id).ExecuteCommandAsync();

            // 新增商品出库记录新数据
            var erpOutrecordEntityList = input.erpOutrecordList.Adapt<List<ErpOutrecordEntity>>();
            if (erpOutrecordEntityList != null)
            {
                foreach (var item in erpOutrecordEntityList)
                {
                    item.Id ??= SnowflakeIdHelper.NextId();
                    item.OutId = entity.Id;
                }

                await _repository.Context.Insertable(erpOutrecordEntityList).ExecuteCommandAsync();
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
    /// 删除出库订单表.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        if (!await _repository.Context.Queryable<ErpOutorderEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        //try
        //{

        //    // 开启事务
        //    _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<ErpOutorderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();


            // 找出出库记录明细，然后恢复库存
            var records = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(it => it.OutId.Equals(id)).Select(x => new ErpOutrecordEntity { Id = x.Id }).ToListAsync();
            foreach (var record in records)
            {
                await _erpStoreService.Restore(record.Id);
            }

            // 清空商品出库记录表数据
            await _repository.Context.Deleteable<ErpOutrecordEntity>(records).ExecuteCommandAsync();
            //await _repository.Context.Deleteable<ErpOutrecordEntity>().Where(it => it.OutId.Equals(entity.Id)).ExecuteCommandAsync();

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
    /// 删除出库订单表明细.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("Detail/{id}")]
    [SqlSugarUnitOfWork]
    public async Task DeleteDetail(string id)
    {
        var record = await _repository.Context.Queryable<ErpOutrecordEntity>().FirstAsync(it => it.Id.Equals(id)) ?? throw Oops.Oh(ErrorCode.COM1005);

        // 已生成调入单就不允许删除
        var inrecord = await _repository.Context.Queryable<ErpInrecordEntity>().ClearFilter<ICompanyEntity>().Where(x => x.Id == record.Id).FirstAsync();
        if (inrecord!=null)
        {
            throw Oops.Oh("该出库记录已生成入库记录，无法删除！");
        }

        // 找出出库记录明细，然后恢复库存
        await _erpStoreService.Restore(record.Id);

        // 清空商品出库记录表数据
        await _repository.Context.Deleteable<ErpOutrecordEntity>(record).ExecuteCommandAsync();
      
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpOutorderListQueryInput input, [FromServices] IFileManager fileManager, [FromServices] IDictionaryDataService dictionaryDataService)
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


        //var qur = _repository.Context.Queryable<ErpBuyorderdetailEntity>()
        //        .InnerJoin<ErpBuyorderEntity>((it, a) => it.Fid == a.Id)
        //        .Select((it, a) => new
        //        {
        //            id = it.Id,
        //            no = a.No,
        //            amount = it.Amount
        //        });

        //List<DateTime> queryPaymentDate = input.paymentDate?.Split(',').ToObject<List<DateTime>>();
        //DateTime? startPaymentDate = queryPaymentDate?.First();
        //DateTime? endPaymentDate = queryPaymentDate?.Last();
        var data = await _repository.Context.Queryable<ErpOutorderEntity>()
            .LeftJoin<ErpOutrecordEntity>((it, a) => it.Id == a.OutId)
            .LeftJoin<ErpProductmodelEntity>((it, a, b) => a.Gid == b.Id)
          .WhereIF(input.beginDate.HasValue, it => it.CreatorTime >= input.beginDate)
          .WhereIF(input.endDate.HasValue, it => it.CreatorTime <= input.endDate)
          .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
          .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
          .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
              it.No.Contains(input.keyword)
              || it.InType.Contains(input.keyword)
              )
          .WhereIF(items.IsAny(), it => items.Contains(it.Id))
          .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
          .Select((it, a, b) => new ErpOutrecordExportOutput
          {
              //id = it.Id,
              //oid = it.Oid,
              //no = it.No,
              //inType = it.InType,
              //amount = it.Amount,
              //xsNo = it.XsNo,
              //creatorTime = it.CreatorTime,
              //oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
              //inOidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.InOid).Select(d => d.FullName),
              //state = it.State
              //createTime = it.CreatorTime.Value.ToString("yyyy-MM-dd"),
              //amount = a.Amount,
              //price = a.Price,
              id = a.Id,
              inType = it.InType,
              no = it.No,
              oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(ddd => ddd.Id == it.Oid).Select(ddd => ddd.FullName),
              num = a.Num,
              itremark = a.Remark,
              gidName = b.Name,
              productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(ddd => ddd.Id == b.Pid).Select(ddd => ddd.Name),
              remark = it.Remark,
              outid= it.Id,
              costAmount = a.CostAmount
          })
          .Take(input.pageSize)
          .ToListAsync();

        var outTypeOptions = await dictionaryDataService.GetList("QTErpCklx");

        foreach (var item in data)
        {
            item.inType = outTypeOptions.Find(x => x.EnCode == item.inType)?.FullName ?? "";
        }

        // 退货出库
        if (input.inType == "5")
        {
           // var idList = data.Select(x => x.id).ToList();
           // var erpOutrecordList = await _repository.Context.Queryable<ErpOutrecordEntity>()
           //.Where(w => idList.Contains(w.OutId))
           //.Select((w) => new ErpOutrecordEntity
           //{
           //    Id = w.Id,
           //    OutId = w.OutId
           //}).ToListAsync();

            // 关联入库记录
            var list = await _repository.Context.Queryable<ErpInrecordEntity>()
                .InnerJoin<ErpOutdetailRecordEntity>((a, b) => a.Id == b.InId)
                .LeftJoin<ErpBuyorderdetailEntity>((a, b, c) => a.Bid == c.Id)
                .Where((a, b) => data.Any(x => x.id == b.OutId))
                .Select((a, b, c) => new ErpStoreInfoOutput
                {
                    id = a.Id,
                    price = a.Price,
                    amount = SqlFunc.Round(a.Price * b.Num, 2),
                    supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(ddd => ddd.Id == c.Supplier).Select(ddd => ddd.Name),
                    billId = b.OutId,
                    num = b.Num,
                    no = SqlFunc.Subqueryable<ErpInorderEntity>().Where(ddd => ddd.Id == a.InId).Select(ddd => ddd.No)
                })
            .ToListAsync();

            List<ErpOutrecordThExportOutput> data2 = new List<ErpOutrecordThExportOutput>();
            foreach (var item in data)
            {
                var xitem = item.Adapt<ErpOutrecordThExportOutput>();
                xitem.supplierName = string.Join(",", list.Where(x => x.billId == item.id).Select(x => x.supplierName).Distinct());
                xitem.inNo = string.Join(",", list.Where(x => x.billId == item.id).Select(x => x.no).Distinct());

                data2.Add(xitem);
            }


            ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_{1}.xls", DateTime.Now, "退货出库记录"));
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOutrecordThExportOutput>();
            foreach (KeyValuePair<string, string> item in FileEncode)
            {
                string? column = item.Key;
                string? excelColumn = item.Value;
                excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
            var fs = ExcelExportHelper<ErpOutrecordThExportOutput>.ExportMemoryStream(data2, excelconfig);
            var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
            if (flag.Item1)
            {
                fs.Flush();
                fs.Close();
            }


            return new { name = excelconfig.FileName, url = flag.Item2 };
        }
        else
        {

            ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_{1}.xls", DateTime.Now, "出库记录"));
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOutrecordExportOutput>();
            foreach (KeyValuePair<string, string> item in FileEncode)
            {
                string? column = item.Key;
                string? excelColumn = item.Value;
                excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
            var fs = ExcelExportHelper<ErpOutrecordExportOutput>.ExportMemoryStream(data, excelconfig);
            var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
            if (flag.Item1)
            {
                fs.Flush();
                fs.Close();
            }


            return new { name = excelconfig.FileName, url = flag.Item2 };
        }

    }
}
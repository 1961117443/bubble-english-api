using Mapster;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Service;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp.OrderFj;
using QT.JXC.Interfaces;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using QT.UnifyResult;
using Spire.Presentation;
using System.Diagnostics;

namespace QT.JXC;

/// <summary>
/// 业务实现：订单信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpOrderFj", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpOrderFjService : IErpOrderFjService, IDynamicApiController, ITransient
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
    private readonly IErpStoreService _erpStoreService;
    private readonly ICacheManager _cacheManager;
    private readonly IErpOrderTraceService _erpOrderTraceService;
    private readonly IErpCustomerService _erpCustomerService;
    private readonly ICoreSysConfigService _coreSysConfigService;
    private readonly ErpOutorderService _erpOutorderService;

    /// <summary>
    /// 初始化一个<see cref="ErpOrderFjService"/>类型的新实例.
    /// </summary>
    public ErpOrderFjService(
        ISqlSugarRepository<ErpOrderEntity> erpOrderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IErpStoreService erpStoreService,
        ICacheManager cacheManager,
        IErpOrderTraceService erpOrderTraceService,
        IErpCustomerService erpCustomerService,
        ICoreSysConfigService coreSysConfigService,
        ErpOutorderService erpOutorderService)
    {
        _repository = erpOrderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _erpStoreService = erpStoreService;
        _cacheManager = cacheManager;
        _erpOrderTraceService = erpOrderTraceService;
        _erpCustomerService = erpCustomerService;
        _coreSysConfigService = coreSysConfigService;
        _erpOutorderService = erpOutorderService;
    }

    /// <summary>
    /// 获取订单信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = await _repository.Where(x => x.Id == id).Select<ErpOrderFjInfoOutput>(it => new ErpOrderFjInfoOutput
        {
            cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(x => x.Id == it.Cid).Select(x => x.Name),
            cidType = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(x => x.Id == it.Cid).Select(x => x.Type)
        }, true).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);
        if (output.state.IsNullOrEmpty())
        {
            output.state = OrderStateEnum.Draft;
        }
        //if (!output.cid.IsNullOrEmpty())
        //{
        //    //output.cidName = await _cacheManager.GetOrCreateAsync($"ErpCustomerEntity:{output.cid}", async entry =>
        //    //{
        //    //    return await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == output.cid).Select(x => x.Name).FirstAsync();
        //    //});
        //    output.cidName = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == output.cid).Select(x => x.Name).FirstAsync();
        //}
        if (output.posttime.HasValue)
        {
            output.dayOfWeek = output.posttime.Value.ToString("dddd");
        }
        var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((it, a) => it.Mid == a.Id)
            .InnerJoin<ErpProductEntity>((it, a, b) => a.Pid == b.Id)
            .Where(it => it.Fid == output.id)
            .Select((it, a, b) => new ErpOrderdetailInfoOutput
            {
                productId = b.Id,
                midName = a.Name,
                productName = b.Name,
                sorterState = it.SorterState ?? "0",
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName),
                midUnit = a.Unit,
                customerUnit = a.CustomerUnit ?? a.Unit,
                fjNum = it.FjNum ?? 0,
                remark = it.Remark ?? "",
                sorterDes = it.SorterDes ?? ""
            }, true).ToListAsync();
        output.erpOrderdetailList = erpOrderdetailList; // erpOrderdetailList.Adapt<List<ErpOrderdetailInfoOutput>>();

        var erpOrderoperaterecordList = await _repository.Context.Queryable<ErpOrderoperaterecordEntity>().Where(w => w.Fid == output.id).ToListAsync();
        output.erpOrderoperaterecordList = erpOrderoperaterecordList.Adapt<List<ErpOrderoperaterecordInfoOutput>>();

        var erpOrderremarksList = await _repository.Context.Queryable<ErpOrderremarksEntity>().Where(w => w.OrderId == output.id).ToListAsync();
        output.erpOrderremarksList = erpOrderremarksList.Adapt<List<ErpOrderremarksInfoOutput>>();

        // 加载图片
        await Wrapper(erpOrderdetailList);
        //if (erpOrderdetailList.IsAny())
        //{
        //    var idList = erpOrderdetailList.Select(x => x.productId).Distinct().ToArray();
        //    if (idList.IsAny())
        //    {
        //        var erpProductpicList = await _repository.Context.Queryable<ErpProductpicEntity>().Where(it => idList.Contains(it.Pid)).ToListAsync();
        //        foreach (var item in erpOrderdetailList)
        //        {
        //            item.imageList = erpProductpicList.Where(it => it.Pid == item.productId).Select(it => it.Pic).ToArray();
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
    public async Task<dynamic> GetList([FromQuery] ErpOrderFjListQueryInput input)
    {
        var data = await CreateQuery(input).ToPagedListAsync(input.currentPage, input.pageSize);

        await Wrapper(data?.list);


        await _cacheManager.SetAsync($"u:{_userManager.UserId}:m:GetList", input);

        return PageResult<ErpOrderFjListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 瀑布流获取数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Waterfall")]
    public async Task<List<ErpOrderFjListOutput>> ListWaterfall([FromQuery] ErpOrderFjListQueryInput input)
    {
        var firstInput = await _cacheManager.GetAsync<ErpOrderFjListQueryInput>($"u:{_userManager.UserId}:m:GetList");
        if (firstInput != null)
        {
            firstInput.currentPage = input.currentPage;
        }
        else
        {
            firstInput = input;
        }


        var list = await CreateQuery(firstInput).ToPageListAsync(firstInput.currentPage, firstInput.pageSize);

        await Wrapper(list);

        return list;
    }

    #region 分类分拣

    /// <summary>
    /// 获取分类汇总.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Category")]
    public async Task<dynamic> GetCategoryList([FromQuery] ErpOrderFjListQueryInput input)
    {
        var list = await _repository.Context.Queryable<ErpOrderEntity>()
            .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
            .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
            .InnerJoin<ViewErpProducttypeEx>((a, b, c, d, e) => d.Tid == e.Id)
            .Where(a => a.State == OrderStateEnum.PendingApproval)
            //.Where((a, b) => (b.SorterState ?? "") != "1")
            .WhereIF(input.cid.IsNotEmptyOrNull(), a => a.Cid == input.cid)
             .WhereIF(input.createTime.HasValue, a => a.CreateTime >= input.createTime && a.CreateTime < input.createTime.Value.AddDays(1))
             .WhereIF(input.posttime.HasValue, a => a.Posttime >= input.posttime && a.Posttime < input.posttime.Value.AddDays(1))
            .GroupBy((a, b, c, d, e) => new { e.RootId, e.RootName })
            .Select((a, b, c, d, e) => new
            {
                id = e.RootId,
                name = e.RootName,
                num = SqlFunc.AggregateDistinctCount(b.Mid)
            })
            .ToListAsync();

        return new { list = list.OrderBy(x => x.name).ToList() };
    }

    /// <summary>
    /// 获取分类分拣.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Category/{id}")]
    public async Task<dynamic> GetCategoryList(string id, [FromQuery] ErpOrderFjListQueryInput input)
    {
        var data = await CreateCategoryQuery(id, input).ToPagedListAsync(input.currentPage, input.pageSize);

        await Wrapper(data?.list);


        await _cacheManager.SetAsync($"u:{_userManager.UserId}:m:GetCategoryList", input);

        return PageResult<ErpOrderdetailInfoOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取分类分拣按规格汇总.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Category/{id}/byProduct")]
    public async Task<dynamic> GetCategoryListGroupByProduct(string id, [FromQuery] ErpOrderFjListQueryInput input)
    {
        var sp = Stopwatch.StartNew();
        var data = await CreateCategoryQuery(id, input)
            .MergeTable()
            .GroupBy(x => new { x.mid, x.productName, x.midName, x.productId, x.midUnit })
            .Select(x => new ErpOrderdetailInfoOutput
            {

                id = x.mid,
                mid = x.mid,
                productId = x.productId,
                productName = x.productName,
                midName = x.midName,
                midUnit = x.midUnit,
                num = SqlFunc.AggregateSum(x.num),
                num1 = SqlFunc.AggregateSum(x.num1),
                num2 = SqlFunc.AggregateCount(x.id)
            })
            .ToListAsync();
        sp.Stop();
        Console.WriteLine("GetCategoryListGroupByProduct查询耗时：{0}毫秒", sp.ElapsedMilliseconds);
        sp = Stopwatch.StartNew();
        await Wrapper(data);
        sp.Stop();
        Console.WriteLine("GetCategoryListGroupByProduct:Wrapper查询耗时：{0}毫秒", sp.ElapsedMilliseconds);
        return new
        {
            pagination = new PageResult
            {
                pageIndex = 1,
                pageSize = data.Count,
                total = data.Count
            },
            list = data.OrderBy(x => x.mid).ToList()
        };
    }

    /// <summary>
    /// 获取分类分拣按规格汇总.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Category/byProduct/{id}")]
    public async Task<dynamic> GetCategoryListByProduct(string id, [FromQuery] ErpOrderFjListQueryInput input)
    {
        //var input = new ErpOrderFjListQueryInput()
        //{
        //    mid = id
        //};
        input.mid = id;
        var data = await CreateCategoryQuery("0", input).ToListAsync();

        await Wrapper(data);

        return new
        {
            list = data
        };
    }

    /// <summary>
    /// 瀑布流获取分类数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Category/{id}/Waterfall")]
    public async Task<List<ErpOrderdetailInfoOutput>> CategoryListWaterfall(string id, [FromQuery] ErpOrderFjListQueryInput input)
    {
        var firstInput = await _cacheManager.GetAsync<ErpOrderFjListQueryInput>($"u:{_userManager.UserId}:m:GetCategoryList");
        if (firstInput != null)
        {
            firstInput.currentPage = input.currentPage;
        }
        else
        {
            firstInput = input;
        }

        var list = await CreateCategoryQuery(id, firstInput).ToPageListAsync(firstInput.currentPage, firstInput.pageSize);

        await Wrapper(list);

        return list;
    }
    #endregion



    /// <summary>
    /// 更新订单明细的分拣数量.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("{id}/handle")]
    [Obsolete]
    public async Task HandleFJ(string id, [FromBody] ErpOrderDetailFjHlInput input)
    {
        var entity = await _repository.Context.Queryable<ErpOrderdetailEntity>().InSingleAsync(input.id) ?? throw Oops.Oh(ErrorCode.COM1005);
        // 非分拣状态不允许修改
        if (!await _repository.Context.Queryable<ErpOrderEntity>().AnyAsync(x => x.Id == entity.Fid && x.State == OrderStateEnum.PendingApproval))
        {
            throw Oops.Oh("当前订单非分拣状态，不允许修改!");
        }
        _repository.Context.Tracking(entity);
        entity.Num1 = input.num1;
        entity.SorterState = "1";
        entity.SorterTime = DateTime.Now;
        entity.SorterUserId = _userManager.UserId;

        // 根据分拣数量计算金额
        entity.Amount1 = entity.SalePrice * entity.Num1;

        await _repository.Context.Updateable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 取消订单明细的分拣数量.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("{id}/cancel/{mainId}")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> CancelFJWithMainId(string id, string mainId)
    {
        await CancelFJ(id);

        return await GetInfo(mainId);
    }
    /// <summary>
    /// 取消订单明细的分拣数量.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("{id}/cancel")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> CancelFJ(string id)
    {
        _repository.Context.EnableDebug();
        var entity = await _repository.Context.Queryable<ErpOrderdetailEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        // 非分拣状态不允许修改
        if (!await _repository.Context.Queryable<ErpOrderEntity>().AnyAsync(x => x.Id == entity.Fid && x.State == OrderStateEnum.PendingApproval))
        {
            throw Oops.Oh("当前订单非分拣状态，不允许修改!");
        }

        //1、删除出库单记录
        //2、更新订单分拣数据
        //3、如果是特殊入库，删除特殊入库记录
        var tsList = await _repository.Context.Queryable<ErpInrecordEntity>().InnerJoin<ErpInorderEntity>((a, b) => a.InId == b.Id)
             .InnerJoin<ErpOutdetailRecordEntity>((a, b, c) => c.InId == a.Id)
             .Where((a, b, c) => b.InType == "5" && c.OutId == id)
             .Select((a, b, c) => new
             {
                 did = a.Id,
                 mid = b.Id
             })
             .ToListAsync();

        // 先判断特殊入库记录是否关联了采购记录
        if (tsList.IsAny())
        {
            //var tsEntity = await _repository.Context.Queryable<ErpInrecordTsEntity>().FirstAsync(it => tsList.Any(x => x.did == it.TsId));
            //if (tsEntity!=null)
            //{
            //    var cgNo = await _repository.Context.Queryable<ErpInorderEntity>()
            //        .InnerJoin<ErpInrecordEntity>((a, b) => a.Id == b.InId)
            //        .Where((a, b) => b.Id == tsEntity.InId)
            //        .Select((a, b) => a.CgNo)
            //        .FirstAsync();

            //    var msg = $"特殊入库已关联采购订单";
            //    if (cgNo.IsNotEmptyOrNull())
            //    {
            //        msg += $"[{cgNo}]";
            //    }
            //    throw Oops.Oh(msg);
            //}

            // 排除掉已关联采购订单的记录
            //var list1 = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(it => tsList.Any(x => x.did == it.TsId)).ToListAsync();
            var relations = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(it => tsList.Any(x => x.did == it.TsId)).Select(it => it.TsId).ToListAsync();
            for (int i = tsList.Count - 1; i >= 0; i--)
            {
                if (relations.Contains(tsList[i].did))
                {
                    tsList.RemoveAt(i);
                }
            }
        }

        #region 【1、恢复库存、删除出库单记录】
        await _erpStoreService.Restore(id);
        await _repository.Context.Deleteable(new ErpOutorderEntity { Id = id }).ExecuteCommandAsync();
        await _repository.Context.Deleteable(new ErpOutrecordEntity { Id = id }).ExecuteCommandAsync();
        #endregion

        #region 【2、更新订单分拣数据】
        _repository.Context.Tracking(entity);
        entity.Num1 = 0;
        entity.FjNum = 0;
        entity.SorterState = "";
        entity.SorterTime = null;
        entity.SorterUserId = "";
        entity.SorterFinishTime = null;
        await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        #endregion

        #region 【3、如果是特殊入库，删除特殊入库记录】


        if (tsList.IsAny())
        {
            var ids = tsList.Select(x => x.did).ToList();
            var tempList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => ids.Contains(x.Id)).Select(x => new ErpInrecordEntity
            { Id = x.Id, Num = x.Num, InNum = x.InNum, InId = x.InId })
                .ToListAsync();

            foreach (var item in tempList)
            {
                // 没有其他出库记录才删除
                if (item.InNum == item.Num)
                {
                    // 删除从表
                    await _repository.Context.Deleteable<ErpInrecordEntity>(item).EnableDiffLogEvent().ExecuteCommandAsync();

                    if (!await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => x.InId == item.InId).AnyAsync())
                    {
                        // 删除主表
                        await _repository.Context.Deleteable<ErpInorderEntity>(new ErpInorderEntity { Id = item.InId }).EnableDiffLogEvent().ExecuteCommandAsync();
                    }
                }
            }

        }
        #endregion

        UnifyContext.Fill(new
        {
            sql = _repository.Context.GetDebugSql()
        });
        return new
        {
            num1 = entity.Num1,
            sorterState = entity.SorterState,
            sorterTime = entity.SorterTime,
            sorterUserId = entity.SorterUserId,
            sorterFinishTime = entity.SorterFinishTime
        };


    }

    /// <summary>
    /// 创建出库订单
    /// </summary>
    /// <returns></returns>
    [HttpPost("{id}/Create/Outer")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> CreateOutorderWithId(string id, [FromBody] ErpOutdetailRecordUpInput input)
    {
        await CreateOutorder(input);

        //extra.Add("dataForm", await GetInfo(order.Id));
        return await GetInfo(id);
    }
    /// <summary>
    /// 创建出库订单
    /// </summary>
    /// <returns></returns>
    [HttpPost("Create/Outer")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> CreateOutorder([FromBody] ErpOutdetailRecordUpInput input)
    {
        //throw Oops.Oh("当前订单非分拣状态，不允许修改!");
        //_repository.Context.EnableDebug();
        //1、创建出库单，
        //2、扣减库存数量
        //3、更新订单的分拣数量
        //var _db = _repository.Ado;
        Dictionary<string, object> extra = new Dictionary<string, object>();
        //var s0 = Stopwatch.StartNew();
        var orderItem = await _repository.Context.Queryable<ErpOrderdetailEntity>().InSingleAsync(input.id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var order = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(orderItem.Fid) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (order.State != OrderStateEnum.PendingApproval)
        {
            throw Oops.Oh("当前订单非分拣状态，不允许修改!");
        }

        if (orderItem.SorterState == "1")
        {
            throw Oops.Oh("当前明细已做分拣，请取消后再重新修改!");
        }
        var list = await _repository.Context
            .Queryable<ErpInrecordEntity>()
            .ClearFilter<ICompanyEntity>()
            .Where(x => input.records.Any(r => r.id == x.Id) && x.Gid == orderItem.Mid).ToListAsync();
        if (!list.IsAny())
        {
            throw Oops.Oh("数据异常，请返回再重新操作!");
        }

        // 判断是否快速调拨，判断的条件应该是list里面不包含当前公司的数据
        if (list.Where(x => x.Oid != _userManager.CompanyId).IsAny())
        //if (list.GroupBy(x => x.Oid).Count() > 1)
        {
            // 存在多个公司数据，必须是调拨单
            // 判断关联库存是否有数据
            var config = await _coreSysConfigService.GetConfig<SysErpConfigOutput>();
            if (config.erpShareStock.IsNotEmptyOrNull())
            {
                var oid = _userManager.CompanyId;
                var companys = config.erpShareStock.Split(",", true);
                if (companys.IsAny() && companys.Contains(oid))
                {
                    foreach (var item in list)
                    {
                        if (!companys.Contains(item.Oid))
                        {
                            throw Oops.Oh("数据异常，存在多个公司的数据！");
                        }
                    }

                    var total = input.num;
                    List<ErpOutdetailRecordInInput> records = new List<ErpOutdetailRecordInInput>();
                    foreach (var item in input.records)
                    {
                        var xitem = list.FirstOrDefault(x => x.Id == item.id);
                        if (xitem == null)
                        {
                            throw Oops.Oh("数据异常，请返回再重新操作!");
                        }
                        if (xitem.Oid == oid)
                        {
                            total -= xitem.Num;
                            // 本公司数据，直接使用
                            records.Add(item);
                        }
                        else
                        {
                            // 剩余需求数大于库存数，全部使用
                            var num = total > xitem.Num ? xitem.Num : total;
                            if (num > 0)
                            {
                                total -= num; // 扣减需求数
                                var newRecord = await _erpOutorderService.DbAuto(new ErpOutorderCrInput
                                {
                                    oid = xitem.Oid,
                                    inOid = oid,
                                    inType = "2",
                                    state = "1",
                                    erpOutrecordList = new List<ErpOutrecordCrInput>
                                    {
                                        new ErpOutrecordCrInput
                                        {
                                            gid  = xitem.Gid,
                                            num = num,
                                            storeDetailList = new List<ErpStorerecordInput>()
                                            {
                                                new ErpStorerecordInput { id = xitem.Id, num = xitem.Num }
                                            },
                                            price = xitem.Price,
                                            amount = Math.Round(xitem.Price * num,2)
                                        }
                                    }
                                });
                                records.Add(new ErpOutdetailRecordInInput
                                {
                                    id = newRecord.erpOutrecordList[0].id,
                                    num = newRecord.erpOutrecordList[0].num
                                });
                            }

                        }
                    }
                    input.records = records; // 替换records
                }
                else
                {
                    throw Oops.Oh("数据异常，存在多个公司的数据！");
                }
            }
            else
            {
                throw Oops.Oh("数据异常，存在多个公司的数据！");
            }
        }



        // 开始处理分拣
        var outOrderNo = await _billRullService.GetBillNumber("QTErpOutOrder");
        //s0.Stop();
        //extra.Add("0验证数据", s0.ElapsedMilliseconds);
        ////判断是否已经有做过出库记录（放在取消那里操作）
        //var itemEntity = await _repository.Context.Queryable<ErpOutrecordEntity>().InSingleAsync(input.id) ?? throw Oops.Oh(ErrorCode.COM1005);
        //if (itemEntity!=null)
        //{
        //    //先把库存还原回来

        //}

        var s1 = Stopwatch.StartNew();
        #region 【1、创建出库单】
        //出库主表实体
        ErpOutorderEntity outEntity = new()
        {
            Id = orderItem.Id,
            InType = "1", //出库
            No = outOrderNo,
            Oid = order.Oid,
            XsNo = order.No,
            Amount = orderItem.SalePrice * input.num
        };

        //出库明细
        var outItemEntity = new ErpOutrecordEntity
        {
            Id = orderItem.Id,
            Oid = order.Oid,
            Gid = orderItem.Mid,
            Num = input.num,
            OrderId = orderItem.Id,
            OutId = orderItem.Id,
            OutTime = DateTime.Now,
            Amount = orderItem.SalePrice * input.num
        };


        //await _repository.Context.Insertable<ErpOutorderEntity>(outEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();

        ////插入出库明细
        //await _repository.Context.Insertable<ErpOutrecordEntity>(outItemEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"{_repository.Context.Insertable<ErpOutorderEntity>(outEntity).ToSqlString()}");
        stringBuilder.Append($"{_repository.Context.Insertable<ErpOutrecordEntity>(outItemEntity).ToSqlString()}");
        await _repository.Ado.ExecuteCommandAsync(stringBuilder.ToString());
        #endregion
        s1.Stop();
        extra.Add("1创建出库单", s1.ElapsedMilliseconds);

        _repository.Context.Tracking(orderItem);
        //var s2 = Stopwatch.StartNew();
        #region 【2、扣减库存数量】
        var cost = await _erpStoreService.Reduce(input);
        if (cost.CostAmount > 0)
        {
            outItemEntity.CostAmount = cost.CostAmount;
            await _repository.Context.Updateable(outItemEntity).UpdateColumns(x => x.CostAmount).ExecuteCommandAsync();
            //stringBuilder.Append($"{_repository.Context.Updateable(outItemEntity).UpdateColumns(x => x.CostAmount).ToSqlString()}");
        }

        #endregion
        //s2.Stop();
        //extra.Add("2扣减库存数量", s2.ElapsedMilliseconds);

        //var s3 = Stopwatch.StartNew();
        #region 【3、更新订单的分拣数量】
        stringBuilder.Clear();
        orderItem.Num1 = outItemEntity.Num;
        orderItem.SorterState = "1";
        orderItem.SorterTime = DateTime.Now;
        orderItem.SorterUserId = _userManager.UserId;
        if (input.fjNum.HasValue)
        {
            orderItem.FjNum = input.fjNum.Value;
        }

        // 分拣完成
        if (input.done.HasValue && input.done == 1)
        {
            orderItem.SorterFinishTime = DateTime.Now;
        }


        // 根据分拣数量计算金额
        if (await _erpCustomerService.Check(order.Cid))
        {
            orderItem.Amount1 = Math.Floor(orderItem.SalePrice * orderItem.Num1 * 100) * 0.01m;
        }
        else
        {
            orderItem.Amount1 = orderItem.SalePrice * orderItem.Num1;
        }


        bool updateJE = false;
        if (orderItem.Amount1 > 0)
        {
            orderItem.Amount = orderItem.Amount1;
            updateJE = true;
        }

        stringBuilder.Append(_repository.Context.Updateable<ErpOrderdetailEntity>(orderItem).ToSqlString()).Append(";");

        //await _repository.Context.Updateable<ErpOrderdetailEntity>(orderItem).ExecuteCommandAsync();

        if (updateJE)
        {
            // 求和订单金额
            var amount = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == order.Id).SumAsync(x => x.Amount);

            order.Amount = amount;

            stringBuilder.Append(_repository.Context.Updateable(order).UpdateColumns(nameof(ErpOrderEntity.Amount)).ToSqlString()).Append(";");

            //await _repository.Context.Updateable(order).UpdateColumns(nameof(ErpOrderEntity.Amount)).ExecuteCommandAsync();
        }

        if (stringBuilder.Length > 0)
        {
            await _repository.Ado.ExecuteCommandAsync(stringBuilder.ToString());
        }
        #endregion

        //s3.Stop();
        //extra.Add("3更新订单的分拣数量", s3.ElapsedMilliseconds);

        //extra.Add("sql", _repository.Context.GetDebugSql());

        //extra.Add("dataForm", await GetInfo(order.Id));
        //UnifyContext.Fill(extra);
        //// 关闭事务
        //_db.CommitTran();

        return new
        {
            num1 = orderItem.Num1,
            sorterState = orderItem.SorterState,
            sorterTime = orderItem.SorterTime,
            sorterUserId = orderItem.SorterUserId,
            amount1 = orderItem.Amount1
        };

    }

    /// <summary>
    /// 获取未分拣订单汇总数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Sum")]
    public async Task<dynamic> GetSumList()
    {
        var list = await _repository.Context.Queryable<ErpOrderEntity>()
            .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
            .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
            .Where(a => a.State == OrderStateEnum.PendingApproval)
            .GroupBy((a, b, c, d) => new { b.Mid, c.Pid, c.Name, pName = d.Name })
            .Select((a, b, c, d) => new ErpOrderFjSumListOutput
            {
                mid = b.Mid,
                pid = c.Pid,
                name = c.Name,
                productName = d.Name,
                num = SqlFunc.AggregateSum(b.Num)
            })
            .ToListAsync();

        SqlSugarPagedList<ErpOrderFjSumListOutput> data = new SqlSugarPagedList<ErpOrderFjSumListOutput>
        {
            list = list,
            pagination = new PagedModel
            {
                PageIndex = 1,
                PageSize = list.Count,
                Total = list.Count
            }
        };
        return PageResult<ErpOrderFjSumListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取有库存未分拣订单汇总数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("StoreSum")]
    public async Task<dynamic> GetStoreSumList()
    {
        var list = await _repository.Context.Queryable<ErpOrderEntity>()
            .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
            .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
            .Where(a => a.State == OrderStateEnum.PendingApproval)
            .GroupBy((a, b, c, d) => new { b.Mid, c.Pid, c.Name, pName = d.Name })
            .Select((a, b, c, d) => new ErpOrderFjStoreSumListOutput
            {
                mid = b.Mid,
                pid = c.Pid,
                name = c.Name,
                productName = d.Name,
                num = SqlFunc.AggregateSum(b.Num)
            })
            .ToListAsync();

        var ids = list.Select(x => x.mid).Distinct().ToList();

        var storeList = await _repository.Context.Queryable<ErpInrecordEntity>()
            .Where(x => ids.Contains(x.Gid) && x.Num > 0)
            .GroupBy(x => x.Gid)
            .Select(x => new
            {
                x.Gid,
                num = SqlFunc.AggregateSum(x.Num)
            }).ToListAsync();

        list.ForEach(x =>
        {
            x.storeNum = storeList.Find(w => w.Gid == x.mid)?.num ?? 0m;
        });

        list = list.Where(x => x.storeNum > 0).ToList();

        return new
        {
            list,
            pagination = new
            {
                pageIndex = 1,
                pageSize = list.Count,
                total = list.Count
            }
        };
        //SqlSugarPagedList<ErpOrderFjStoreSumListOutput> data = new SqlSugarPagedList<ErpOrderFjStoreSumListOutput>
        //{
        //    list = list,
        //    pagination = new PagedModel
        //    {
        //        PageIndex = 1,
        //        PageSize = list.Count,
        //        Total = list.Count
        //    }
        //};
        //return PageResult<ErpOrderFjStoreSumListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 获取当天未分拣订单汇总数据.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Sum/Today")]
    public async Task<dynamic> GetSumTodayList()
    {
        var today = DateTime.Now.Date;
        var list = await _repository.Context.Queryable<ErpOrderEntity>()
            .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
            .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
            .Where(a => a.State == OrderStateEnum.PendingApproval)
            .Where(a => a.Posttime >= today && a.Posttime < today.AddDays(1))
            .GroupBy((a, b, c, d) => new { b.Mid, c.Pid, c.Name, pName = d.Name })
            .Select((a, b, c, d) => new ErpOrderFjSumListOutput
            {
                mid = b.Mid,
                pid = c.Pid,
                name = c.Name,
                productName = d.Name,
                num = SqlFunc.AggregateSum(b.Num)
            })
            .ToListAsync();

        SqlSugarPagedList<ErpOrderFjSumListOutput> data = new SqlSugarPagedList<ErpOrderFjSumListOutput>
        {
            list = list,
            pagination = new PagedModel
            {
                PageIndex = 1,
                PageSize = list.Count,
                Total = list.Count
            }
        };
        return PageResult<ErpOrderFjSumListOutput>.SqlSugarPageResult(data);
    }

    #region 导出
    /// <summary>
    /// 导出Excel.
    /// 有固定的模板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel/{type}")]
    public async Task<dynamic> ExportExcel(string type, [FromQuery] ErpOrderFjListQueryInput input, [FromServices] IFileManager fileManager)
    {
        if (type == "4" || type == "5")
        {
            var today = DateTime.Now.Date;
            var list = await _repository.Context.Queryable<ErpOrderEntity>()
                .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
                .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
                .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
                .Where(a => a.State == OrderStateEnum.PendingApproval)
                .WhereIF(type == "4", a => a.Posttime >= today && a.Posttime < today.AddDays(1))
                .GroupBy((a, b, c, d) => new { b.Mid, c.Pid, c.Name, pName = d.Name })
                .Select((a, b, c, d) => new ErpOrderFjSumListOutput
                {
                    mid = b.Mid,
                    pid = c.Pid,
                    name = c.Name,
                    productName = d.Name,
                    num = SqlFunc.AggregateSum(b.Num)
                }).ToListAsync();

            var title = type == "4" ? string.Format("{0:yyyy-MM-dd}_当日汇总.xls", DateTime.Now) : string.Format("{0:yyyy-MM-dd}_未分拣汇总.xls", DateTime.Now);
            ExcelConfig excelconfig = ExcelConfig.Default(title);
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOrderFjSumListOutput>();
            //var selectKey = input.selectKey.Split(',').ToList();
            foreach (KeyValuePair<string, string> item in FileEncode)
            {
                string? column = item.Key;
                string? excelColumn = item.Value;
                excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
            var fs = ExcelExportHelper<ErpOrderFjSumListOutput>.ExportMemoryStream(list, excelconfig);
            var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
            if (flag.Item1)
            {
                fs.Flush();
                fs.Close();
            }

            return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
        }

        if (type == "6")
        {
            var result = await GetStoreSumList();
            var list = result.list as List<ErpOrderFjStoreSumListOutput>;

            var title = string.Format("{0:yyyy-MM-dd}_有库存未分拣汇总.xls", DateTime.Now);
            ExcelConfig excelconfig = ExcelConfig.Default(title);
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpOrderFjStoreSumListOutput>();
            //var selectKey = input.selectKey.Split(',').ToList();
            foreach (KeyValuePair<string, string> item in FileEncode)
            {
                string? column = item.Key;
                string? excelColumn = item.Value;
                excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
            var fs = ExcelExportHelper<ErpOrderFjStoreSumListOutput>.ExportMemoryStream(list, excelconfig);
            var flag = await fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
            if (flag.Item1)
            {
                fs.Flush();
                fs.Close();
            }

            return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
        }

        throw Oops.Oh("导出失败！");
    }
    #endregion


    /// <summary>
    /// 分拣提交，检查所有商品是否分拣完成（已分拣，并且数量大于订单数量）
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/{id}/SubmitCheck")]
    public async Task<ErpOrderFjSubmitCheckOutput> HandleSubmitCheck(string id)
    {
        var list = await GetOrderFjWaitList(id);

        return new ErpOrderFjSubmitCheckOutput
        {
            list = list.Where(x => x.sorterState == "1").ToList(), //只显示已分拣的记录
            done = !list.Any(x => x.wnum > 0), // 是否整单分拣完毕(不存在待分拣数量)
        };


    }

    /// <summary>
    /// 分拣提交 分批提交
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <param name="erpOrderService"></param>
    /// <returns></returns>
    [HttpPost("Actions/{id}/Submit")]
    [SqlSugarUnitOfWork]
    public async Task HandleSubmit(string id, [FromBody] List<ErpOrderFjWaitListOutput> input, [FromServices] IErpOrderService erpOrderService)
    {
        //1、如果整单提交，并且没有拆过子单，那么按照原来的方式执行 ProcessOrder
        //2、如果整单提交，但是有子单，那么把当前订单生成一个子单，然后子单执行 ProcessOrder，主单状态改为 99(拆单状态)
        //3、如果是分批提交，那么把当前订单生成一个子单，然后子单执行 ProcessOrder，主单状态改为 99(拆单状态)  2和3处理逻辑基本一样
        /***
         拆单规格：
            1、把当前订单主表和从表的数据复制到一个新订单，关系插入到关联表 ErpOrderRelationEntity
            2、把当前订单从表的分拣状态重置（Num1,SorterState,SorterTime,SorterUserId）,如果是整单提交，主单状态更新为 99(拆单状态)
            3、把出库单主表和从表id替换为子单的id （ErpOutorderEntity，ErpOutrecordEntity）出库单的主表id和从表id、从表Outid 都是为订单明细id
            4、更新出入库关联表的出库id （ErpOutdetailRecordEntity）关联表Outid为订单明细id
            5、所有子单的状态为"已收货，待结算"（OrderStateEnum.Receiving），那么更新主单的状态为"已收货，待结算"（OrderStateEnum.Receiving）,
                并且汇总子单的 分拣数，复合数，分拣金额，复合金额，退货数量到主单，同时更新主单的金额（在ProcessOrder里面处理）
         */

        // 待分拣明细
        var waitList = await HandleSubmitCheck(id);
        input ??= new List<ErpOrderFjWaitListOutput>();
        // 获取当前订单数据
        var order = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var orderDetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid.Equals(id)).ToListAsync();

        //判断数据是否有变动
        // 所有单品都分拣完，并且 input是所有的明细
        bool isAll = waitList.done; //是否整单提交
        List<ErpOrderdetailEntity> newOrderDetailList = new List<ErpOrderdetailEntity>();
        foreach (var item in orderDetailList)
        {
            var xitem = input.Find(x => x.id == item.Id);
            if (xitem != null)
            {
                // 单品未分拣或者实际分拣数量不一致
                if (item.SorterState != "1" || xitem.num1 != item.Num1)
                {
                    throw Oops.Oh("数据有变动，请稍后再试！");
                }
                // 新明细id在下面赋值
                ErpOrderdetailEntity erpOrderdetailEntity = new ErpOrderdetailEntity();
                item.Adapt(erpOrderdetailEntity);
                newOrderDetailList.Add(erpOrderdetailEntity);
            }
            //else
            //{
            //    var entity = waitList.Find(x => x.id == item.Id);
            //    if (entity!=null && entity.wnum > 0)
            //    {
            //        isAll = false;
            //        continue;
            //    }
            //}
        }

        // 是否有子单
        var childrenCount = await _repository.Context.Queryable<ErpOrderRelationEntity>().Where(x => x.Oid == order.Id && x.Type == nameof(ErpOrderEntity)).CountAsync();
        if (isAll && childrenCount == 0)
        {
            //1、如果是整单提交，并且没有拆过子单，那么按照原来的方式执行 ProcessOrder
            await erpOrderService.ProcessOrder(order.Id, OrderStateEnum.Picked);
            return;
        }

        if (!newOrderDetailList.IsAny())
        {
            throw Oops.Oh("数据有变动，请稍后再试！");
        }

        /***
         拆单规格：
            1、把当前订单主表和从表的数据复制到一个新订单，关系插入到关联表 ErpOrderRelationEntity
            2、把当前订单从表的分拣状态重置（Num1,SorterState,SorterTime,SorterUserId）,如果是整单提交，主单状态更新为 99(拆单状态)
            3、把出库单主表和从表id替换为子单的id （ErpOutorderEntity，ErpOutrecordEntity）出库单的主表id和从表id、从表Outid 都是为订单明细id
            4、更新出入库关联表的出库id （ErpOutdetailRecordEntity）关联表Outid为订单明细id
         */
        #region 拆单
        List<ErpOrderRelationEntity> erpOrderRelationEntities = new List<ErpOrderRelationEntity>();
        // 拆分子单
        var newOrder = new ErpOrderEntity();
        order.Adapt(newOrder);
        newOrder.Id = SnowflakeIdHelper.NextId();
        newOrder.No = $"{order.No}-{(childrenCount + 1).ToString().PadLeft(2, '0')}";
        newOrder.LastModifyTime = DateTime.Now;
        newOrder.LastModifyUserId = _userManager.UserId;
        erpOrderRelationEntities.Add(new ErpOrderRelationEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            Oid = order.Id,
            Cid = newOrder.Id,
            Type = nameof(ErpOrderEntity)
        });

        List<string> oldOrderDetailIdList = newOrderDetailList.Select(x => x.Id).ToList();
        // 找出出库记录（ErpOutorderEntity，ErpOutrecordEntity）
        var outorderList = await _repository.Context.Queryable<ErpOutorderEntity>().Where(x => oldOrderDetailIdList.Contains(x.Id)).ToListAsync();
        var outrecordList = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(x => oldOrderDetailIdList.Contains(x.Id)).ToListAsync();
        var outdetailRecordEntities = await _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(x => oldOrderDetailIdList.Contains(x.OutId)).ToListAsync();

        newOrderDetailList.ForEach(x =>
        {
            var oldId = x.Id;
            var newId = SnowflakeIdHelper.NextId();

            outorderList.FindAll(w => w.Id == oldId)?.ForEach(w => w.Id = newId);
            outrecordList.FindAll(w => w.Id == oldId)?.ForEach(w =>
            {
                w.Id = newId;
                w.OrderId = newId;
                w.OutId = newId;
            });
            outdetailRecordEntities.FindAll(w => w.OutId == oldId)?.ForEach(w => w.OutId = newId);

            erpOrderRelationEntities.Add(new ErpOrderRelationEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                Oid = oldId,
                Cid = newId,
                Type = nameof(ErpOrderdetailEntity)
            });
            x.Id = newId;
            x.Fid = newOrder.Id;
            x.LastModifyUserId = _userManager.UserId;
            x.LastModifyTime = DateTime.Now;
        });

        // 更新子单主表的金额
        newOrder.Amount = newOrderDetailList.Sum(x => x.Amount1);
        newOrder.Amount1 = newOrderDetailList.Sum(x => x.Amount1);


        #endregion


        #region 更新数据
        //1、把当前订单主表和从表的数据复制到一个新订单，关系插入到关联表 ErpOrderRelationEntity
        await _repository.Context.Insertable<ErpOrderEntity>(newOrder).ExecuteCommandAsync();
        await _repository.Context.Insertable(newOrderDetailList).ExecuteCommandAsync();
        await _repository.Context.Insertable(erpOrderRelationEntities).ExecuteCommandAsync();

        //2、把当前订单从表的分拣状态重置（Num1,SorterState,SorterTime,SorterUserId）,如果是整单提交，主单状态更新为 99(拆单状态)
        // 重置当前订单明细的分拣状态
        await _repository.Context.Updateable<ErpOrderdetailEntity>()
            .SetColumns(it => new ErpOrderdetailEntity
            {
                Num1 = 0,
                FjNum = 0,
                SorterState = "",
                SorterTime = null,
                SorterUserId = ""
            }).Where(it => oldOrderDetailIdList.Contains(it.Id))
            .ExecuteCommandAsync();
        if (isAll)
        {
            await _repository.Context.Updateable<ErpOrderEntity>()
            .SetColumns(it => new ErpOrderEntity
            {
                State = OrderStateEnum.Split
            }).Where(it => it.Id.Equals(order.Id))
            .ExecuteCommandAsync();
        }

        //3、把出库单主表和从表id替换为子单的id （ErpOutorderEntity，ErpOutrecordEntity）出库单的主表id和从表id、从表Outid 都是为订单明细id
        await _repository.Context.Insertable<ErpOutorderEntity>(outorderList).ExecuteCommandAsync();
        await _repository.Context.Insertable<ErpOutrecordEntity>(outrecordList).ExecuteCommandAsync();
        await _repository.Context.Deleteable<ErpOutorderEntity>().Where(x => oldOrderDetailIdList.Contains(x.Id)).ExecuteCommandAsync();
        await _repository.Context.Deleteable<ErpOutrecordEntity>().Where(x => oldOrderDetailIdList.Contains(x.Id)).ExecuteCommandAsync();

        //4、更新出入库关联表的出库id （ErpOutdetailRecordEntity）关联表Outid为订单明细id
        await _repository.Context.Updateable<ErpOutdetailRecordEntity>(outdetailRecordEntities).UpdateColumns(x => x.OutId).ExecuteCommandAsync();
        #endregion

        // 提交订单（子单）
        await erpOrderService.ProcessOrder(newOrder.Id, OrderStateEnum.Picked);
    }

    /// <summary>
    /// 分拣批量完成
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("actions/complete")]
    public async Task BatchCompleted([FromBody] List<string> input)
    {
        if (input.IsAny())
        {
            var list = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => input.Contains(it.Id)).ToListAsync();

            foreach (var item in list)
            {
                _repository.Context.Tracking(item);
                item.SorterFinishTime = DateTime.Now;

            }
            await _repository.Context.Updateable(list).ExecuteCommandAsync();
        }
    }

    #region 私有方法

    /// <summary>
    /// 创建查询
    /// </summary>
    private ISugarQueryable<ErpOrderFjListOutput> CreateQuery(ErpOrderFjListQueryInput input)
    {
        List<string> cidList = new List<string>();
        if (input.tid.IsNotEmptyOrNull())
        {
            cidList = _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Type == input.tid).Select(x => x.Id).Take(100).ToList();

            cidList.Add(input.tid);
        }
        return _repository.Context.Queryable<ErpOrderEntity>()
             .Where(it => it.State == OrderStateEnum.PendingApproval)
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
             .WhereIF(input.diningType.IsNotEmptyOrNull(), it => it.DiningType == input.diningType)
             .WhereIF(cidList.IsAny(), it => cidList.Contains(it.Cid))
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
                 createUidName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.CreateUid).Select(d => d.RealName),
                 diningType = it.DiningType
             }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort);
    }
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
                    done = x.SorterFinishTime.HasValue || x.SorterState == "1" ? 1 : 0,
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


    /// <summary>
    /// 创建分类查询
    /// </summary>
    private ISugarQueryable<ErpOrderdetailInfoOutput> CreateCategoryQuery(string tid, ErpOrderFjListQueryInput input)
    {
        if (tid == "0")
        {
            tid = string.Empty;
        }
        return _repository.Context.Queryable<ErpOrderEntity>()
          .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
          .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
          .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
          .Where(a => a.State == OrderStateEnum.PendingApproval)
          .WhereIF(!input.type.HasValue || input.type == 0, (a, b) => (b.SorterState ?? "") != "1")
          .WhereIF(input.type == 1, (a, b) => true)
          //.Where((a, b) => (b.SorterState ?? "") != "1")
          .WhereIF(!string.IsNullOrEmpty(tid), (a, b, c, d) => SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == d.Tid && ddd.RootId == tid).Any())
          .WhereIF(input.beginDate.HasValue, a => a.CreateTime >= input.beginDate)
             .WhereIF(input.endDate.HasValue, a => a.CreateTime <= input.endDate)
             .WhereIF(input.createTime.HasValue, a => a.CreateTime >= input.createTime && a.CreateTime < input.createTime.Value.AddDays(1))
             .WhereIF(input.posttime.HasValue, a => a.Posttime >= input.posttime && a.Posttime < input.posttime.Value.AddDays(1))
             .WhereIF(!string.IsNullOrEmpty(input.no), a => a.No.Contains(input.no))
             .WhereIF(!string.IsNullOrEmpty(input.cid), a => a.Cid == input.cid)
             .WhereIF(!string.IsNullOrEmpty(input.keyword), (a, b, c, d) =>
                 a.No.Contains(input.keyword)
                 || a.Cid.Contains(input.keyword)
                 || c.Name.Contains(input.keyword)
                 || d.Name.Contains(input.keyword)
                 || SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == a.Cid && d.Name.Contains(input.keyword)).Any()
                 )
             .WhereIF(!string.IsNullOrEmpty(input.mid), (a, b) => b.Mid == input.mid)
           .Select((a, b, c, d) => new ErpOrderdetailInfoOutput
           {
               id = b.Id,
               amount = b.Amount,
               amount1 = b.Amount1,
               amount2 = b.Amount2,
               checkTime = b.CheckTime,
               mid = b.Mid,
               num = b.Num,
               num1 = b.Num1,
               num2 = b.Num2,
               receiveState = b.ReceiveState,
               receiveTime = b.ReceiveTime,
               remark = b.Remark ?? "",
               salePrice = b.SalePrice,
               sorterDes = b.SorterDes,
               sorterTime = b.SorterTime,
               sorterUserId = b.SorterUserId,
               productId = d.Id,
               midName = c.Name,
               productName = d.Name,
               sorterState = b.SorterState ?? "0",
               //rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName),
               midUnit = c.Unit,
               orderNo = a.No,
               customerUnit = c.CustomerUnit ?? c.Unit,
               customerName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == a.Cid).Select(ddd => ddd.Name),
               diningType = a.DiningType ?? "",
               posttime = a.Posttime
           }, true);
    }

    private async Task Wrapper(IEnumerable<ErpOrderdetailInfoOutput>? list)
    {
        // 加载图片
        if (list.IsAny())
        {
            var idList = list.Select(x => x.productId).Distinct().ToList();
            if (idList.IsAny())
            {
                //var sp = Stopwatch.StartNew();
                for (int i = idList.Count - 1; i >= 0; i--)
                {
                    var productId = idList[i];
                    var res = await _cacheManager.GetAsync<string[]>($"ErpProductpicEntity:{productId}");
                    if (res != null && res.Length > 0)
                    {
                        foreach (var item in list.Where(x => x.productId == productId))
                        {
                            item.imageList = res;
                        }
                        idList.RemoveAt(i);
                    }
                }

                if (idList.IsAny())
                {
                    var erpProductpicList = await _repository.Context.Queryable<ErpProductpicEntity>()
                                        .Where(it => idList.Contains(it.Pid)).Select(it => new ErpProductpicEntity
                                        {
                                            Id = it.Id,
                                            Pid = it.Pid,
                                            Pic = it.Pic
                                        }).ToListAsync();

                    foreach (var item in list)
                    {
                        if (item.imageList.IsAny())
                        {
                            continue;
                        }
                        var imageList = erpProductpicList.Where(it => it.Pid == item.productId)
                            .Select(it =>
                            {
                                var url = it.Pic;
                                if (!string.IsNullOrEmpty(url) && url.IndexOf("?") > -1)
                                {
                                    //url=url.Replace("?", $"?id={it.Id}&");
                                    url = url.Replace("?encryption=", "/");
                                }

                                return url;
                            }).ToArray();
                        item.imageList = imageList;
                        await _cacheManager.SetAsync($"ErpProductpicEntity:{item.productId}", item.imageList);
                    }
                }

                //sp.Stop();
                //Console.WriteLine("ErpProductpicEntity:Wrapper查询耗时：{0}毫秒", sp.ElapsedMilliseconds);
            }
            idList = list.Select(x => x.id).Distinct().ToList();
            if (idList.IsAny())
            {
                //var sp = Stopwatch.StartNew();
                // 子单汇总
                var childrens = await _repository.Context.Queryable<ErpOrderdetailEntity>()
                    .InnerJoin<ErpOrderRelationEntity>((a, b) => a.Id == b.Cid)
                    .Where((a, b) => idList.Contains(b.Oid) && b.Type == nameof(ErpOrderdetailEntity))
                    .GroupBy((a, b) => b.Oid)
                    .Select((a, b) => new
                    {
                        id = b.Oid,
                        cnum1 = SqlFunc.AggregateSum(a.Num1),
                        cfjNum = SqlFunc.AggregateSum(a.FjNum ?? 0)
                    })
                    .ToListAsync();

                foreach (var item in list)
                {
                    var xitem = childrens.Find(x => x.id == item.id);
                    if (xitem != null)
                    {
                        item.cnum1 = xitem.cnum1;
                        item.cfjNum = xitem.cfjNum;
                    }
                }
                //sp.Stop();
                //Console.WriteLine("ErpOrderdetailEntity:Wrapper查询耗时：{0}毫秒", sp.ElapsedMilliseconds);
            }

            //foreach (var item in list)
            //{
            //    var outlist = await _erpOrderTraceService.GetOrderOutList(item.id);
            //    if (outlist.IsAny())
            //    {
            //        var outdto = outlist.Where(x => x.productDate.IsNotEmptyOrNull()).OrderByDescending(x => x.id).FirstOrDefault();
            //        if (outdto!=null)
            //        {
            //            item.productDate = outdto.productDate;
            //            item.retention = outdto.retention;
            //        }
            //    }
            //}
        }
    }


    /// <summary>
    /// 获取订单待分拣列表
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<List<ErpOrderFjWaitListOutput>> GetOrderFjWaitList(string id)
    {
        // 获取当前订单列表的数据
        var list = await _repository.AsQueryable().InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
             .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
             .Where((a, b, c) => a.Id == id && a.State == OrderStateEnum.PendingApproval)
             .Select((a, b, c) => new ErpOrderFjWaitListOutput
             {
                 id = b.Id,
                 num = b.Num,
                 num1 = b.Num1,
                 fjNum = b.FjNum ?? 0,
                 sorterState = b.SorterState ?? "",
                 midName = c.Name,
                 unit = c.Unit,
                 customerUnit = c.CustomerUnit,
                 productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(ddd => ddd.Id == c.Pid).Select(ddd => ddd.Name),
                 sorterFinishTime = b.SorterFinishTime
             })
             .ToListAsync();

        var idList = list.Select(x => x.id).ToArray();

        // 子单汇总
        var childrens = await _repository.Context.Queryable<ErpOrderdetailEntity>()
            .InnerJoin<ErpOrderRelationEntity>((a, b) => a.Id == b.Cid)
            .Where((a, b) => idList.Contains(b.Oid) && b.Type == nameof(ErpOrderdetailEntity) && a.SorterState == "1")
            .GroupBy((a, b) => b.Oid)
            .Select((a, b) => new
            {
                id = b.Oid,
                cnum1 = SqlFunc.AggregateSum(a.Num1),
                cfjNum = SqlFunc.AggregateSum(a.FjNum ?? 0)
            })
            .ToListAsync();

        // 合并数据
        foreach (var item in list)
        {
            var xitem = childrens.Find(x => x.id == item.id);
            if (xitem != null)
            {
                item.cnum1 = xitem.cnum1;
                item.cfjNum = xitem.cfjNum;
            }
        }

        return list;
    }
    #endregion

    /// <summary>
    /// 子单取消
    /// </summary>
    /// <returns></returns>
    [HttpPost("actions/{id}/sub-cancel")]
    [SqlSugarUnitOfWork]
    public async Task SubOrderCancel(string id)
    {
        var entity = await _repository.Context.Queryable<ErpOrderEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        // 待分拣和待出库才能退回
        if (entity.State == OrderStateEnum.Picked || entity.State == OrderStateEnum.PendingApproval)
        {
            //var order = (await GetInfo(id)) as ErpOrderFjInfoOutput;
            var erpOrderdetailList = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => x.Fid == id).ToListAsync();
            var relation = await _repository.Context.Queryable<ErpOrderRelationEntity>().SingleAsync(x => x.Cid == id);
            List<string> relationIdList = new List<string>();
            relationIdList.Add(entity.Id);
            if (erpOrderdetailList.IsAny())
            {
                // 取消订单分拣，
                foreach (var item in erpOrderdetailList)
                {
                    if (item.SorterState == "1")
                    {
                        await CancelFJ(item.Id);
                    }

                    _repository.Context.Tracking(item);
                    item.Delete();

                    relationIdList.Add(item.Id);
                }
            }

            // 删除子单，删除子单关联 
            _repository.Context.Tracking(entity);
            entity.DeleteTime = DateTime.Now;
            entity.DeleteUserId = _userManager.UserId;

            if (relationIdList.IsAny())
            {
                await _repository.Context.Deleteable<ErpOrderRelationEntity>().Where(x => relationIdList.Contains(x.Cid)).ExecuteCommandAsync();
            }

            await _repository.Context.Updateable<ErpOrderEntity>(entity).ExecuteCommandAsync();
            await _repository.Context.Updateable<ErpOrderdetailEntity>(erpOrderdetailList).ExecuteCommandAsync();

            // 如果没有子单，更新主单的状态
            if (relation != null && !await _repository.Context.Queryable<ErpOrderRelationEntity>().AnyAsync(x => x.Oid == relation.Oid))
            {
                await _repository.Context.Updateable<ErpOrderEntity>()
                    .SetColumns(x => x.State == OrderStateEnum.PendingApproval)
                    .Where(x => x.Id == relation.Oid)
                    .ExecuteCommandAsync();

            }
        }
        else
        {
            throw Oops.Oh("订单状态异常！");
        }

    }

    /// <summary>
    /// 批量提交
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPost("actions/auditBatch")]
    [SqlSugarUnitOfWork]
    public async Task<int> AuditBatch([FromBody] List<string> ids, [FromServices] IErpOrderService erpOrderService)
    {
        int i = 0;
        foreach (var id in ids)
        {
            var result = await HandleSubmitCheck(id);
            if (!result.done)
            {
                //该订单还有明细未进行分拣，不能提交！
            }
            else
            {
                await HandleSubmit(id, result.list, erpOrderService);
                i++;
            }
        }
         

        return i;
    }
}
using Mapster;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp.BuyOrder;
using QT.JXC.Entitys.Entity.ERP.gz_cycs;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：采购任务订单.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpBuyorderCk", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpBuyorderCkService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpBuyorderEntity> _repository;

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
    private readonly IFileManager _fileManager;
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 初始化一个<see cref="ErpBuyorderService"/>类型的新实例.
    /// </summary>
    public ErpBuyorderCkService(
        ISqlSugarRepository<ErpBuyorderEntity> erpBuyorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IFileManager fileManager,
        IDictionaryDataService dictionaryDataService)
    {
        _repository = erpBuyorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _fileManager = fileManager;
        _dictionaryDataService = dictionaryDataService;
    }

    /// <summary>
    /// 获取采购任务订单列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpBuyorderListQueryInput input)
    {
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
        }

        var data = await _repository.Context.Queryable<ErpBuyorderEntity>()
            .Where(it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(ddd => ddd.Fid == it.Id && (ddd.WhetherProcess ?? 0) == 0).Count() > 0)
            //.Where(it => it.State != "0")
            .Where(it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && (d.BuyState ?? "0") != "0").Any())
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(queryTaskBuyTime != null, it => SqlFunc.Between(it.TaskBuyTime, startTaskBuyTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endTaskBuyTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                )
            .WhereIF(!string.IsNullOrEmpty(input.state), it => it.State == input.state)
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
            .WhereIF(input.channel.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && d.Channel == input.channel).Any())
            .WhereIF(input.payment.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && d.Payment == input.payment).Any())
            .WhereIF(suppliers1.IsAny(), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && suppliers1.Contains(d.Supplier)).Any())
            .WhereIF(productNames.IsAny(), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && productNames.Contains(d.Gid)).Any())
            .WhereIF(input.rkNo.IsNotEmptyOrNull(), it => it.RkNo.Contains(input.rkNo.Trim()))
            .WhereIF(input.auditUserName.IsNotEmptyOrNull(),it => SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.AuditUserId && d.RealName.Contains(input.auditUserName)).Any())
            .OrderBy(it => it.TaskBuyTime, OrderByType.Desc)
            .Select(it => new ErpBuyorderListOutput
            {
                id = it.Id,
                creatorTime = it.CreatorTime,
                //oid = it.Oid,
                no = it.No,
                taskToUserId = it.TaskToUserId,
                taskBuyTime = it.TaskBuyTime,
                taskRemark = it.TaskRemark,
                taskToUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.TaskToUserId).Select(d => d.RealName),
                state = it.State ?? "0",
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
                auditTime = it.AuditTime,
                auditUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.AuditUserId).Select(d => d.RealName),
                detailCount = SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && d.BuyState == "1").Count(),
                channel = SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id).Select(d => d.Channel),
                payment = SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id).Select(d => d.Payment),
                selfCheck = SqlFunc.IsNullOrEmpty(it.SelfReportProof) ? 0 : 1,
                hasQualityReport = SqlFunc.IsNullOrEmpty(it.QualityReportProof) ? 0 : 1,
                rkNo = !SqlFunc.IsNullOrEmpty(it.RkNo) ? it.RkNo : SqlFunc.Subqueryable<ErpInorderEntity>().Where(d => d.Id == it.Id).Select(d => d.No),
                amount = SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id).Sum(d => d.Amount),
                hasCycs = SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().InnerJoin<ErpCycsInEntity>((d,c)=>d.Id == c.buyId).Where(d => d.Fid == it.Id).Any() ? true : false,
            })
            .OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);

        var idList = data.list?.Select(x => x.id).ToList() ?? new List<string>();

        if (idList.IsAny())
        {
            var suppliers = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(it => idList.Contains(it.Fid))
           .Select(it => new
           {
               it.Fid,
               supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == it.Supplier).Select(x => x.Name),
           }).ToListAsync();


            foreach (var item in data.list)
            {
                var slist = suppliers.Where(it => it.Fid == item.id && !string.IsNullOrEmpty(it.supplierName)).Select(it => it.supplierName).ToList();
                item.supplierName = string.Join(",", slist.Distinct());
            }

            // 统计退回数量和金额
            var thList = await _repository.Context.Queryable<ErpOutorderEntity>()
                .InnerJoin<ErpOutrecordEntity>((a, b) => a.Id == b.OutId)
                .InnerJoin<ErpOutdetailRecordEntity>((a, b, c) => b.Id == c.OutId)
                .InnerJoin<ErpInrecordEntity>((a, b, c, d) => c.InId == d.Id)
                .InnerJoin<ErpBuyorderdetailEntity>((a, b, c, d, e) => d.Bid == e.Id)
                .Where((a, b, c, d, e) => a.InType == "5" && idList.Contains(e.Fid))
                .GroupBy((a, b, c, d, e) => new { d.Bid, e.Fid })
                .Select((a, b, c, d, e) => new
                {
                    fid = e.Fid,
                    bid = d.Bid,
                    thNum = SqlFunc.AggregateSum(c.Num),
                    inNum = e.InNum ?? 0,
                    tsNum = e.TsNum,
                    price = e.Price,
                    amount = e.Amount
                })
                .ToListAsync();

            foreach (var g in thList.GroupBy(x => x.fid))
            {
                var row = data.list.FirstOrDefault(z => z.id == g.Key);
                if (row!=null)
                {
                    row.thAmount = g.Sum(a => a.thNum == a.inNum + a.tsNum ? a.amount : a.thNum * a.price);
                    row.amount -= row.thAmount;
                }
            }
        }

      

        return PageResult<ErpBuyorderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 更新采购任务订单.
    /// 只更新入库数量
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpBuyorderCkUpInput input)
    {
        var entity = _repository.Single(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        //if (entity.State != "1")
        //{
        //    throw Oops.Oh("订单状态异常！");
        //}

        // 采购订单明细的入库数量
        var erpBuyorderdetailEntityList = input.erpBuyorderdetailList.Adapt<List<ErpBuyorderdetailEntity>>();
        if (erpBuyorderdetailEntityList != null)
        {
            await _repository.Context.Updateable(erpBuyorderdetailEntityList)
                .UpdateColumns(it => new { it.InNum, it.StoreRomeId, it.StoreRomeAreaId, it.Price, it.Amount, it.ProductionDate, it.Retention, it.BatchNumber })
                .Where(it => it.BuyState == "1").ExecuteCommandAsync();
        }
    }


    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("Proof/{id}")]
    public async Task<ErpBuyorderCkUpProofInput> GetProof(string id)
    {
        var entity = _repository.Single(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        return entity.Adapt<ErpBuyorderCkUpProofInput>();
    }

    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("Proof/{id}")]
    public async Task Proof(string id, [FromBody] ErpBuyorderCkUpProofInput input)
    {
        var entity = input.Adapt<ErpBuyorderEntity>();

        await _repository.Context.Updateable(entity).UpdateColumns(x => new { x.QualityReportProof, x.SelfReportProof }).ExecuteCommandAsync();
    }


    [HttpPost("Audit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Audit(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.State == "2")
        {
            throw Oops.Oh("订单已入库！");
        }

        if (entity.State != "1")
        {
            throw Oops.Oh("订单未完成采购！");
        }

        // 有审核日期，但是状态为1的记录先修复
        var templist = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
            .Where(x => x.Fid == entity.Id && x.BuyState == "1" && x.AuditTime.HasValue)
            .Select(x=> new ErpBuyorderdetailEntity
            {
                Id = x.Id,
                BuyState= x.BuyState,
                AuditTime = x.AuditTime
            })
            .ToListAsync();

        if (templist.IsAny())
        {
            // 判断入库记录是否存在，存在则提示先删除
            foreach (var item in templist)
            {
                var indata = await _repository.Context.Queryable<ErpInrecordEntity>().LeftJoin<ErpProductmodelEntity>((a, b) => a.Gid == b.Id)
                    .LeftJoin<ErpProductEntity>((a, b, c) => b.Pid == c.Id)
                    .InnerJoin<ErpInorderEntity>((a, b, c, d) => a.InId == d.Id)
                    .Where((a, b, c) => a.Id == item.Id)
                    .Select((a, b, c, d) => new
                    {
                        no = d.No,
                        name = c.Name
                    })
                    .FirstAsync();

                if (indata!=null)
                {
                    throw Oops.Oh($"请先删除入库单[{indata.no}]的[{indata.name}]再进行审核!");
                }

                item.BuyState = "2";

            }
            await _repository.Context.Updateable<ErpBuyorderdetailEntity>(templist).UpdateColumns(x => x.BuyState).ExecuteCommandAsync();
        }

        // 过滤未入库的记录
        var items = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Id && x.BuyState == "1" && !x.AuditTime.HasValue).ToListAsync();

        if (!items.IsAny())
        {
            // 所有明细都审核完毕，更新主表状态返回
            entity.State = "2";
            entity.AuditTime = DateTime.Now;
            entity.AuditUserId = _userManager.UserId;
            //更新订单状态为已入库
            await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State, it.AuditUserId, it.AuditTime }).ExecuteCommandAsync();
            //throw Oops.Oh("所有明细已入库！");

            return;
        }

        List<string> detailIdList = new List<string>();
        foreach (var item in items)
        {
            //入库审核中，实际入库数量为0的，设置不能审核订单
            var realNum = item.TsNum + (item.InNum ?? 0);
            if (realNum == 0)
            {
                throw Oops.Oh("实际入库数量为0，禁止审核！");
            }
            if (item.Num < item.TsNum + item.InNum)
            {
                //throw Oops.Oh("实际入库数量+特殊入库数量，大于实际采购数量，禁止操作！");
            }
            detailIdList.Add(item.Id);
        }

        var rkNo = entity.RkNo ?? await _billRullService.GetBillNumber("QTErpInOrder");
        // 判断id是否存在
        var newId = (await _repository.Context.Queryable<ErpInorderEntity>().Where(x => x.Id == entity.Id).AnyAsync()) ? SnowflakeIdHelper.NextId() : entity.Id;
        ErpInorderEntity erpInorderEntity = new ErpInorderEntity
        {
            Id = newId,
            No = rkNo,
            InType = "1",
            CgNo = entity.No,
            Oid = entity.Oid,
        };
        var erpInrecordEntityList = items.Select(x =>
        {
            var erpInrecordEntity = new ErpInrecordEntity
            {
                Id = x.Id,
                InId = erpInorderEntity.Id,
                Num = x.InNum ?? 0,
                InNum = x.InNum ?? 0,
                Oid = erpInorderEntity.Oid,
                Gid = x.Gid,
                Bid = x.Id,
                OrderNum = x.Num ?? 0,
                Price = x.Price,
                Amount = x.Amount,
                StoreRomeId = x.StoreRomeId,
                StoreRomeAreaId = x.StoreRomeAreaId,
                ProductionDate = x.ProductionDate,
                BatchNumber = x.BatchNumber,
                Retention = x.Retention
            };
            if (x.TsNum > 0)
            {
                erpInrecordEntity.Amount = Math.Round(erpInrecordEntity.InNum * erpInrecordEntity.Price, 2);
            }
            return erpInrecordEntity;

        }).ToList();

        // 更新主表的金额
        erpInorderEntity.Amount = erpInrecordEntityList.Sum(it => it.Amount);

        // 获取特殊入库的记录
        // 中间表，关联记录
        var relations = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(t => detailIdList.Contains(t.InId)).ToListAsync();
        var tsIdList = relations.Select(x => x.TsId).ToArray();
        // 所有的关联的特殊入库记录
        var tsListAll = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => tsIdList.Contains(x.Id)).ToListAsync();
        if (tsListAll.IsAny())
        {
            //// 合计特殊入库的金额 （关联采购入库单）
            var ts_amount_sum = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                .InnerJoin<ErpBuyorderdetailEntity>((a, b) => a.InId == b.Id)
                .Where((a, b) => tsIdList.Contains(a.TsId))
                .Select((a, b) => new
                {
                    a.TsId,  // 特殊入库明细id
                    a.Num,  // 关联特殊入库的数量
                    Amount = SqlFunc.Round(SqlFunc.IIF(a.Num == b.TsNum, b.Amount, a.Num * b.Price), 2),  // 特殊入库的金额
                    a.InId  // 采购明细id
                }).ToListAsync();

            _repository.Context.Tracking(tsListAll);
            foreach (var detail in items)
            {
                var temp = relations.Where(t => t.InId == detail.Id).Select(t => t.TsId).ToArray();
                if (!temp.IsAny())
                {
                    continue;
                }
                var inrecordEntity = erpInrecordEntityList.Find(x => x.Id == detail.Id);
                if (inrecordEntity == null)
                {
                    continue;
                }
                // 入库明细对应的特殊入库记录
                var tsList = tsListAll.Where(x => temp.Contains(x.Id)).ToList();
                if (tsList.IsAny())
                {
                    foreach (var xitem in tsList)
                    {
                        var total = ts_amount_sum.Where(x => x.TsId == xitem.Id).Sum(x => x.Amount);

                        xitem.Price = inrecordEntity.Price;
                        xitem.Amount = total; // Math.Round(inrecordEntity.Price * xitem.InNum, 2);
                        xitem.IsSpecial = "1";

                        xitem.Bid = inrecordEntity.Bid; // 关联采购单
                        //var totalNum = ts_amount_sum.Where(x => x.TsId == xitem.Id).Sum(x => x.Num);

                        //xitem.Amount = ts_amount_sum.Where(x => x.TsId == xitem.Id).Sum(x => x.Amount);
                        //xitem.Price = xitem.InNum > 0 ? Math.Round(xitem.Amount / xitem.InNum, 2) : 0;

                        //if (totalNum >= xitem.InNum)
                        //{
                        //    xitem.IsSpecial = "1";
                        //}
                    }
                    var total_ts_amount = ts_amount_sum.Where(x => x.InId == detail.Id).Sum(x => x.Amount);
                    // 计算最后的金额
                    //var diff = detail.Amount - (tsList.Sum(x => x.Amount) + inrecordEntity.Amount);
                    var diff = detail.Amount - (total_ts_amount + inrecordEntity.Amount);
                    if (diff != 0)
                    {
                        tsList[0].Amount += diff;
                    }
                }
            }
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        entity.State = "2";
        entity.AuditTime = DateTime.Now;
        entity.AuditUserId = _userManager.UserId;
        //更新订单状态为已入库
        await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State, it.AuditUserId, it.AuditTime }).ExecuteCommandAsync();

        if (string.IsNullOrEmpty(entity.RkNo))
        {
            entity.RkNo = rkNo;
            await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.RkNo }).ExecuteCommandAsync();
        }

        // 更新明细状态
        items.ForEach(x =>
        {
            _repository.Context.Tracking(x);
            x.BuyState = "2";
            x.AuditTime = DateTime.Now;
            x.AuditUserId = _userManager.UserId;
        });
        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(items).ExecuteCommandAsync();

        //生成采购入库订单
        await _repository.Context.Insertable(erpInorderEntity).ExecuteCommandAsync();
        await _repository.Context.Insertable(erpInrecordEntityList).ExecuteCommandAsync();


        // 更新特殊入库的单价和金额，并且更新完成状态
        if (tsListAll.IsAny())
        {
            // 合计特殊入库的金额
            var ts_amount_sum = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                .InnerJoin<ErpInrecordEntity>((a, b) => a.InId == b.Id)
                .Where((a, b) => tsIdList.Contains(a.TsId))
                .Select((a, b) => new
                {
                    a.TsId,
                    a.Num,
                    Amount = SqlFunc.IIF(a.Num == b.InNum, b.Amount, a.Num * b.Price)
                }).ToListAsync();

            foreach (var item in tsListAll)
            {
                var totalNum = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Num);

                //上面已经计算了金额
                //item.Amount = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Amount);
                item.Price = item.InNum > 0 ? Math.Round(item.Amount / item.InNum, 2) : 0;

                if (totalNum >= item.InNum)
                {
                    item.IsSpecial = "1";
                }
                else
                {
                    item.IsSpecial = "";
                }
            }

            await _repository.Context.Updateable(tsListAll).ExecuteCommandAsync();

            //更新特殊入库主表的完成状态
            var tsMainIdList = tsListAll.Select(x => x.InId).Distinct().ToArray();
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

        }

        // 更新出库成本
        await this.CalcOutrecordAmount(items?.Select(x => x.Gid)?.ToList());

        //    // 关闭事务
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
    /// 整单反审
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("UnAudit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task UnAudit(string id)
    {
        //1、判断订单是否存在
        //2、判断订单是否已审核
        //3、判断订单所有明细是否未出库
        //4、删除所有的入库明细
        //5、删除没有明细的入库单
        //6、更新订单以及明细的状态为未审核（主表State = 1，从表BuyState=1，清空审核时间、审核人）
        //7、删除所有明细对应的特殊入库，更新特殊入库的数量为0
        //8、退回到 已采购 的状态（state=1）

        //1、判断订单是否存在
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        //2、判断订单是否已审核
        if (!entity.AuditTime.HasValue)
        {
            throw Oops.Oh("订单未审核！");
        }

        // 找出所有的采购明细
        var items = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Id).ToListAsync();

        // 找出所有的入库明细
        var detailIds = items.Select(it => it.Id).ToArray();
        //3、判断订单所有明细是否未出库
        if (await _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(it => detailIds.Contains(it.InId)).AnyAsync())
        {
            throw Oops.Oh("明细已做出库记录，不允许反审");
        }


        var inrecordEntities = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => detailIds.Contains(x.Id)).ToListAsync();

        //找出所有出入库单明细
        var mainIds = inrecordEntities.Select(it => it.InId).Distinct().ToArray();

        // 同一份入库单，还有其他入库明细的记录
        var mains = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => mainIds.Contains(x.InId) && !detailIds.Contains(x.Id)).Select(it => it.InId).ToListAsync();
        if (mains.IsAny())
        {
            mainIds = mainIds.Except(mains).ToArray();
        }

        // 找出所有的特殊入库记录
        var tsEntities = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => detailIds.Contains(x.InId)).ToListAsync();

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //4、删除所有的入库明细
        await _repository.Context.Deleteable<ErpInrecordEntity>(inrecordEntities).ExecuteCommandAsync();

        //5、删除没有明细的入库单
        if (mainIds.IsAny())
        {
            await _repository.Context.Deleteable<ErpInorderEntity>(mainIds).ExecuteCommandAsync();
        }

        //6、更新订单以及明细的状态为未审核（主表State = 1，从表BuyState=1，清空审核时间、审核人）
        entity.State = "1";
        entity.AuditTime = null;
        entity.AuditUserId = string.Empty;
        //更新订单状态为已采购
        await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State, it.AuditUserId, it.AuditTime }).ExecuteCommandAsync();

        // 更新明细状态
        items.ForEach(x =>
        {
            x.BuyState = "1";
            x.AuditTime = null;
            x.AuditUserId = string.Empty;
        });
        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(items).UpdateColumns(it => new { it.BuyState, it.AuditUserId, it.AuditTime }).ExecuteCommandAsync();

        //7、删除所有明细对应的特殊入库，更新特殊入库的数量为0
        if (tsEntities.IsAny())
        {
            await _repository.Context.Deleteable<ErpInrecordTsEntity>(tsEntities).ExecuteCommandAsync();

            var mainIdList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => tsEntities.Any(t => t.TsId == x.Id)).Select(x => x.InId).ToListAsync();
            await _repository.Context.Updateable<ErpInrecordEntity>()
                     .SetColumns(x => new ErpInrecordEntity
                     {
                         IsSpecial = ""
                     })
                     .Where(x => tsEntities.Any(t => t.TsId == x.Id))
                     .ExecuteCommandAsync();

            await _repository.Context.Updateable<ErpInorderEntity>()
                    .SetColumns(x => new ErpInorderEntity
                    {
                        SpecialState = ""
                    })
                    .Where(x => mainIdList.Contains(x.Id))
                    .ExecuteCommandAsync();
        }
        items.ForEach(x =>
        {
            x.BuyState = "1";
            x.InNum = 0;
            x.TsNum = 0;
        });
        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(items).UpdateColumns(it => new { it.BuyState, it.TsNum, it.InNum }).ExecuteCommandAsync();

        //8、退回到已采购的状态（state = 1）
        entity.State = "1";
        //更新订单状态为已采购
        await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State }).ExecuteCommandAsync();

        //    // 关闭事务
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
    /// 明细反审
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("Detail/UnAudit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task DetailUnAudit(string id)
    {
        //1、判断订单明细是否存在
        //2、判断订单明细是否已审核
        //3、判断订单明细是否未出库
        //4、删除入库明细
        //5、删除没有明细的入库单
        //6、更新入库单的合计金额
        //7、更新订单以及订单明细的状态为未审核（从表BuyState=1，清空审核时间、审核人）


        //1、判断订单明细是否存在
        var entity = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        //2、判断订单明细是否已审核
        if (!entity.AuditTime.HasValue)
        {
            throw Oops.Oh("订单未审核！");
        }

        //// 找出所有的采购明细
        //var items = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Id).ToListAsync();

        //// 找出所有的入库明细
        //var detailIds = items.Select(it => it.Id).ToArray();

        var detailIds = new string[] { entity.Id };
        //3、判断订单明细是否未出库
        if (await _repository.Context.Queryable<ErpOutdetailRecordEntity>().Where(it => detailIds.Contains(it.InId)).AnyAsync())
        {
            throw Oops.Oh("明细已做出库记录，不允许反审");
        }


        var inrecordEntity = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => detailIds.Contains(x.Id)).FirstAsync();

        //找出订单明细对应的出入库单明细
        var mainIds = new string[] { inrecordEntity.InId }; // inrecordEntity.Select(it => it.InId).Distinct().ToArray();

        // 找出订单明细对应的入库单的其他明细记录
        var mains = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => mainIds.Contains(x.InId) && !detailIds.Contains(x.Id))
            .Select(x => new ErpInrecordEntity
            {
                Id = x.Id,
                Amount = x.Amount
            })
            .ToListAsync();

        //if (mains.IsAny())
        //{
        //    mainIds = mainIds.Except(mains).ToArray();
        //}

        // 找出所有的特殊入库记录
        var tsEntities = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => detailIds.Contains(x.InId)).ToListAsync();

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //4、删除所有的入库明细
        await _repository.Context.Deleteable<ErpInrecordEntity>(inrecordEntity).ExecuteCommandAsync();

        //5、删除没有明细的入库单
        if (mains.IsAny())
        {
            //6、更新入库单的合计金额
            ErpInorderEntity erpInorderEntity = new ErpInorderEntity
            {
                Id = inrecordEntity.InId,
                Amount = mains.Sum(x => x.Amount)
            };
            await _repository.Context.Updateable<ErpInorderEntity>(erpInorderEntity).UpdateColumns(it => new { it.Amount }).ExecuteCommandAsync();
        }
        else
        {
            await _repository.Context.Deleteable<ErpInorderEntity>(mainIds).ExecuteCommandAsync();
        }

        //7、更新订单以及订单明细的状态为未审核（从表BuyState=1，清空审核时间、审核人）
        var mainEntity = await _repository.SingleAsync(x => x.Id == entity.Fid);
        if (mainEntity != null)
        {
            _repository.Context.Tracking(mainEntity);
            mainEntity.State = "1";
            mainEntity.AuditTime = null;
            mainEntity.AuditUserId = string.Empty;
            //更新订单状态为已采购
            await _repository.Context.Updateable<ErpBuyorderEntity>(mainEntity).ExecuteCommandAsync();
        }


        // 更新明细状态
        entity.BuyState = "1";
        entity.AuditTime = null;
        entity.AuditUserId = string.Empty;

        //8、删除所有明细对应的特殊入库，更新特殊入库的数量为0
        entity.InNum = 0;
        entity.TsNum = 0;
        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(entity).UpdateColumns(it => new { it.BuyState, it.AuditUserId, it.AuditTime, it.TsNum, it.InNum }).ExecuteCommandAsync();

        //8、删除所有明细对应的特殊入库，更新特殊入库的数量为0
        if (tsEntities.IsAny())
        {
            await _repository.Context.Deleteable<ErpInrecordTsEntity>(tsEntities).ExecuteCommandAsync();

            var mainIdList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => tsEntities.Any(t => t.TsId == x.Id)).Select(x => x.InId).ToListAsync();
            await _repository.Context.Updateable<ErpInrecordEntity>()
                     .SetColumns(x => new ErpInrecordEntity
                     {
                         IsSpecial = ""
                     })
                     .Where(x => tsEntities.Any(t => t.TsId == x.Id))
                     .ExecuteCommandAsync();

            await _repository.Context.Updateable<ErpInorderEntity>()
                    .SetColumns(x => new ErpInorderEntity
                    {
                        SpecialState = ""
                    })
                    .Where(x => mainIdList.Contains(x.Id))
                    .ExecuteCommandAsync();
        }
        //await _repository.Context.Updateable<ErpBuyorderdetailEntity>(items).UpdateColumns(it => new { it.BuyState, it.TsNum, it.InNum }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 明细审核入库，带明细数据一起审核
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Detail/Audit/v2/{id}")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> DetailAudit(string id, [FromBody] ErpBuyorderdetailCkUpInput input)
    {
        var erpBuyorderdetailEntity = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (erpBuyorderdetailEntity.BuyState != "1")
        {
            throw Oops.Oh("数据异常，请刷新后再操作!");
        }

        if (erpBuyorderdetailEntity.AuditTime.HasValue && erpBuyorderdetailEntity.BuyState !="2")
        {
            var indata = await _repository.Context.Queryable<ErpInrecordEntity>().LeftJoin<ErpProductmodelEntity>((a, b) => a.Gid == b.Id)
                    .LeftJoin<ErpProductEntity>((a, b, c) => b.Pid == c.Id)
                    .InnerJoin<ErpInorderEntity>((a, b, c, d) => a.InId == d.Id)
                    .Where((a, b, c) => a.Id == id)
                    .Select((a, b, c, d) => new
                    {
                        no = d.No,
                        name = c.Name
                    })
                    .FirstAsync();

            if (indata != null)
            {
                throw Oops.Oh($"请先删除入库单[{indata.no}]的[{indata.name}]再进行审核!");
            }
        }
        

        _repository.Context.Tracking(erpBuyorderdetailEntity);

        // 更新采购订单明细的入库数量
        input.Adapt(erpBuyorderdetailEntity);

        // 确定单价
        var num = (erpBuyorderdetailEntity.InNum ?? 0) + erpBuyorderdetailEntity.TsNum;
        if (num > 0)
        {
            erpBuyorderdetailEntity.Price = Math.Round(erpBuyorderdetailEntity.Amount / num, 2);
        }

        //入库审核中，实际入库数量为0的，设置不能审核订单
        if (num == 0)
        {
            throw Oops.Oh("实际入库数量为0，禁止审核！");
        }

        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(erpBuyorderdetailEntity).ExecuteCommandAsync();
        //if (erpBuyorderdetailEntity != null)
        //{
        //    await _repository.Context.Updateable<ErpBuyorderdetailEntity>(erpBuyorderdetailEntity)
        //        .UpdateColumns(it => new { it.InNum, it.StoreRomeId, it.StoreRomeAreaId, it.Price, it.Amount, it.ProductionDate, it.Retention, it.BatchNumber })
        //        .Where(it => it.BuyState == "1").ExecuteCommandAsync();
        //}

        await DetailAudit(id);

        // 判断是否整单完成
        if (await _repository.AsQueryable().Where(x => x.Id == erpBuyorderdetailEntity.Fid && x.AuditTime.HasValue).AnyAsync())
        {
            return null;
        }
        else
        {
            return await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
                                    .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
                                    .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
                                    .Where(w => w.Id == id) // && (w.WhetherProcess??0)!=1
                                    .Select((w, a, b) => new ErpBuyorderdetailInfoOutput
                                    {
                                        gidName = a.Name,
                                        productName = b.Name,
                                        supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == w.Supplier).Select(x => x.Name),
                                        buyState = w.BuyState ?? "0",
                                        whetherProcess = w.WhetherProcess == 1,
                                        unit = a.Unit,
                                        rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName),
                                        tsNum = w.TsNum
                                    }, true)
                                    .FirstAsync();
        }
    }

    /// <summary>
    /// 明细审核入库
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("Detail/Audit/{id}")]
    [SqlSugarUnitOfWork]
    public async Task DetailAudit(string id)
    {
        var entity = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.BuyState == "2")
        {
            throw Oops.Oh("订单明细已入库！");
        }

        if (entity.BuyState != "1")
        {
            throw Oops.Oh("订单明细未完成采购！");
        }

        if (entity.Num < entity.TsNum + entity.InNum)
        {
            //throw Oops.Oh("实际入库数量+特殊入库数量，大于实际采购数量，禁止操作！");
        }

        //入库审核中，实际入库数量为0的，设置不能审核订单
        var realNum = entity.TsNum + (entity.InNum ?? 0);
        if (realNum == 0)
        {
            throw Oops.Oh("实际入库数量为0，禁止审核！");
        }

        // 
        //var items = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Id).ToListAsync();

        var main = await _repository.SingleAsync(x => x.Id == entity.Fid) ?? throw Oops.Oh(ErrorCode.COM1005);

        var rkNo = main.RkNo ?? await _billRullService.GetBillNumber("QTErpInOrder");
        ErpInorderEntity erpInorderEntity = new ErpInorderEntity
        {
            Id = SnowflakeIdHelper.NextId(), // entity.Id,
            No = rkNo,
            InType = "1",
            CgNo = main.No,
            Oid = main.Oid,
        };
        var erpInrecordEntityList = new ErpInrecordEntity
        {
            Id = entity.Id,
            InId = erpInorderEntity.Id,
            Num = entity.InNum ?? 0,
            InNum = entity.InNum ?? 0,
            Oid = erpInorderEntity.Oid,
            Gid = entity.Gid,
            Bid = entity.Id,
            OrderNum = entity.Num ?? 0,
            Price = entity.Price,
            Amount = entity.Amount,
            StoreRomeId = entity.StoreRomeId,
            StoreRomeAreaId = entity.StoreRomeAreaId,
            ProductionDate = entity.ProductionDate,
            BatchNumber = entity.BatchNumber,
            Retention = entity.Retention,
        };

        if (entity.TsNum > 0)
        {
            erpInrecordEntityList.Amount = Math.Round(erpInrecordEntityList.InNum * erpInrecordEntityList.Price, 2);
        }

        // 更新主表的金额
        erpInorderEntity.Amount = erpInrecordEntityList.Amount;


        // 获取特殊入库的记录
        var tsList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => SqlFunc.Subqueryable<ErpInrecordTsEntity>().Where(t => t.TsId == x.Id && t.InId == entity.Id).Any()).ToListAsync();
        if (tsList.IsAny())
        {
            var tsIdList = tsList.Select(x => x.Id).ToArray();
            //// 合计特殊入库的金额 （关联采购入库单）
            var ts_amount_sum = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                .InnerJoin<ErpBuyorderdetailEntity>((a, b) => a.InId == b.Id)
                .Where((a, b) => tsIdList.Contains(a.TsId))
                .Select((a, b) => new
                {
                    a.TsId,  // 特殊入库明细id
                    a.Num,  // 关联特殊入库的数量
                    Amount = SqlFunc.Round( SqlFunc.IIF(a.Num == b.TsNum, b.Amount, a.Num * b.Price),2),  // 特殊入库的金额
                    a.InId  // 采购明细id
                }).ToListAsync();

            //ts_amount_sum.Add(())

            _repository.Context.Tracking(tsList);

            foreach (var item in tsList)
            {
                var total = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Amount);
                item.Price = erpInrecordEntityList.Price;
                item.Amount = total; // Math.Round(erpInrecordEntityList.Price * item.InNum, 2);
                item.IsSpecial = "1";

                item.Bid = erpInrecordEntityList.Bid; // 采购明细id
                                                      //var totalNum = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Num);

                //item.Amount = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Amount);
                //item.Price = item.InNum > 0 ? Math.Round(item.Amount / item.InNum, 2) : 0;

                //if (totalNum >= item.InNum)
                //{
                //    item.IsSpecial = "1";
                //}
            }

            // 采购入库的特殊入库的金额 == 特殊入库中间表的金额汇总
            // cgts_amount = curTsAmount

            var total_ts_amount = ts_amount_sum.Where(x => x.InId == entity.Id).Sum(x => x.Amount);
            // 计算最后的金额
            //var diff = total_ts_amount - (tsList.Sum(x => x.Amount) /*+ erpInrecordEntityList.Amount*/);
            var diff = entity.Amount - (total_ts_amount + erpInrecordEntityList.Amount);
            if (diff != 0)
            {
                tsList[0].Amount += diff;
            }

        }
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        entity.BuyState = "2";
        entity.AuditTime = DateTime.Now;
        entity.AuditUserId = _userManager.UserId;
        //更新订单明细状态为已入库
        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(entity).UpdateColumns(it => new { it.BuyState, it.AuditUserId, it.AuditTime }).ExecuteCommandAsync();

        //判断是否整单完成入库
        if (!await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Fid && (x.BuyState ?? "0") != "2").AnyAsync())
        {
            main.State = "2";
            main.AuditTime = DateTime.Now;
            main.AuditUserId = _userManager.UserId;
            await _repository.Context.Updateable(main).UpdateColumns(it => new { it.State, it.AuditUserId, it.AuditTime }).ExecuteCommandAsync();
        }

        if (string.IsNullOrEmpty(main.RkNo))
        {
            main.RkNo = rkNo;
            await _repository.Context.Updateable<ErpBuyorderEntity>(main).UpdateColumns(it => new { it.RkNo }).ExecuteCommandAsync();
        }

        //生成采购入库订单
        await _repository.Context.Insertable(erpInorderEntity).ExecuteCommandAsync();
        await _repository.Context.Insertable(erpInrecordEntityList).ExecuteCommandAsync();

        // 更新特殊入库的单价和金额，并且更新完成状态
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
                    Amount = SqlFunc.IIF(a.Num == b.InNum, b.Amount, a.Num * b.Price)
                }).ToListAsync();

            foreach (var item in tsList)
            {
                var totalNum = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Num);
                //上面已经计算了金额
                //item.Amount = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Amount);
                item.Price = item.InNum > 0 ? Math.Round(item.Amount / item.InNum, 2) : 0;

                if (totalNum >= item.InNum)
                {
                    item.IsSpecial = "1";
                }
                else
                {
                    item.IsSpecial = "";
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

        }

        // 更新出库成本
        await CalcOutrecordAmount(new List<string> { entity.Gid });

        //    // 关闭事务
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
    /// 根据入库明细id获取待选记录
    /// </summary>
    /// <param name="inid">入库明细id</param>
    /// <param name="mid">规格id</param>
    /// <returns></returns>
    [HttpGet("Actions/Transfer/{inid}/{mid}")]
    [Obsolete]
    public async Task<dynamic> GetTransferList(string inid, string mid)
    {
        // 当前选中的记录
        var value = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId == inid).Select(x => x.TsId).ToListAsync();
        var list = await _repository.Context.Queryable<ErpInorderEntity>()
            .InnerJoin<ErpInrecordEntity>((a, b) => a.Id == b.InId)
            .Where((a, b) => a.InType == "5" /*&& (a.SpecialState ?? "") == ""*/ && b.Gid == mid)  // 未完成的特殊入库记录
            .Where((a, b) => SqlFunc.Subqueryable<ErpInrecordTsEntity>().Where(ddd => ddd.TsId == b.Id).NotAny() || value.Contains(b.Id))
            .OrderBy((a, b) => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new
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
                       posttime = c.Posttime
                   }).ToListAsync();


        var nlist = list.Select(it => new
        {
            it.id,
            it.num,
            creatorTime = relationOrder.FirstOrDefault(x => x.id == it.id)?.posttime,
            relationOrder.FirstOrDefault(x => x.id == it.id)?.no,
        }).ToList();

        return new
        {
            value = value ?? new List<string>(),
            data = nlist,
        };
    }


    /// <summary>
    /// 根据入库明细id获取待选记录
    /// </summary>
    /// <param name="inid">入库明细id</param>
    /// <param name="mid">规格id</param>
    /// <returns></returns>
    [HttpGet("Actions/Transfer/v2/{inid}/{mid}")]
    public async Task<dynamic> GetTransferListV2(string inid, string mid)
    {
        var qur = _repository.Context.Queryable<ErpInrecordTsEntity>().GroupBy(c => c.TsId).Select(c => new
        {
            c.TsId,
            Num = SqlFunc.AggregateSum(c.Num)
        });
        // 当前选中的记录
        var value = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId == inid).Select(x => x.TsId).ToListAsync();
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
                       customerName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == c.Cid).Select(ddd => ddd.Name),
                       customerType = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(ddd => ddd.Id == c.Cid).Select(ddd => ddd.Type)
                   }).ToListAsync();

        var customTypeOptions = await _dictionaryDataService.GetList("QTErpCustomType");

        foreach (var item in relationOrder)
        {
            var xitem = list.FirstOrDefault(x => x.id == item.id);
            if (xitem != null)
            {
                xitem.posttime = item.posttime;
                xitem.orderNo = item.no;
                xitem.customerName = item.customerName;
                xitem.customerType = customTypeOptions.FirstOrDefault(x => x.EnCode == item.customerType)?.FullName;
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


        //var nlist = list.Select(it => new
        //{
        //    it.id,
        //    it.num,
        //    creatorTime = relationOrder.FirstOrDefault(x => x.id == it.id)?.posttime,
        //    no = relationOrder.FirstOrDefault(x => x.id == it.id)?.no,
        //}).ToList();

        return new
        {
            value = value ?? new List<string>(),
            data = list,
        };
    }

    /// <summary>
    /// 保存关联关系
    /// </summary>
    /// <param name="inid"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/Transfer/{inid}")]
    [Obsolete]
    public async Task<dynamic> PostTransferList(string inid, [FromBody] ErpInrecordTransferInput input)
    {
        // 获取采购记录
        var buydetail = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().InSingleAsync(inid) ?? throw Oops.Oh(ErrorCode.COM1005);
        _repository.Context.Tracking(buydetail);
        // 汇总特殊入库数量
        var arr = input.value ?? new List<string>();
        buydetail.TsNum = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => input.value.Contains(x.Id)).SumAsync(x => x.InNum);
        //decimal? inNum = null;
        if (buydetail.TsNum > buydetail.Num)
        {
            //throw Oops.Oh("特殊入库数量大于实际采购数量，禁止操作！");
        }
        else
        {
            if (!buydetail.InNum.HasValue)
            {
                //buydetail.InNum = buydetail.Num - buydetail.TsNum;
            }
        }

        // 更新实际采购入库数
        //buydetail.InNum = buydetail.Num - buydetail.TsNum;
        var num = (buydetail.InNum ?? 0) + buydetail.TsNum;
        buydetail.Price = num != 0 ? Math.Round(buydetail.Amount / num, 2) : 0;

        // 当前选中的记录
        var list = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId == inid).ToListAsync();

        var items = input.value?.Select(x => new ErpInrecordTsEntity
        {
            InId = inid,
            TsId = x
        }).ToList() ?? new List<ErpInrecordTsEntity>();

        if (items.IsAny())
        {
            foreach (var item in items)
            {
                var entity = list.Find(x => x.InId == item.InId && x.TsId == item.TsId);
                if (entity != null)
                {
                    item.Id = entity.Id;
                }
                else
                {
                    item.Id = SnowflakeIdHelper.NextId();
                }
            }
        }

        await _db.TranExecute(async () =>
        {
            await _repository.Context.Deleteable<ErpInrecordTsEntity>().Where(x => x.InId == inid).ExecuteCommandAsync();

            if (items.IsAny())
            {
                await _repository.Context.Insertable<ErpInrecordTsEntity>(items).ExecuteCommandAsync();
            }

            // 更新特殊入库的数量
            await _repository.Context.Updateable<ErpBuyorderdetailEntity>(buydetail).ExecuteCommandAsync();
        });

        return new
        {
            tsNum = buydetail.TsNum,
            price = buydetail.Price,
            //inNum = buydetail.InNum
        };
    }

    /// <summary>
    /// 保存关联关系
    /// </summary>
    /// <param name="inid"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Actions/Transfer/v2/{inid}")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> PostTransferListV2(string inid, [FromBody] ErpInrecordTransferInputV2 input)
    {
        // 本次关联的特殊入库id集合
        if (input.items.IsAny())
        {
            var tsids = input.items.Select(x => x.id).ToArray();
            // 1、判断累计关联特殊入库数量是否大于特殊入库数量
            var relate_tslist = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId != inid && tsids.Contains(x.Id)).Select(x => new ErpInrecordTransferInputV2Item
            {
                id = x.Id,
                num = x.Num
            }).ToListAsync();

            relate_tslist.AddRange(input.items);
            relate_tslist = relate_tslist.GroupBy(x => x.id).Select(x => new ErpInrecordTransferInputV2Item
            {
                id = x.Key,
                num = x.Sum(a => a.num)
            }).ToList();
            // 特殊入库记录
            var tsList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => tsids.Contains(x.Id)).ToListAsync();

            foreach (var item in tsList)
            {
                var xitem = relate_tslist.FirstOrDefault(x => x.id == item.Id);
                if (xitem != null && xitem.num > item.InNum)
                {
                    throw Oops.Oh("累计关联特殊入库数据大于特殊入库数量，禁止保存！");
                }
            }
        }



        // 获取采购记录
        var buydetail = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().InSingleAsync(inid) ?? throw Oops.Oh(ErrorCode.COM1005);
        _repository.Context.Tracking(buydetail);
        // 当前的入库数量
        if (input.inNum.HasValue)
        {
            buydetail.InNum = input.inNum;
        }
        // 汇总特殊入库数量
        //var arr = input.value ?? new List<string>();
        buydetail.TsNum = input.items.Sum(x => x.num); // await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => input.value.Contains(x.Id)).SumAsync(x => x.InNum);
        //decimal? inNum = null;
        if (buydetail.TsNum > buydetail.Num)
        {
            //throw Oops.Oh("特殊入库数量大于实际采购数量，禁止操作！");
        }
        else
        {
            if (!buydetail.InNum.HasValue)
            {
                //buydetail.InNum = buydetail.Num - buydetail.TsNum;
            }
        }

        // 更新实际采购入库数
        //buydetail.InNum = buydetail.Num - buydetail.TsNum;
        var num = (buydetail.InNum ?? 0) + buydetail.TsNum;
        buydetail.Price = num != 0 ? Math.Round(buydetail.Amount / num, 2) : 0;

        // 当前选中的记录
        var list = await _repository.Context.Queryable<ErpInrecordTsEntity>().Where(x => x.InId == inid).ToListAsync();

        var items = input.items?.Select(x => new ErpInrecordTsEntity
        {
            InId = inid,
            TsId = x.id,
            Num = x.num
        }).ToList() ?? new List<ErpInrecordTsEntity>();

        if (items.IsAny())
        {
            foreach (var item in items)
            {
                var entity = list.Find(x => x.InId == item.InId && x.TsId == item.TsId);
                if (entity != null)
                {
                    item.Id = entity.Id;
                }
                else
                {
                    item.Id = SnowflakeIdHelper.NextId();
                }
            }
        }

        await _repository.Context.Deleteable<ErpInrecordTsEntity>().Where(x => x.InId == inid).ExecuteCommandAsync();

        if (items.IsAny())
        {
            await _repository.Context.Insertable<ErpInrecordTsEntity>(items).ExecuteCommandAsync();
        }

        // 更新特殊入库的数量
        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(buydetail).ExecuteCommandAsync();

        return new
        {
            tsNum = buydetail.TsNum,
            price = buydetail.Price,
            //inNum = buydetail.InNum
        };
    }

    /// <summary>
    /// 导出Excel.
    /// 有固定的模板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpBuyorderListQueryInput input)
    {
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
        }

        List<string> ids = new List<string>();

        if (input.dataType == 2)
        {
            ids = input.items.Split(",", true).ToList();
        }
        else if (input.dataType == 0)
        {
            var tempData = await _repository.Context.Queryable<ErpBuyorderEntity>()
                .Where(it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && (d.BuyState ?? "0") != "0").Any())
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(queryTaskBuyTime != null, it => SqlFunc.Between(it.TaskBuyTime, startTaskBuyTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endTaskBuyTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                )
            .WhereIF(!string.IsNullOrEmpty(input.state), it => it.State == input.state)
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
            .WhereIF(input.channel.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && d.Channel == input.channel).Any())
            .WhereIF(input.payment.IsNotEmptyOrNull(), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && d.Payment == input.payment).Any())
            .WhereIF(suppliers1.IsAny(), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && suppliers1.Contains(d.Supplier)).Any())
            .WhereIF(productNames.IsAny(), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id && productNames.Contains(d.Gid)).Any())
            .WhereIF(input.rkNo.IsNotEmptyOrNull(), it => it.RkNo.Contains(input.rkNo.Trim()))
                .Select(it => new ErpBuyorderListOutput
                {
                    id = it.Id,
                    //no = it.No,
                    //taskToUserId = it.TaskToUserId,
                    //taskBuyTime = it.TaskBuyTime,
                    //taskRemark = it.TaskRemark,
                    //taskToUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.TaskToUserId).Select(d => d.RealName),
                    //state = it.State ?? "0",
                    //oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
                    //creatorTime = it.CreatorTime
                })
                .OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
                .ToPagedListAsync(input.currentPage, input.pageSize);

            if (tempData.list.IsAny())
            {
                ids = tempData.list.Select(it => it.id).ToList();
            }
            else
            {
                ids.Add("-1");
            }
        }

        var query = _repository.Context.Queryable<ErpBuyorderEntity>()
            .InnerJoin<ErpBuyorderdetailEntity>((it, a) => it.Id == a.Fid)
            .InnerJoin<ErpProductmodelEntity>((it, a, b) => a.Gid == b.Id)
            .LeftJoin<ErpProductEntity>((it, a, b, c) => b.Pid == c.Id)
            //.WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            //.WhereIF(queryTaskBuyTime != null, it => SqlFunc.Between(it.TaskBuyTime, startTaskBuyTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endTaskBuyTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            //.WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
            //    it.No.Contains(input.keyword)
            //    )
            //.WhereIF(!string.IsNullOrEmpty(input.state), it => it.State == input.state)
            //.WhereIF(!string.IsNullOrEmpty(input.taskToUserId), it => it.TaskToUserId == input.taskToUserId)
            //.WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
            .WhereIF(ids.Any(), it => ids.Contains(it.Id))
            .OrderBy(it => it.CreatorTime, OrderByType.Desc)
            .Select((it, a, b, c) => new ErpBuyorderCkExportDetailOutput //ErpBuyorderCkExportDetailOutput
            {
                id = a.Id,
                //no = it.No,
                //taskToUserId = it.TaskToUserId,
                taskBuyTime = it.TaskBuyTime.Value.ToString("yyyy-MM-dd"),
                //taskRemark = it.TaskRemark,
                taskToUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.TaskToUserId).Select(d => d.RealName),
                //state = it.State == "1" ? "已完成" : "未完成",
                //oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),

                amount = a.Amount,
                //buyTime = a.BuyTime,
                //channel = a.Channel,
                gidName = b.Name,
                //num = a.Num,
                payment = a.Payment,
                //planNum = a.PlanNum,
                price = a.Price,
                productName = c.Name,
                remark = a.Remark,
                supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == a.Supplier).Select(x => x.Name),
                //storeNum = a.StoreNum,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == c.Tid).Select(ddd => ddd.RootName),
                rkNo = it.RkNo,
                realInNum = (a.InNum ?? 0) + a.TsNum,
                gidUnit = b.Unit,
                storeRomeIdName = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(x => x.Id == a.StoreRomeId).Select(x => x.Name),
                auditUserName = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.AuditUserId).Select(x => x.RealName),
            });

        //var data = input.dataType == 0 ? await query.ToPageListAsync(input.currentPage, input.pageSize) : await query.ToListAsync();
        var data = await query.ToListAsync();

        // 统计退回数量
        var thList = await _repository.Context.Queryable<ErpOutorderEntity>()
            .InnerJoin<ErpOutrecordEntity>((a, b) => a.Id == b.OutId)
            .InnerJoin<ErpOutdetailRecordEntity>((a, b, c) => b.Id == c.OutId)
            .InnerJoin<ErpInrecordEntity>((a, b, c, d) => c.InId == d.Id)
            .InnerJoin<ErpBuyorderdetailEntity>((a,b,c,d,e)=> d.Bid == e.Id)
            .Where((a, b, c, d) => a.InType == "5")
            .WhereIF(ids.IsAny(),(a,b,c,d,e)=> ids.Contains(e.Fid))
            .GroupBy((a, b, c, d) => d.Bid)
            .Select((a, b, c, d) => new
            {
                bid = d.Bid,
                num = SqlFunc.AggregateSum(c.Num)
            })
            .ToListAsync();

        foreach (var item in thList)
        {
            var row = data.Find(x => x.id == item.bid);
            if (row != null)
            {
                row.thNum = item.num;
                row.thAmount = row.thNum == row.realInNum ? row.amount : Math.Round((row.thNum ?? 0) * (row.price ?? 0), 2);
                //row.realInNum -= (row.thNum ?? 0);
                //row.amount -= (row.thAmount ?? 0);
            }
        }

        var unitOptions = await _dictionaryDataService.GetList("JLDW");

        foreach (var item in data)
        {
            if (item.gidUnit.IsNotEmptyOrNull())
            {
                item.gidUnit = unitOptions.Find(x => x.EnCode == item.gidUnit)?.FullName ?? "";
            }
        }

        ExcelConfig excelconfig = ExcelConfig.Default($"入库审核明细_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xls");
        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);

        excelconfig.ColumnModel = Common.Extension.Extensions.GetExcelColumnModels<ErpBuyorderCkExportDetailOutput>();

        var fs = ExcelExportHelper<ErpBuyorderCkExportDetailOutput>.ExportMemoryStream(data, excelconfig);
        var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }


        //ExcelExportHelper<ErpBuyorderExportDetailOutput>.Export(data, excelconfig, addPath);

        //using FileStream stream = File.OpenRead(addPath);
        //var flag = await _fileManager.UploadFileByType(stream, _fileManager.GetPathByType(""), excelconfig.FileName);

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT" + "|" + _userManager.TenantId) };
    }

    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("DetailProof/{id}")]
    public async Task<ErpBuyorderCkUpProofInput> GetDetailProof(string id)
    {
        var entity = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        return entity.Adapt<ErpBuyorderCkUpProofInput>();
    }

    /// <summary>
    /// 更新采购任务订单质检报告和自检报告
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("DetailProof/{id}")]
    public async Task DetailProof(string id, [FromBody] ErpBuyorderCkUpProofInput input)
    {
        var entity = input.Adapt<ErpBuyorderdetailEntity>();

        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(entity).UpdateColumns(x => new { x.QualityReportProof }).ExecuteCommandAsync();
    }


    /// <summary>
    /// 更新出库单成本
    /// </summary>
    /// <param name="gidList">规格集合</param>
    /// <returns></returns>
    private async Task CalcOutrecordAmount(List<string> gidList)
    {
        /*
         * 
         * 
INSERT INTO temp_table(id,costamount)
SELECT t.F_OutId,SUM(t.CostAmount) AS costamount 
FROM 
(
SELECT t1.F_OutId,
case WHEN t1.F_Num = t3.F_InNum THEN t3.F_Amount
ELSE ROUND(t1.F_Num * t3.F_Price,2) END AS costamount
FROM erp_outdetailrecord t1 INNER JOIN erp_inrecord t3 ON t3.F_Id = t1.F_InId
) t 
GROUP BY t.F_OutId;

 
-- 更新语句

UPDATE erp_outrecord INNER JOIN temp_table ON erp_outrecord.F_Id = temp_table.id
SET erp_outrecord.F_CostAmount = temp_table.costamount 
WHERE erp_outrecord.F_CostAmount <> temp_table.costamount;
         */

        if (gidList.IsAny())
        {
            var qur = _repository.Context.Queryable<ErpOutdetailRecordEntity>()
                    .InnerJoin<ErpInrecordEntity>((t1, t3) => t3.Id == t1.InId)
                    .Where((t1, t3) => gidList.Contains(t3.Gid))
                    .GroupBy((t1, t3) => t1.OutId)
                    .Select((t1, t3) => new ErpOutrecordEntity
                    {
                        Id = t1.OutId,
                        CostAmount = SqlFunc.AggregateSum(SqlFunc.IIF(t1.Num == t3.InNum, t3.Amount, t1.Num * t3.Price))
                    });
                    //.Select((t1, t3) => new ErpOutrecordEntity
                    //{
                    //    Id = t1.OutId,
                    //    CostAmount = SqlFunc.IIF(t1.Num == t3.InNum, t3.Amount, Math.Round(t1.Num * t3.Price, 2))
                    //})
                    //.MergeTable()
                    //.GroupBy(a => a.Id)
                    //.Select(a => new ErpOutrecordEntity
                    //{
                    //    Id = a.Id,
                    //    CostAmount = SqlFunc.AggregateSum(a.CostAmount)
                    //});
            var tempList = await qur.ToListAsync();

            if (tempList.IsAny())
            {
                // 只更新最近100条
                tempList = tempList.OrderByDescending(x => x.Id).Take(100).ToList();
                var upr = _repository.Context.Updateable<ErpOutrecordEntity>(tempList.ToArray()).UpdateColumns(x => new { x.CostAmount });
                await _repository.Ado.ExecuteCommandAsync(upr.ToSqlString());
            }
        }
    }

    [HttpGet("Actions/AuditNames")]
    public async Task<List<string>> GetAuditNames()
    {
        var usernames = await _repository.Context.Queryable<ErpBuyorderEntity>()
            .InnerJoin<UserEntity>((a, b) => a.AuditUserId == b.Id)
            .Where((a,b)=>a.Oid == _userManager.CompanyId)
            .Where((a,b) => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == a.Id && (d.BuyState ?? "0") != "0").Any())
            .GroupBy((a, b) => b.RealName)
            .Select((a, b) => b.RealName)
            .ToListAsync();

        return usernames;
    }
}
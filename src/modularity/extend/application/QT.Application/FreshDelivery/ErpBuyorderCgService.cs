using MiniExcelLibs;
using QT.Application.Entitys.Dto.FreshDelivery.Base;
using QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;
using QT.Application.Entitys.FreshDelivery;
using QT.ClayObject.Extensions;
using QT.Common;
using QT.Common.Configuration;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Helper;
using QT.Common.Models;
using QT.Common.Models.NPOI;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Reflection.Extensions;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：采购任务订单.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "采购订单采购员", Name = "ErpBuyorderCg", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpBuyorderCgService : IDynamicApiController, ITransient
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

    /// <summary>
    /// 初始化一个<see cref="ErpBuyorderService"/>类型的新实例.
    /// </summary>
    public ErpBuyorderCgService(
        ISqlSugarRepository<ErpBuyorderEntity> erpBuyorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager, IFileManager fileManager)
    {
        _repository = erpBuyorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _fileManager = fileManager;
    }

    /// <summary>
    /// 获取采购任务订单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005)).Adapt<ErpBuyorderInfoOutput>();

        if (string.IsNullOrEmpty(output.rkNo))
        {
            output.rkNo = await _repository.Context.Queryable<ErpInorderEntity>().Where(d => d.Id == output.id).Select(d => d.No).FirstAsync();
        }


        var erpBuyorderdetailList = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
            .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
            .Where(w => w.Fid == output.id) // && (w.WhetherProcess??0)!=1
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
            .ToListAsync();
        output.erpBuyorderdetailList = erpBuyorderdetailList; //.Adapt<List<ErpBuyorderdetailInfoOutput>>();

        output.supplierName = string.Join(",", erpBuyorderdetailList.Where(x => !string.IsNullOrEmpty(x.supplierName)).Select(x => x.supplierName).Distinct());

        var users = await _repository.Context.Queryable<ErpBuyorderUserEntity>().Where(it => it.Bid == id).Select(it => it.Uid).ToListAsync();

        if (output.taskToUserId.IsAny())
        {
            users.AddRange(output.taskToUserId);
        }

        output.taskToUserId = users.Distinct().ToArray();

        //// 获取特殊入库的数量
        //var idList = erpBuyorderdetailList.Select(x => x.id).ToList();
        //var tsList = await _repository.Context.Queryable<ErpInrecordEntity>().ClearFilter()
        //    .InnerJoin<ErpInrecordTsEntity>((a, b) => a.Id == b.TsId)
        //    .Where((a, b) => idList.Contains(b.InId))
        //    .GroupBy((a, b) => b.InId)
        //    .Select((a, b) => new
        //    {
        //        b.InId,
        //        Num = SqlFunc.AggregateSum(a.InNum)
        //    })
        //    .ToListAsync();
        //if (tsList.IsAny())
        //{
        //    foreach (var item in output.erpBuyorderdetailList)
        //    {
        //        item.tsNum = tsList.Find(x => x.InId == item.id)?.Num ?? 0;
        //    }
        //}

        // 统计退回数量
        var thList = await _repository.Context.Queryable<ErpOutorderEntity>()
            .InnerJoin<ErpOutrecordEntity>((a, b) => a.Id == b.OutId)
            .InnerJoin<ErpOutdetailRecordEntity>((a, b, c) => b.Id == c.OutId)
            .InnerJoin<ErpInrecordEntity>((a, b, c, d) => c.InId == d.Id)
            .Where((a, b, c, d) => a.InType == "5" && output.erpBuyorderdetailList.Any(x => x.id == d.Bid))
            .GroupBy((a, b, c, d) => d.Bid)
            .Select((a, b, c, d) => new
            {
                bid = d.Bid,
                num = SqlFunc.AggregateSum(c.Num)
            })
            .ToListAsync();

        foreach (var item in thList)
        {
            var row = output.erpBuyorderdetailList.Find(x => x.id == item.bid);
            if (row!=null)
            {
                row.thNum = item.num;
            }
        }

        return output;
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
        var data = await _repository.Context.Queryable<ErpBuyorderEntity>()
            .Where(it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(ddd => ddd.Fid == it.Id && ((ddd.WhetherProcess ?? 0) == 0)).Count() > 0)
            //.Where(it => it.TaskToUserId == input.taskToUserId || string.IsNullOrEmpty(it.TaskToUserId) || SqlFunc.Subqueryable<ErpBuyorderUserEntity>().Where(xxx => xxx.Bid == it.Id && xxx.Uid == input.taskToUserId).Any())
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(queryTaskBuyTime != null, it => SqlFunc.Between(it.TaskBuyTime, startTaskBuyTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endTaskBuyTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword) ||
                SqlFunc.Subqueryable<ErpBuyorderdetailEntity>()
                    .Where(xxx => xxx.Fid == it.Id && SqlFunc.Subqueryable<ErpProductmodelEntity>()
                    .Where(ddd => ddd.Id == xxx.Gid && SqlFunc.Subqueryable<ErpProductEntity>()
                    .Where(eee => eee.Id == ddd.Pid && eee.Name.Contains(input.keyword)).Any()).Any()).Any()
                )
            .WhereIF(!string.IsNullOrEmpty(input.state), it => it.State == input.state)
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
            .WhereIF(!string.IsNullOrEmpty(input.channel), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(xxx => xxx.Fid == it.Id && xxx.Channel == input.channel).Any())
            .WhereIF(!string.IsNullOrEmpty(input.productName), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>()
            .Where(xxx => xxx.Fid == it.Id && SqlFunc.Subqueryable<ErpProductmodelEntity>()
            .Where(ddd => ddd.Id == xxx.Gid && SqlFunc.Subqueryable<ErpProductEntity>()
            .Where(eee => eee.Id == ddd.Pid && eee.Name.Contains(input.productName)).Any()).Any()).Any())
            .WhereIF(!string.IsNullOrEmpty(input.supplierId), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(xxx => xxx.Fid == it.Id && xxx.Supplier == input.supplierId).Any())
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
                amount = SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(d => d.Fid == it.Id).Sum(d => d.Amount)
            })
            .OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);

        // 获取采购员
        var idList = data.list?.Select(x => x.id).ToList() ?? new List<string>();

        if (idList.IsAny())
        {
            var users = await _repository.Context.Queryable<ErpBuyorderUserEntity>().Where(it => idList.Contains(it.Bid))
            .Select(it => new
            {
                it.Bid,
                Name = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.Uid).Select(d => d.RealName),
            }).ToListAsync();

            var suppliers = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(it => idList.Contains(it.Fid))
           .Select(it => new
           {
               it.Fid,
               supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == it.Supplier).Select(x => x.Name),
           }).ToListAsync();


            foreach (var item in data.list)
            {
                var list = users.Where(it => it.Bid == item.id).Select(it => it.Name).ToList();
                if (!string.IsNullOrEmpty(item.taskToUserName))
                {
                    list.Add(item.taskToUserName);
                }
                item.taskToUserName = string.Join(",", list.Distinct());

                var slist = suppliers.Where(it => it.Fid == item.id).Select(it => it.supplierName).ToList();
                item.supplierName = string.Join(",", slist.Distinct());
            }
        }

        return PageResult<ErpBuyorderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建采购任务订单.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpBuyorderCrInput input)
    {
        // 新增的时候根据供应商拆分采购数据
        //var erpBuyorderdetailEntityList = input.erpBuyorderdetailList.Adapt<List<ErpBuyorderdetailEntity>>() ?? new List<ErpBuyorderdetailEntity>();

        if (!input.erpBuyorderdetailList.IsAny())
        {
            throw Oops.Oh("缺少采购明细！");
        }

        // 过滤掉没有供应商 或者不是仓库加工的记录
        input.erpBuyorderdetailList = input.erpBuyorderdetailList.Where(x => !string.IsNullOrEmpty(x.supplier) || x.whetherProcess).ToList();
        if (!input.erpBuyorderdetailList.IsAny())
        {
            throw Oops.Oh("采购明细缺少供应商！");
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();


        // 供应商定价
        var supplierPriceList = await _repository.Context.Queryable<ErpSupplierpriceEntity>().Where(it => input.erpBuyorderdetailList.Any(x => x.supplier == it.SupplierId)).ToListAsync();
        var companyId = _userManager.CompanyId;
        foreach (var g in input.erpBuyorderdetailList.GroupBy(it => it.supplier))
        {
            // 判断供应商是否存在
            if (g.Key.IsNotEmptyOrNull())
            {
                if (! await _repository.Context.Queryable<ErpSupplierEntity>().Where(x => x.Id == g.Key && x.Oid == companyId).AnyAsync())
                {
                    throw Oops.Oh("当前页面数据异常，请刷新后再操作！");
                }
            }

            var entity = input.Adapt<ErpBuyorderEntity>();
            entity.Id = SnowflakeIdHelper.NextId();
            //entity.No = await _billRullService.GetBillNumber("QTErpBuyOrder");
            entity.State = "0"; //状态为0

            var erpBuyorderdetailEntityList = g.Adapt<List<ErpBuyorderdetailEntity>>() ?? new List<ErpBuyorderdetailEntity>();
            //判断是否有仓库加工
            entity.No = erpBuyorderdetailEntityList.Any(x => x.WhetherProcess == 1) ? await _billRullService.GetBillNumber("QTErpJG") : await _billRullService.GetBillNumber("QTErpBuyOrder");

            var newEntity = await _repository.Context.Insertable<ErpBuyorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();


            if (erpBuyorderdetailEntityList != null)
            {
                foreach (var item in erpBuyorderdetailEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Fid = newEntity.Id;

                    if (item.WhetherProcess == 1)
                    {
                        item.BuyState = "2";
                    }

                    var priceEntity = supplierPriceList.Find(x => x.SupplierId == g.Key && x.Gid == item.Gid);
                    if (priceEntity != null && priceEntity.PricingType == 2)
                    {
                        // 按价格
                        item.Price = priceEntity.Price;
                    }
                }

                await _repository.Context.Insertable<ErpBuyorderdetailEntity>(erpBuyorderdetailEntityList).ExecuteCommandAsync();

                //判断是否整单完成入库
                if (!await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Id && ((x.BuyState ?? "0") != "2")).AnyAsync())
                {
                    entity.State = "2";
                    entity.AuditTime = DateTime.Now;
                    entity.AuditUserId = _userManager.UserId;
                    await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State, it.AuditUserId, it.AuditTime }).ExecuteCommandAsync();
                }
            }

            // 关联采购员
            if (input.taskToUserId.IsAny())
            {
                var users = input.taskToUserId.Select(it => new ErpBuyorderUserEntity
                {
                    Bid = newEntity.Id,
                    Uid = it,
                    Id = SnowflakeIdHelper.NextId()
                }).ToList();

                await _repository.Context.Insertable<ErpBuyorderUserEntity>(users).ExecuteCommandAsync();
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
    /// 更新采购任务订单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpBuyorderUpInput input)
    {
        var entity = input.Adapt<ErpBuyorderEntity>();
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        await _repository.Context.Updateable<ErpBuyorderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();


        // 清空采购订单明细原有数据
        await _repository.Context.Deleteable<ErpBuyorderdetailEntity>().Where(it => it.Fid == entity.Id).ExecuteCommandAsync();

        // 新增采购订单明细新数据
        var erpBuyorderdetailEntityList = input.erpBuyorderdetailList.Adapt<List<ErpBuyorderdetailEntity>>();
        if (erpBuyorderdetailEntityList != null)
        {
            foreach (var item in erpBuyorderdetailEntityList)
            {
                item.Id ??= SnowflakeIdHelper.NextId();
                item.Fid = entity.Id;

                if (item.WhetherProcess == 1)
                {
                    item.BuyState = "2";
                }
            }

            await _repository.Context.Insertable<ErpBuyorderdetailEntity>(erpBuyorderdetailEntityList).ExecuteCommandAsync();

            //判断是否整单完成入库
            if (!await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Id && ((x.BuyState ?? "0") != "2")).AnyAsync())
            {
                entity.State = "2";
                entity.AuditTime = DateTime.Now;
                entity.AuditUserId = _userManager.UserId;
                await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State, it.AuditUserId, it.AuditTime }).ExecuteCommandAsync();
            }
        }

        // 清空采购员明细原有数据
        await _repository.Context.Deleteable<ErpBuyorderUserEntity>().Where(it => it.Bid == entity.Id).ExecuteCommandAsync();

        // 关联采购员
        if (input.taskToUserId.IsAny())
        {
            var users = input.taskToUserId.Select(it => new ErpBuyorderUserEntity
            {
                Bid = entity.Id,
                Uid = it,
                Id = SnowflakeIdHelper.NextId()
            }).ToList();

            await _repository.Context.Insertable<ErpBuyorderUserEntity>(users).ExecuteCommandAsync();
        }

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
    /// 更新凭证.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("{id}/Proof")]
    public async Task UpdateProof(string id, [FromBody] List<FileControlsModel> files)
    {
        var entity = new ErpBuyorderEntity { Id = id };
        if (!files.IsAny())
        {
            throw Oops.Oh("缺少凭证！");
        }
        entity.Proof = files.ToJsonString();
        await _repository.Context.Updateable<ErpBuyorderEntity>(entity).UpdateColumns(x => x.Proof).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除采购任务订单.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id)) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.State == "2")
        {
            throw Oops.Oh("已入库，不允许删除！");
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        await _repository.Context.Deleteable<ErpBuyorderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

        // 清空采购订单明细表数据
        await _repository.Context.Deleteable<ErpBuyorderdetailEntity>().Where(it => it.Fid.Equals(entity.Id)).ExecuteCommandAsync();

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
    /// 批量删除采购任务订单.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("Batch")]
    [SqlSugarUnitOfWork]
    public async Task Delete([FromBody] List<string> idList)
    {
        if (idList.IsAny())
        {
            if (idList.Count > 100)
            {
                throw Oops.Oh("一次最多删除100份订单！");
            }
            var deletList = await _repository.Where(x => idList.Contains(x.Id) && x.State != "2").Select(x => x.Id).ToListAsync();

            foreach (var item in deletList)
            {
                await this.Delete(item);
            }
        }
    }


    [HttpPost("Claim/{id}")]
    public async Task Claim(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (!string.IsNullOrEmpty(entity.TaskToUserId))
        {
            throw Oops.Oh("订单已认领！");
        }

        entity.TaskToUserId = _userManager.UserId;
        await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.TaskToUserId }).ExecuteCommandAsync();
    }


    /// <summary>
    /// 获取采购任务订单.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Detail/{id}")]
    public async Task<ErpBuyorderdetailOutput> GetDetailInfo(string id)
    {
        var entity = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().FirstAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        return entity.Adapt<ErpBuyorderdetailOutput>();
    }

    /// <summary>
    /// 明细完成采购
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("Detail/{id}")]
    [SqlSugarUnitOfWork]
    public async Task CompleteDetail(string id, [FromBody] ErpBuyorderdetailDoneInput input)
    {
        var entity = input.Adapt<ErpBuyorderdetailEntity>();
        var main = await _repository.Context.Queryable<ErpBuyorderEntity>()
            .FirstAsync(x => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>()
        .Where(it => it.Id == entity.Id && it.Fid == x.Id).Any()) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (main != null && main.State != "0")
        {
            throw Oops.Oh("订单已完成！");
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();
        if (!entity.BuyTime.HasValue)
        {
            entity.BuyTime = DateTime.Now;
        }
        await _repository.Context.Updateable(entity)
            .UpdateColumns(it => new { it.Num, it.ItRemark, it.BuyTime, it.Proof, it.Price, it.Amount, it.ProductionDate, it.Retention })
            .IgnoreColumnsIF(input.proof == null, it => it.Proof)
            .ExecuteCommandAsync();

        #region 暂时不更新
        //// 找出没有采购日期的记录
        //var flag = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == main.Id && !x.BuyTime.HasValue).AnyAsync();

        //if (!flag)
        //{
        //    main.State = "1";
        //    //更新订单状态为完成
        //    await _repository.Context.Updateable(main).UpdateColumns(it => new { it.State }).ExecuteCommandAsync();
        //} 
        #endregion


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
    /// 订单完成采购
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("Complete/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Complete(string id)
    {
        var entity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.State == "1")
        {
            throw Oops.Oh("订单已完成采购！");
        }

        // 找出没有采购日期的记录
        var items = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Id).ToListAsync();

        if (items.Any(x => !x.BuyTime.HasValue && x.WhetherProcess != 1))
        {
            throw Oops.Oh("订单明细未完成采购！");
        }

        //只要未完成采购的明细
        items = items.Where(x => (x.BuyState ?? "0") == "0").ToList();

        if (string.IsNullOrEmpty(entity.RkNo))
        {
            entity.RkNo = await _billRullService.GetBillNumber("QTErpInOrder");
        }

        entity.State = "1";

        //更新订单状态为完成采购
        await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State, it.RkNo }).ExecuteCommandAsync();

        if (items.IsAny())
        {
            //更新入库数量默认为实际采购数量
            //状态改成已采购完成
            foreach (var x in items)
            {
                x.InNum = x.Num;
                x.BuyState = "1";
                x.BuyUserId = _userManager.UserId;
                await _repository.Context.Updateable<ErpBuyorderdetailEntity>(x).UpdateColumns(it => new { it.InNum, it.BuyState, it.BuyUserId }).ExecuteCommandAsync();
            }
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        //    entity.State = "1";

        //    //更新订单状态为完成采购
        //    await _repository.Context.Updateable(entity).UpdateColumns(it => new { it.State,it.RkNo }).ExecuteCommandAsync();

        //    if (items.IsAny())
        //    {
        //        //更新入库数量默认为实际采购数量
        //        //状态改成已采购完成
        //        items.ForEach(x =>
        //        {
        //            x.InNum = x.Num;
        //            x.BuyState = "1";
        //            x.BuyUserId = _userManager.UserId;
        //        });
        //        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(items).UpdateColumns(it => new { it.InNum, it.BuyState,it.BuyUserId }).ExecuteCommandAsync();
        //    }

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
    /// 订单明细完成采购
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("Detail/Complete/{id}")]
    [SqlSugarUnitOfWork]
    public async Task DetailComplete(string id)
    {
        var entity = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.BuyState == "1")
        {
            throw Oops.Oh("订单明细已完成采购！");
        }

        if (!entity.BuyTime.HasValue)
        {
            throw Oops.Oh("订单明细未完成采购！");
        }

        //更新订单状态为完成采购
        entity.BuyState = "1";
        ////更新入库数量默认为实际采购数量
        entity.InNum = entity.Num;
        // 完成人id
        entity.BuyUserId = _userManager.UserId;

        await _repository.Context.Updateable<ErpBuyorderdetailEntity>(entity).UpdateColumns(it => new { it.BuyState, it.InNum, it.BuyUserId }).ExecuteCommandAsync();

        //判断是否整单完成
        if (!await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == entity.Fid && (x.BuyState ?? "0") == "0").AnyAsync())
        {
            ErpBuyorderEntity main = new ErpBuyorderEntity
            {
                Id = entity.Fid,
                State = "1"
            };
            await _repository.Context.Updateable(main).UpdateColumns(it => it.State).ExecuteCommandAsync();
        }

        // 判断是否有入库单号 
        if (await _repository.AnyAsync(it => it.Id == entity.Fid && string.IsNullOrEmpty(it.RkNo)))
        {
            var rkNo = await _billRullService.GetBillNumber("QTErpInOrder");
            ErpBuyorderEntity main = new ErpBuyorderEntity
            {
                Id = entity.Fid,
                RkNo = rkNo,
            };
            await _repository.Context.Updateable(main).UpdateColumns(it => it.RkNo).ExecuteCommandAsync();
        }
    }

    #region 导出
    /// <summary>
    /// 导出Excel.
    /// 有固定的模板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel/{id}")]
    public async Task<dynamic> ExportExcel(string id)
    {
        var entity = await GetInfo(id) as ErpBuyorderInfoOutput ?? throw Oops.Oh(ErrorCode.COM1005);

        // 公司集合
        //var oidList = erpOrderList.Where(x => !string.IsNullOrEmpty(x.oidName)).Select(x => x.oidName).ToArray();
        //var oids = await _repository.Context.Queryable<OrganizeEntity>().Where(x => oidList.Contains(x.Id)).ToListAsync();

        //var unitOptions = await _dictionaryDataService.GetList("JLDW");

        //List<Task> exportTasks = new List<Task>();
        //var tempDir = Guid.NewGuid().ToString();
        //var rootPath = Path.Combine(Path.GetTempPath(), tempDir);
        //if (!Directory.Exists(rootPath))
        //{
        //    Directory.CreateDirectory(rootPath);
        //}
        var templatePath = Path.Combine(App.WebHostEnvironment.WebRootPath, "Template", "Erp", "采购订单导出模板.xlsx");
        //List<string> excelFilePaths = new List<string>();
        //int no = 1;
        //foreach (var item in erpOrderList)
        //{
        //    #region 设置公司名称
        //    var org = oids.Find(x => x.Id == item.oidName);
        //    if (org != null)
        //    {
        //        item.oidName = org.FullName;
        //        if (!string.IsNullOrEmpty(org.PropertyJson))
        //        {
        //            var extend = org.PropertyJson.ToObject<OrganizePropertyModel>();
        //            if (extend != null && !string.IsNullOrEmpty(extend.shortName))
        //            {
        //                item.oidName = extend.shortName;
        //            }
        //        }
        //    }
        //    #endregion

        //    var details = erpOrderdetailList.Where(x => x.fid == item.id).ToList();

        //    if (details.Any())
        //    {
        //        item.totalCount = details.Count.ToString();
        //        item.totalAmount = details.Sum(x => decimal.Parse(x.amount)).ToString();

        //        details.ForEach(dx =>
        //        {
        //            dx.productUnit = unitOptions.Find(x => x.EnCode == dx.productUnit)?.FullName ?? "";
        //        });
        //    }

        //    #region 设置导出数据源
        //    var path = Path.Combine(rootPath, $"{no++}-{DateTime.Parse(item.orderDate).ToString("MMdd")}-{item.cidName}.xlsx");
        //    excelFilePaths.Add(path);

        //    var param = item.ToDictionary();

        //    param.Add("detail", details);
        //    #endregion

        //    //exportTasks.Add(MiniExcel.SaveAsByTemplateAsync(path, templatePath, param));
        //    await MiniExcel.SaveAsByTemplateAsync(path, templatePath, param);
        //}

        //var path = Path.Combine(rootPath, $"{data.no}-{DateTime.Parse(item.orderDate).ToString("MMdd")}-{item.cidName}.xlsx");

        var fileName = $"采购订单_{entity.no}.xlsx";

        var type = string.Empty;
        var path = Path.Combine(_fileManager.GetPathByType(type), fileName);

        var param = entity.ToDictionary();
        param["totalCount"] = entity.erpBuyorderdetailList.GroupBy(x => x.gid).Count();
        param[nameof(ErpBuyorderInfoOutput.taskBuyTime)] = entity.taskBuyTime?.ToString("yyyy-MM-dd");

        await MiniExcel.SaveAsByTemplateAsync(path, templatePath, param);

        using FileStream stream = File.OpenRead(path);
        var flag = await _fileManager.UploadFileByType(stream, _fileManager.GetPathByType(type), fileName);

        return new { name = fileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + fileName + "|" + type, "QT" + "|" + _userManager.TenantId) };
    }
    #endregion



    #region 批量上传采购订单

    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public async Task<dynamic> TemplateDownload()
    {
        // 初始化 一条空数据 
        List<ErpBuyorderCgImportData>? dataList = new List<ErpBuyorderCgImportData>() { new ErpBuyorderCgImportData() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("采购员采购订单导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpBuyorderCgImportData>())
        {
            if (item.Key == nameof(BaseImportDataInput.ErrorMessage))
            {
                continue;
            }
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        //ExcelExportHelper<ErpOrderListImportDataInput>.Export(dataList, excelconfig, addPath);

        var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpBuyorderCgImportData>.ExportMemoryStream(dataList, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);


        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT") };
    }

    /// <summary>
    /// 01.上传文件.（api/File/Uploader/template）
    /// 02.导入预览.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ImportPreview")]
    public async Task<dynamic> ImportPreview(string fileName)
    {
        try
        {
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpBuyorderCgImportData>();

            string? filePath = Path.Combine(FileVariable.TemporaryFilePath, fileName.Replace("@", "."));
            using (var stream = (await _fileManager.DownloadFileByType(filePath, fileName))?.FileStream)
            {
                // 得到数据
                var excelData = ExcelImportHelper.ToDataTable(stream, ExcelImportHelper.IsXls(fileName));
                foreach (DataColumn item in excelData.Columns)
                {
                    //
                    if (FileEncode.ContainsValue(item.ToString()))
                    {
                        excelData.Columns[item.ToString()].ColumnName = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                    }

                }

                // 返回结果
                return new { dataRow = excelData };
            }


        }
        catch (Exception e)
        {
            //UnifyContext.Fill(e.Message);
            throw Oops.Oh(ErrorCode.D1801);
        }
    }

    /// <summary>
    /// 03.导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    [SqlSugarUnitOfWork]
    public async Task<dynamic> ImportData([FromBody] ImportDataInput<ErpBuyorderCgImportData> list)
    {
        object[]? res = await ImportOrderData(list.list);
        List<ErpBuyorderCrInput>? addlist = res.First() as List<ErpBuyorderCrInput>;
        List<ErpBuyorderCgImportData>? errorlist = res.Last() as List<ErpBuyorderCgImportData>;
        var output = new ErpBuyorderCgImportResultOutput()
        {
            snum = addlist.Count,
            fnum = errorlist.Count,
            //failResult = errorlist,
            resultType = errorlist.Count < 1 ? 0 : 1
        };
        output.failResult = errorlist;

        if (errorlist.IsAny())
        {
            ExcelConfig excelconfig = ExcelConfig.Default($"{DateTime.Now.ToString("yyyyMMddHHmmss")}_采购导入错误报告.xls");
            foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpBuyorderCgImportData>())
            {
                string? column = item.Key;
                string? excelColumn = item.Value;
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
            }

            string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);

            var flag = await _fileManager.UploadFileByType(ExcelExportHelper<ErpBuyorderCgImportData>.ExportMemoryStream(errorlist, excelconfig), FileVariable.TemporaryFilePath, excelconfig.FileName);


            output.failFileUrl = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath + "|" + _userManager.TenantId, "QT");
        }
        return output;
    }

    /// <summary>
    /// 导入用户数据函数.
    /// </summary>
    /// <param name="list">list.</param>
    /// <returns>[成功列表,失败列表].</returns>
    private async Task<object[]> ImportOrderData(List<ErpBuyorderCgImportData> list)
    {
        List<ErpBuyorderCgImportData> userInputList = list;
        //var oid = _userManager.CompanyId;
        //var oidName = "当前公司";
        //if (oid.IsNotEmptyOrNull())
        //{
        //    oidName = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.Id == oid).Select(it => it.FullName).FirstAsync();
        //}
        #region 初步排除错误数据

        if (userInputList == null || userInputList.Count() < 1)
            throw Oops.Oh(ErrorCode.D5019);

        List<ErpBuyorderCgImportData>? errorList = new List<ErpBuyorderCgImportData>();
        // 必填字段验证 (计划采购时间、供应商名称、商品名称、规格、数量单价、金额、采购方式，付款渠道)
        var requiredList = EntityHelper<ErpBuyorderCgImportData>.InstanceProperties.Where(p => p.HasAttribute<RequiredAttribute>());
        if (requiredList.Any())
        {
            errorList = userInputList.Where(x =>
            {
                var error = requiredList.Any(p => !Common.Extension.Extensions.IsNotEmptyOrNull(p.GetValue(x, null)));

                if (!error)
                {
                    // 判断类型是否转换成功
                    if (
                       /*!DateTime.TryParse(x.CreateTime, out var t1)  ||*/
                       !decimal.TryParse(x.planNum, out var n1)
                    || (!string.IsNullOrEmpty(x.amount) && !decimal.TryParse(x.amount, out var n2))
                    || (!string.IsNullOrEmpty(x.price) && !decimal.TryParse(x.price, out var n3)) // 价格填了值才去判断
                    )
                    {
                        error = true;
                        x.ErrorMessage = $"日期或者金额字段类型转换失败！";
                    }
                }
                else
                {
                    var str = string.Join(",", requiredList.Select(x => x.GetDescription()));
                    if (!string.IsNullOrEmpty(str))
                    {
                        x.ErrorMessage = $"{str}，字段不能为空！";
                    }
                }

                return error;
            }).ToList();
        }

        #endregion

        var oid = _userManager.CompanyId;
        //单位字典
        //var unitOptions = await _dictionaryDataService.GetList("JLDW");

        ////餐别字典
        //var diningTypeOptions = await _dictionaryDataService.GetList("ErpOrderDiningType");

        // 带出关联表的数据
        //客户集合
        var supplierList = await _repository.Context.Queryable<ErpSupplierEntity>().Where(it => it.Oid == oid).Select(it => new ErpSupplierEntity { Id = it.Id, Name = it.Name }).ToListAsync();
        //// 用户集合
        //var userList = await _repository.Context.Queryable<UserEntity>().Select(it => new UserEntity { Id = it.Id, RealName = it.RealName }).ToListAsync();

        // 公司集合
        var organizeList = await _repository.Context.Queryable<OrganizeEntity>().Select(it => new OrganizeEntity { Id = it.Id, FullName = it.FullName }).ToListAsync();

        // 商品集合
        var productList = await _repository.Context.Queryable<ErpProductmodelEntity>()
            .InnerJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
            .LeftJoin<ErpProducttypeEntity>((a, b, c) => b.Tid == c.Id)
            .Select((a, b, c) => new
            {
                productName = b.Name,
                productType = c.Name,
                midName = a.Name,
                id = a.Id,
                unit = a.Unit,
                pid = b.Id
            })
            .ToListAsync();

        // 当前公司关联的商品
        var relationCompanys = await _repository.Context.Queryable<ErpProductcompanyEntity>().Select(it => new ErpProductcompanyEntity
        {
            Pid = it.Pid,
            Oid = it.Oid
        }).ToListAsync();


        userInputList = userInputList.Except(errorList).ToList();
        List<ErpBuyorderCgImportData> newList = new List<ErpBuyorderCgImportData>();
        foreach (var item in userInputList)
        {
            //// 调出公司id
            //item.oid1 = organizeList.Find(x => x.FullName == item.Oid1Name)?.Id ?? "";
            //// 调入公司
            //item.oid2 = organizeList.Find(x => x.FullName == item.Oid2Name)?.Id ?? "";

            // 转规格
            var qur = productList.Where(x => x.productName == item.productName && x.midName == item.gidName);
            //if (!string.IsNullOrEmpty(item.ProductType))
            //{
            //    qur = qur.Where(x => x.productType == item.ProductType);
            //}
            //if (!string.IsNullOrEmpty(unit))
            //{
            //    qur = qur.Where(x => x.unit == unit);
            //}
            var midEntity = qur.FirstOrDefault();
            item.gid = midEntity?.id ?? "";

            item.supplier = supplierList.Find(x => x.Name == item.supplierName)?.Id ?? "";

            //var mid = productList.Find(x => x.productName == item.ProductName && x.productType == item.ProductType && x.midName == item.MidName && x.unit == unit)?.id ?? "";

            // 转换后判断客户id，下单员、规格是否有空，如果是则加入错误列表
            if (!item.gid.IsNotEmptyOrNull() || !item.supplier.IsNotEmptyOrNull() /*|| (item.DiningType.IsNotEmptyOrNull() && !diningType.IsNotEmptyOrNull())*/)
            {
                var errors = new List<string>();

                if (!item.gid.IsNotEmptyOrNull())
                {
                    errors.Add("商品规格不存在！");
                }
                if (!item.supplier.IsNotEmptyOrNull())
                {
                    errors.Add("供应商不存在！");
                }
                //if (!diningType.IsNotEmptyOrNull())
                //{
                //    errors.Add("餐别不存在！");
                //}
                item.ErrorMessage = string.Join(", ", errors);
                errorList.Add(item);
            }
            else
            {
                // 判断规格是否绑定了当前公司
                if (relationCompanys.Any(x => x.Oid == oid && (x.Pid == midEntity!.id || x.Pid == midEntity.pid))
                    || !(relationCompanys.Any(x => x.Pid == midEntity!.id || x.Pid == midEntity.pid)))
                {
                    // 给记录重新赋值
                    //item.Oid1Name = oid1;
                    //item.Oid2Name = oid2;
                    //item.CreateUidName = createUid;
                    //item.MidName = mid;
                    //item.DiningType = diningType;
                    newList.Add(item);
                }
                else
                {
                    item.ErrorMessage = $"商品没有关联当前公司";
                    errorList.Add(item);
                }
            }
        }

        var dump = newList.GroupBy(x => new { x.taskBuyTime, x.supplier, x.gid }).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
        if (dump.IsAny())
        {
            // 排除重复
            dump.ForEach(x =>
            {
                x.ErrorMessage = $"商品重复！";
                errorList.Add(x);
                newList.Remove(x);
            });
        }

        // 再重新排除一次异常记录
        userInputList = newList;

        List<ErpOutrecordEntity> addDetailList = new List<ErpOutrecordEntity>();


        List<ErpBuyorderCrInput> addList = new List<ErpBuyorderCrInput>();
        foreach (var g in userInputList.GroupBy(it => new { it.taskBuyTime }))
        {
            ErpBuyorderCrInput erpBuyorderCrInput = new ErpBuyorderCrInput()
            {
                taskBuyTime = g.Key.taskBuyTime,
                taskToUserId = new string[] { _userManager.UserId },
                erpBuyorderdetailList = g.Select(it => new ErpBuyorderdetailCrInput
                {
                    gid = it.gid,
                    supplier = it.supplier,
                    planNum = decimal.Parse(it.planNum),
                    num = decimal.Parse(it.planNum),
                    payment = it.payment,
                    channel = it.channel,
                    price = decimal.Parse(it.price),
                    amount = decimal.Parse(it.amount),
                    productionDate = it.productionDate,
                    retention = it.retention
                }).ToList()
            };

            if (erpBuyorderCrInput.erpBuyorderdetailList.IsAny())
            {
                addList.Add(erpBuyorderCrInput);
            }

        }

        if (addList.IsAny())
        {
            foreach (var item in addList)
            {
                // 导入调出单
                await this.Create(item);

            }
        }
        return new object[] { addList, errorList };
    }

    #endregion

}
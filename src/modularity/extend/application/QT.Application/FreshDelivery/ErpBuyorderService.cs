using Microsoft.AspNetCore.Http;
using QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpInorder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductmodel;
using QT.Application.Entitys.Enum.FreshDelivery;
using QT.Application.Entitys.FreshDelivery;
using QT.Application.Interfaces.FreshDelivery;
using QT.Common.Configuration;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Helper;
using QT.Common.Models.NPOI;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.DatabaseAccessor.SqlSugar;
using QT.LinqBuilder;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.System;
using QT.UnifyResult;
using System.Data;
using System.Linq.Expressions;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：采购任务订单.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "采购订单", Name = "ErpBuyorder", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpBuyorderService : IErpBuyorderService, IDynamicApiController, ITransient
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
    public ErpBuyorderService(
        ISqlSugarRepository<ErpBuyorderEntity> erpBuyorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IFileManager fileManager)
    {
        _repository = erpBuyorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _fileManager = fileManager;

        // 清楚全局过滤条件
        if (_repository.Context.QueryFilter.GeFilterList.Any())
        {
            _repository.Context.QueryFilter.Clear<ICompanyEntity>();
        }
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

        var erpBuyorderdetailList = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
            .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
            .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
            .Where(w => w.Fid == output.id)
            .Select((w, a, b) => new ErpBuyorderdetailInfoOutput
            {
                gidName = a.Name,
                productName = b.Name,
                supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == b.Supplier).Select(x => x.Name),
                buyState = w.BuyState ?? "0",
                firstChar = b.FirstChar,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
            }, true)
            .ToListAsync();
        output.erpBuyorderdetailList = erpBuyorderdetailList; //.Adapt<List<ErpBuyorderdetailInfoOutput>>();
        output.erpBuyorderdetailList.ForEach(x =>
        {
            x.num = x.num ?? 0;
            x.supplierName = x.supplierName ?? "";
            x.price = x.price ?? 0;
            x.amount = x.amount ?? 0;
            x.remark = x.remark ?? "";
            x.itRemark = x.itRemark ?? "";
        });

        var users = await _repository.Context.Queryable<ErpBuyorderUserEntity>().Where(it => it.Bid == id).Select(it => it.Uid).ToListAsync();

        if (output.taskToUserId.IsAny())
        {
            users.AddRange(output.taskToUserId);
        }

        output.taskToUserId = users.Distinct().ToArray();

        if (output.taskToUserId.IsAny())
        {
            var curTaskUserList = await _repository.Context.Queryable<UserEntity>().Where(x => output.taskToUserId.Contains(x.Id)).Select(x => new
            {
                id = x.Id,
                name = x.RealName
            }).ToListAsync();

            UnifyContext.Fill(new { curTaskUserList });
        }

        return output;
    }

    /// <summary>
    /// 获取采购任务订单列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    [Obsolete]
    public async Task<dynamic> GetList([FromQuery] ErpBuyorderListQueryInput input)
    {
        List<DateTime> queryTaskBuyTime = input.taskBuyTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startTaskBuyTime = queryTaskBuyTime?.First();
        DateTime? endTaskBuyTime = queryTaskBuyTime?.Last();
        var data = await _repository.Context.Queryable<ErpBuyorderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(queryTaskBuyTime != null, it => SqlFunc.Between(it.TaskBuyTime, startTaskBuyTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endTaskBuyTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                )
            .WhereIF(!string.IsNullOrEmpty(input.state), it => it.State == input.state)
            .WhereIF(!string.IsNullOrEmpty(input.taskToUserId), it => it.TaskToUserId == input.taskToUserId || SqlFunc.Subqueryable<ErpBuyorderUserEntity>().Where(xxx => xxx.Bid == it.Id && xxx.Uid == input.taskToUserId).Any())
            .WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
            .WhereIF(!string.IsNullOrEmpty(input.channel), it => SqlFunc.Subqueryable<ErpBuyorderdetailEntity>().Where(xxx => xxx.Fid == it.Id && xxx.Channel == input.channel).Any())
            .OrderBy(it => it.CreatorTime, OrderByType.Desc)
            .Select(it => new ErpBuyorderListOutput
            {
                id = it.Id,
                //oid = it.Oid,
                no = it.No,
                taskToUserId = it.TaskToUserId,
                taskBuyTime = it.TaskBuyTime,
                taskRemark = it.TaskRemark,
                taskToUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.TaskToUserId).Select(d => d.RealName),
                state = it.State ?? "0",
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
                creatorTime = it.CreatorTime
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
                Name = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.Uid).Select(d => d.RealName)
            }).ToListAsync();

            foreach (var item in data.list)
            {
                var list = users.Where(it => it.Bid == item.id).Select(it => it.Name).ToList();
                if (!string.IsNullOrEmpty(item.taskToUserName))
                {
                    list.Add(item.taskToUserName);
                }
                item.taskToUserName = string.Join(",", list.Distinct());
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



        foreach (var g in input.erpBuyorderdetailList.GroupBy(it => it.supplier))
        {
            var entity = input.Adapt<ErpBuyorderEntity>();
            entity.Id = SnowflakeIdHelper.NextId();
            //entity.No = await _billRullService.GetBillNumber("QTErpBuyOrder");
            entity.State = "0"; //状态为0

            var erpBuyorderdetailEntityList = g.Adapt<List<ErpBuyorderdetailEntity>>();
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
        var oldEntity = await _repository.SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);
        var entity = input.Adapt<ErpBuyorderEntity>();
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        await _repository.Context.Updateable<ErpBuyorderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

        /*
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
            }

            await _repository.Context.Insertable<ErpBuyorderdetailEntity>(erpBuyorderdetailEntityList).ExecuteCommandAsync();
        }*/

        var dbList = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(it => it.Fid == entity.Id).ToListAsync();
        _repository.Context.Tracking(dbList);
        var result = CompareList(input.erpBuyorderdetailList, dbList, x => x.Id);
        if (result.add.Any())
        {
            // 如果订单已入库，不允许加单，需要先反审
            if (oldEntity.State == "2")
            {
                throw Oops.Oh("订单已入库，不允许加单，请先反审！");
            }

            result.add.ForEach(x =>
            {
                x.Id = SnowflakeIdHelper.NextId();
                x.Fid = entity.Id;
            });

            await _repository.Context.Insertable<ErpBuyorderdetailEntity>(result.add).ExecuteCommandAsync();
        }
        if (result.update.Any())
        {
            List<ErpBuyorderdetailEntity> updateErpBuyorderdetailEntitys = new List<ErpBuyorderdetailEntity>();
            // 判断是否已经入库，入库不允许修改单价
            foreach (var item in result.update)
            {
                var changes = _repository.Context.GetChanges(item);
                if (changes.Contains(nameof(ErpBuyorderdetailEntity.BuyState)))
                {
                    throw Oops.Oh("单据状态发生变动，请刷新后再修改！");
                }

                if (item.BuyState == "2")
                {
                    if (changes.Contains(nameof(ErpBuyorderdetailEntity.Num)) || changes.Contains(nameof(ErpBuyorderdetailEntity.Price)) || changes.Contains(nameof(ErpBuyorderdetailEntity.Amount)))
                    {
                        throw Oops.Oh("单据状态发生变动，请刷新后再修改！");
                    }

                }
                if (changes.IsAny())
                {
                    updateErpBuyorderdetailEntitys.Add(item);
                }
            }
            if (updateErpBuyorderdetailEntitys.IsAny())
            {
                await _repository.Context.Updateable<ErpBuyorderdetailEntity>(updateErpBuyorderdetailEntitys).ExecuteCommandAsync();
            }
        }
        if (result.delete.Any())
        {
            var delIds = result.delete.Select(x => x.Id).ToList();
            if (await _repository.Context.Queryable<ErpInrecordEntity>().Where(it => delIds.Contains(it.Id)).AnyAsync())
            {
                throw Oops.Oh("采购明细已入库，不允许删除，请检查后再进行操作！");
            }
            foreach (var item in result.delete)
            {
                if (item.BuyState == "2")
                {
                    throw Oops.Oh("采购明细已入库，不允许删除！！");
                }
            }
            
            await _repository.Context.Deleteable<ErpBuyorderdetailEntity>(result.delete).ExecuteCommandAsync();
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


        // 更新订单关联采购id
        var buyorders = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == id)
            .Select(x => new ErpBuyorderdetailEntity
            {
                Id = x.Id,
                ProductionDate = x.ProductionDate,
                Retention = x.Retention
            })
            .ToListAsync();

        if (buyorders.IsAny())
        {
            var inrecordList = await _repository.Context.Queryable<ErpInrecordEntity>()
            .Where(x => buyorders.Any(it => it.Id == x.Bid))
            .Select(x => new ErpInrecordEntity
            {
                Id = x.Id,
                ProductionDate = x.ProductionDate,
                Retention = x.Retention,
                Bid = x.Bid
            })
            .ToListAsync();

            if (inrecordList.IsAny())
            {
                var relations = await _repository.Context.Queryable<ErpOutdetailRecordEntity>()
                     .Where(it => inrecordList.Any(x => x.Id == it.InId))
                     .ToListAsync();

                // 关联订单
                var orderdetails = await _repository.Context.Queryable<ErpOrderdetailEntity>()
                   .Where(it => relations.Any(x => x.OutId == it.Id))
                   .Select(it => new ErpOrderdetailEntity
                   {
                       Id = it.Id,
                       Bid = it.Bid
                   })
                   .ToListAsync();

                List<ErpInrecordEntity> updateErpInrecordEntitys = new List<ErpInrecordEntity>();
                List<ErpOrderdetailEntity> updateErpOrderdetailEntitys = new List<ErpOrderdetailEntity>();
                foreach (var item in buyorders)
                {

                    foreach (var inrecord in inrecordList.Where(x => x.Bid == item.Id).ToList())
                    {
                        if (inrecord.ProductionDate != item.ProductionDate || inrecord.Retention != item.Retention)
                        {
                            _repository.Context.Tracking(inrecord);
                            // 入库信息
                            inrecord.ProductionDate = item.ProductionDate;
                            inrecord.Retention = item.Retention;
                            //await _repository.Context.Updateable<ErpInrecordEntity>(inrecord).UpdateColumns(new string[] { nameof(ErpInrecordEntity.ProductionDate), nameof(ErpInrecordEntity.Retention) }).ExecuteCommandAsync();

                            if (!updateErpInrecordEntitys.Contains(inrecord))
                            {
                                updateErpInrecordEntitys.Add(inrecord);
                            }
                        }

                        // 订单信息
                        foreach (var order in orderdetails.Where(x => relations.Any(it => it.OutId == x.Id && it.InId == inrecord.Id)).ToList())
                        {
                            if (order.Bid != item.Id)
                            {
                                _repository.Context.Tracking(order);
                                order.Bid = item.Id;
                                //await _repository.Context.Updateable<ErpOrderdetailEntity>(order).UpdateColumns(new string[] { nameof(ErpOrderdetailEntity.Bid) }).ExecuteCommandAsync();

                                if (!updateErpOrderdetailEntitys.Contains(order))
                                {
                                    updateErpOrderdetailEntitys.Add(order);
                                }
                            }

                        }
                    }
                }

                if (updateErpInrecordEntitys.IsAny())
                {
                    await _repository.Context.Updateable<ErpInrecordEntity>(updateErpInrecordEntitys).ExecuteCommandAsync();
                }
                if (updateErpOrderdetailEntitys.IsAny())
                {
                    await _repository.Context.Updateable<ErpOrderdetailEntity>(updateErpOrderdetailEntitys).ExecuteCommandAsync();
                }
            }
        }


        // 如果改了供应商，那么拆分订单
        if (input.erpBuyorderdetailList.GroupBy(it => it.supplier).Count() > 1)
        {
            var main = await _repository.Context.Queryable<ErpBuyorderEntity>().SingleAsync(x => x.Id == id);
            var details = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Fid == id).ToListAsync();

            // 最多记录的供应商，保留
            var sid = details.GroupBy(it => it.Supplier).OrderByDescending(x => x.Count()).Select(x => x.Key).FirstOrDefault();

            foreach (var g in details.GroupBy(it => it.Supplier))
            {
                if (g.Key == sid)
                {
                    continue;
                }
                ErpBuyorderEntity newEntity = new ErpBuyorderEntity();
                main.Adapt(newEntity);
                newEntity.Id = SnowflakeIdHelper.NextId();
                newEntity.No = $"{main.No}-{DateTime.Now.ToString("HHmmss")}"; // await _billRullService.GetBillNumber("QTErpBuyOrder");


                await _repository.Context.Insertable<ErpBuyorderEntity>(newEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();

                foreach (var item in g)
                {
                    item.Fid = newEntity.Id;
                    await _repository.Context.Updateable<ErpBuyorderdetailEntity>(item).UpdateColumns(x => x.Fid).ExecuteCommandAsync();
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
    /// 更新订单关联的采购信息
    /// </summary>
    /// <param name="id">采购明细id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task UpdateOrderDetailByBuyorderdetailId(string id, UpdateStoreInfoInput input)
    {
        // 更新订单关联采购id
        var buyorder = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(x => x.Id == id)
            .Select(x => new ErpBuyorderdetailEntity
            {
                Id = x.Id,
                ProductionDate = x.ProductionDate,
                Retention = x.Retention
            })
            .FirstAsync();

        if (buyorder != null)
        {
            buyorder.ProductionDate = input.productionDate;
            buyorder.Retention = input.retention;
            await _repository.Context.Updateable<ErpBuyorderdetailEntity>(buyorder).UpdateColumns(new string[] { nameof(ErpBuyorderdetailEntity.ProductionDate), nameof(ErpBuyorderdetailEntity.Retention) }).ExecuteCommandAsync();

            var inrecordList = await _repository.Context.Queryable<ErpInrecordEntity>()
            .Where(x => buyorder.Id == x.Bid)
            .Select(x => new ErpInrecordEntity
            {
                Id = x.Id,
                ProductionDate = x.ProductionDate,
                Retention = x.Retention,
                Bid = x.Bid
            })
            .ToListAsync();

            if (inrecordList.IsAny())
            {
                var relations = await _repository.Context.Queryable<ErpOutdetailRecordEntity>()
                     .Where(it => inrecordList.Any(x => x.Id == it.InId))
                     .ToListAsync();

                // 关联订单
                var orderdetails = await _repository.Context.Queryable<ErpOrderdetailEntity>()
                   .Where(it => relations.Any(x => x.OutId == it.Id))
                   .Select(it => new ErpOrderdetailEntity
                   {
                       Id = it.Id,
                       Bid = it.Bid
                   })
                   .ToListAsync();


                List<ErpInrecordEntity> updateErpInrecordEntitys = new List<ErpInrecordEntity>();
                List<ErpOrderdetailEntity> updateErpOrderdetailEntitys = new List<ErpOrderdetailEntity>();

                foreach (var inrecord in inrecordList.Where(x => x.Bid == buyorder.Id).ToList())
                {
                    if (inrecord.ProductionDate != buyorder.ProductionDate || inrecord.Retention != buyorder.Retention)
                    {
                        _repository.Context.Tracking(inrecord);
                        // 入库信息
                        inrecord.ProductionDate = buyorder.ProductionDate;
                        inrecord.Retention = buyorder.Retention;

                        if (!updateErpInrecordEntitys.Contains(inrecord))
                        {
                            updateErpInrecordEntitys.Add(inrecord);
                        }
                    }

                    List<string> strs = new List<string>();
                    if (buyorder.ProductionDate.HasValue)
                    {
                        strs.Add($"生产日期{buyorder.ProductionDate.Value.ToString("yyyy.MM.dd")}");
                    }
                    if (buyorder.Retention.IsNotEmptyOrNull())
                    {
                        strs.Add($"保质期{buyorder.Retention}");
                    }
                    var remark = string.Join(",", strs);
                    //await _repository.Context.Updateable<ErpInrecordEntity>(inrecord).UpdateColumns(new string[] { nameof(ErpInrecordEntity.ProductionDate), nameof(ErpInrecordEntity.Retention) }).ExecuteCommandAsync();

                    // 订单信息
                    foreach (var order in orderdetails.Where(x => relations.Any(it => it.OutId == x.Id && it.InId == inrecord.Id)).ToList())
                    {
                        if (order.Bid != buyorder.Id || order.Remark!= remark)
                        {
                            _repository.Context.Tracking(order);
                            order.Bid = buyorder.Id;
                            order.Remark = remark;
                            //await _repository.Context.Updateable<ErpOrderdetailEntity>(order).UpdateColumns(new string[] { nameof(ErpOrderdetailEntity.Bid) }).ExecuteCommandAsync();
                            if (!updateErpOrderdetailEntitys.Contains(order))
                            {
                                updateErpOrderdetailEntitys.Add(order);
                            }
                        }

                    }
                }
                if (updateErpInrecordEntitys.IsAny())
                {
                    await _repository.Context.Updateable<ErpInrecordEntity>(updateErpInrecordEntitys).ExecuteCommandAsync();
                }
                if (updateErpOrderdetailEntitys.IsAny())
                {
                    await _repository.Context.Updateable<ErpOrderdetailEntity>(updateErpOrderdetailEntitys).ExecuteCommandAsync();
                }
            }
        }

    }

    private (List<TTarget> add, List<TTarget> update, List<TTarget> delete) CompareList<TSource, TTarget>(List<TSource> source, List<TTarget> target, Func<TTarget, object> getKey) where TTarget : class, new()
    {
        var add = new List<TTarget>();
        var update = new List<TTarget>();
        var delete = new List<TTarget>();

        var exists = new List<TTarget>();
        foreach (var item in source)
        {
            var newTarget = item.Adapt<TTarget>();

            var key = getKey(newTarget);

            // 新增
            if (key == null)
            {
                add.Add(newTarget);
            }
            else
            {
                var entity = target.Find(x => getKey(x).Equals(key));

                if (entity == null)
                {
                    //新增
                    add.Add(newTarget);
                }
                else
                {
                    _repository.Context.Tracking(entity);
                    entity = item.Adapt(entity);
                    update.Add(entity);
                }
            }
        }

        delete = target.Except(update).ToList();

        return (add, update, delete);
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
    /// 获取采购员列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("BuyerList")]
    public async Task<List<ErpBuyerInfoOutput>> GetBuyerList()
    {
        var roleList = await _repository.Context.Queryable<RoleEntity>().Where(x => x.FullName.Contains("采购员")).Select(x => x.Id).ToListAsync();

        List<ErpBuyerInfoOutput> result = new List<ErpBuyerInfoOutput>();
        if (roleList.Any())
        {
            var where = string.Join(" OR ", roleList.Select(x => $"FIND_IN_SET('{x}',F_RoleId)").ToArray());

            result = await _repository.Context.Queryable<UserEntity>().Where(where).Where(x => x.DeleteMark == null).Select(x => new ErpBuyerInfoOutput
            {
                id = x.Id,
                name = x.RealName,
                //companyId = x.OrganizeId
            }).ToListAsync();

            var userIdList = result.Select(x => x.id).ToArray();

            var organizeList = await _repository.Context.Queryable<UserRelationEntity>().Where(it => userIdList.Contains(it.UserId) && it.ObjectType == "Organize").ToListAsync();

            result.ForEach(x =>
            {
                x.companyId = organizeList.Where(it => it.UserId == x.id).Select(it => it.ObjectId).ToArray();
            });
        }
        return result;
    }


    ///// <summary>
    ///// 采购建议
    ///// </summary>
    ///// <param name="oid">公司id，没有传进来的话，取当前登录的id</param>
    ///// <returns></returns>
    //[HttpGet("recommend/v1")]
    //public async Task<List<ErpProductRecommendOutput>> GetRecommendList([FromQuery] string oid)
    //{
    //    string customerType = string.Empty;
    //    if (!string.IsNullOrEmpty(oid))
    //    {
    //        customerType = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == oid).Select(x => x.Type).FirstAsync();
    //    }

    //    if (string.IsNullOrEmpty(oid))
    //    {
    //        oid = _userManager.CompanyId;
    //    }

    //    // 获取采购建议
    //    var recommendList = await _repository.Context.Queryable<ViewErpProductRecommend>().Where(it => it.Oid == oid).ToListAsync();

    //    if (recommendList == null || !recommendList.Any())
    //    {
    //        return new List<ErpProductRecommendOutput>();
    //    }

    //    var arr = recommendList.Select(x => x.Mid).ToArray();

    //    var data = await _repository.Context.Queryable<ErpProductmodelEntity>()
    //       .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
    //       //.LeftJoin<ErpProductpriceEntity>((a, b, c) => a.Id == c.Gid && c.Cid == cid)
    //       //.LeftJoin<ErpProductcustomertypepriceEntity>((a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
    //       .Where((a, b) => b.State == "1")
    //       .Where((a, b) => arr.Contains(a.Id))
    //       .Select((a, b) => new ErpProductRecommendOutput
    //       {
    //           id = a.Id,
    //           name = a.Name,
    //           costPrice = a.CostPrice,
    //           grossMargin = a.GrossMargin,
    //           minNum = a.MinNum,
    //           num = 0,// a.Num,
    //           package = a.Package,
    //           productCode = b.No,
    //           productName = b.Name,
    //           //salePrice = string.IsNullOrEmpty(c.Id) ? a.SalePrice : c.Price,
    //           //salePrice = c.Price > 0 ? c.Price : (d.Discount > 0 ? (a.SalePrice * d.Discount * 0.01m) : (d.Price > 0 ? d.Price : a.SalePrice)), //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
    //           //salePrice = c.Price > 0 ? c.Price : (d.PricingType == 1 ? (a.SalePrice * d.Discount * 0.01m) : (d.PricingType == 2 ? d.Price : a.SalePrice)), //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
    //           productUnit = b.Unit,
    //           maxNum = a.MaxNum > 0 ? a.MaxNum : 9999999,
    //           supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == b.Supplier).Select(x => x.Name)
    //       }).ToListAsync();

    //    foreach (var item in data)
    //    {
    //        var entity = recommendList.Find(x => x.Mid == item.id);
    //        if (entity != null)
    //        {
    //            item.num = entity.Num;
    //            item.storeNum = entity.StoreNum;
    //            if (entity.StoreNum > 0)
    //            {
    //                //item.remark = $"库存剩余{float.Parse(entity.StoreNum.ToString())}";
    //            }
    //            //把关联的订单数量带到备注
    //            if (!string.IsNullOrEmpty(entity.OrderNum))
    //            {
    //                //var str = entity.OrderNum.Split(',', StringSplitOptions.RemoveEmptyEntries)
    //                //    .Where(x =>
    //                //    {
    //                //        var arr = x.Split(":",true);
    //                //        var numStr = arr[0];
    //                //        var remark = arr.Length > 1 ? arr[1] : "";
    //                //        return decimal.TryParse(numStr, out decimal num) && num > 0;
    //                //}).ToArray();

    //                List<string> str = new List<string>();

    //                foreach (var x in entity.OrderNum.Split(',', StringSplitOptions.RemoveEmptyEntries))
    //                {
    //                    var array = x.Split(":", true);
    //                    var numStr = array[0];
    //                    var remark = array.Length > 1 ? array[1] : "";
    //                    if (decimal.TryParse(numStr, out decimal num) && num > 0)
    //                    {
    //                        str.Add(string.IsNullOrEmpty(remark) ? numStr : $"{numStr}({remark})");
    //                    }
    //                }

    //                if (str != null && str.Any())
    //                {
    //                    item.remark = string.IsNullOrEmpty(item.remark) ? string.Join("+", str) : $"{item.remark}，{string.Join("+", str)}";
    //                }
    //            }
    //        }
    //    }
    //    return data;
    //}

    /// <summary>
    /// 采购建议 2024.3.26
    /// </summary>
    /// <param name="oid">公司id，没有传进来的话，取当前登录的id</param>
    /// <returns></returns>
    [HttpGet("recommend")]
    public async Task<List<ErpProductRecommendOutput>> GetRecommendListV2([FromQuery] ErpProductRecommendInput input)
    {
        var oid = input.oid;
        List<DateTime> queryPosttime = input.posttime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startPosttime = queryPosttime?.First();
        DateTime? endPosttime = queryPosttime?.Last();

        string customerType = string.Empty;
        if (!string.IsNullOrEmpty(oid))
        {
            customerType = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == oid).Select(x => x.Type).FirstAsync();
        }

        if (string.IsNullOrEmpty(oid))
        {
            oid = _userManager.CompanyId;
        }

        // 1、找出state==2的订单集合
        var orderList = await _repository.Context.Queryable<ErpOrderEntity>()
            .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
            .Where((a, b) => a.Oid == oid && a.State == OrderStateEnum.PendingApproval && ((b.SorterState ?? "0") == "0"))
            .WhereIF(queryPosttime != null, a => SqlFunc.Between(a.Posttime, startPosttime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select((a, b, c) => new ErpBuyorderRecommendDto
            {
                mid = b.Mid,
                needNum = b.Num,
                remark = b.Remark,
                productId = c.Pid,
                ratio = c.Ratio ?? 0,
                //ratio = c.Ratio > 0 ? c.Ratio.Value : 1,
                //baseOrderNum = b.Num * (c.Ratio ?? 1),
                orderNum = b.Num
            }).ToListAsync();

        // 包含特殊入库的数据
        if (input.dataType == 1)
        {
            var q = _repository.Context.Queryable<ErpOutdetailRecordEntity>()
                .Where(ddd => SqlFunc.Subqueryable<ErpInorderEntity>().LeftJoin<ErpInrecordEntity>((t1, t2) => t1.Id == t2.InId).Where((t1, t2) => t1.InType == "5").Any());
            var templist = await _repository.Context.Queryable<ErpOrderEntity>()
           .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
           .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
           .Where((a, b) => a.Oid == oid && ((b.SorterState ?? "0") == "1"))
           .WhereIF(queryPosttime != null, a => SqlFunc.Between(a.Posttime, startPosttime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
           .Where((a, b) => SqlFunc.Subqueryable<ErpOutdetailRecordEntity>()
                .Where(ddd => ddd.OutId == b.Id && SqlFunc.Subqueryable<ErpInorderEntity>().LeftJoin<ErpInrecordEntity>((t1, t2) => t1.Id == t2.InId)
                .Where((t1, t2) => t1.InType == "5" && t2.Id == ddd.InId).Any()).Any())
           .Select((a, b, c) => new ErpBuyorderRecommendDto
           {
               mid = b.Mid,
               needNum = b.Num,
               remark = b.Remark,
               productId = c.Pid,
               ratio = c.Ratio ?? 0,
               //ratio = c.Ratio > 0 ? c.Ratio.Value : 1,
               //baseOrderNum = b.Num * (c.Ratio ?? 1),
               orderNum = b.Num
           }).ToListAsync();

            /*
            var templist = await _repository.Context.Queryable<ErpInorderEntity>()
            .InnerJoin<ErpInrecordEntity>((a, b) => a.Id == b.InId)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Gid == c.Id)
            .Where((a,b,c)=> a.Oid == oid)
            //.WhereIF(queryPosttime != null, a => SqlFunc.Between(a., startPosttime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endPosttime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select((a, b, c) => new ErpBuyorderRecommendDto
            {
                mid = b.Gid,
                needNum = b.InNum,
                remark = b.Remark,
                productId = c.Pid,
                ratio = c.Ratio ?? 0,
                //ratio = c.Ratio > 0 ? c.Ratio.Value : 1,
                //baseOrderNum = b.Num * (c.Ratio ?? 1),
                orderNum = b.InNum
            }).ToListAsync();
            */
            if (templist.IsAny())
            {
                orderList.AddRange(templist);
            }
        }

        if (!orderList.IsAny())
        {
            return new List<ErpProductRecommendOutput>();
        }

        // 获取商品id集合
        var productList = orderList.Select(x => x.productId).Distinct().ToList();


        //orderList.Select(x=>x.mid)
        // 2、计算库存数
        var q1 = _repository.Context.Queryable<ErpInorderEntity>().InnerJoin<ErpInrecordEntity>((a, b) => a.Id == b.InId)
            .Where((a, b) => a.Oid == oid && b.Num > 0)
            .Where((a, b) => SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(m => m.Id == b.Gid && productList.Contains(m.Pid)).Any())
            .Select((a, b) => new ErpBuyorderRecommendDto
            {
                mid = b.Gid,
                storeNum = b.Num,
                srcStoreNum = b.Num
            });

        var q2 = _repository.Context.Queryable<ErpBuyorderEntity>().InnerJoin<ErpBuyorderdetailEntity>((a, b) => a.Id == b.Fid)
            .Where((a, b) => a.Oid == oid && SqlFunc.Subqueryable<ErpInrecordEntity>().Where(c => c.Bid == b.Id).NotAny())
            .Where((a, b) => SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(m => m.Id == b.Gid && productList.Contains(m.Pid)).Any())
            .Select((a, b) => new ErpBuyorderRecommendDto
            {
                mid = b.Gid,
                storeNum = b.PlanNum,
                srcStoreNum = 0
            });

        var storeList = await _repository.Context.UnionAll(q1, q2).GroupBy(x => x.mid)
            .Select(x => new ErpBuyorderRecommendDto
            {
                mid = x.mid,
                storeNum = SqlFunc.AggregateSum(x.storeNum),
                srcStoreNum = SqlFunc.AggregateSum(x.srcStoreNum)
            }).ToListAsync();

        //var midList = orderList.Select(x => x.mid).Distinct().ToList();
        //storeList =storeList.Where(it => midList.Contains(it.mid)).ToList();

        #region 排除掉有库存的数据
        //foreach (var item in orderList.GroupBy(x=>x.mid))
        //{
        //    var store = storeList.Find(x => x.mid == item.Key);
        //    if (store != null)
        //    {
        //        // 分配库存数
        //        for (int i = 0; i < item.Count(); i++)
        //        {
        //            var order = item.ElementAt(i);

        //            if (store.storeNum >= order.orderNum)
        //            {
        //                store.storeNum = store.storeNum - order.orderNum;
        //                orderList.RemoveAt(i);
        //            }
        //            else
        //            {
        //                order.orderNum = order.orderNum - store.storeNum;
        //                store.storeNum = 0;
        //            }

        //        }

        //        // 没有库存了就删掉
        //        if (storeNum == 0)
        //        {
        //            storeList.Remove(store);
        //        }
        //    }
        //}

        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            var order = orderList[i];
            var store = storeList.Find(x => x.mid == order.mid && x.storeNum > 0);
            // 没有库存
            if (store == null)
            {
                continue;
            }
            if (store.storeNum >= order.needNum)
            {
                store.storeNum = store.storeNum - order.needNum;
                order.needNum = 0;
                //orderList.RemoveAt(i);
            }
            else
            {
                order.needNum = order.needNum - store.storeNum;
                store.storeNum = 0;
            }

            // 设置库存的单位换算比
            store.ratio = order.ratio;
            store.baseStoreNum = store.ratio * store.storeNum;
            store.productId = order.productId;

            // 库存没了就清掉
            if (store.storeNum == 0)
            {
                //storeList.Remove(store);
            }

            if (storeList.Count == 0)
            {
                break;
            }
        }
        #endregion

        // 如果订单没有数据了，直接返回
        if (!orderList.Where(x => x.needNum > 0).IsAny())
        {
            return new List<ErpProductRecommendOutput>();
        }

        #region 不走换算判断，结果已经跟视图的方式一样 2024.3.26
        // 转换成最小单位汇总计算 
        var baseOrderList = orderList.Where(x => x.needNum > 0 && x.ratio > 0).ToList();
        foreach (var item in baseOrderList)
        {
            item.baseNeedNum = item.needNum * item.ratio;
        }

        foreach (var item in baseOrderList.GroupBy(x => x.productId))
        {
            //库存记录
            var baseStoreList = storeList.Where(x => x.productId == item.Key).ToList();
            foreach (var xitem in item)
            {
                while (xitem.baseNeedNum > 0)
                {
                    var store = storeList.Find(x => x.productId == xitem.productId && x.storeNum > 0);

                    // 没有库存
                    if (store == null)
                    {
                        break;
                    }
                    if (store.baseStoreNum >= xitem.baseNeedNum)
                    {
                        store.baseStoreNum = store.baseStoreNum - xitem.baseNeedNum;
                        xitem.baseNeedNum = 0;
                        xitem.needNum = 0;
                        //orderList.RemoveAt(i);
                    }
                    else
                    {
                        xitem.baseNeedNum = xitem.baseNeedNum - store.baseStoreNum;
                        store.baseStoreNum = 0;
                        store.storeNum = 0;
                        xitem.useBase = true;
                    }

                    // 库存没了就清掉
                    if (store.baseStoreNum == 0)
                    {
                        //storeList.Remove(store);
                    }

                    if (storeList.Count == 0)
                    {
                        break;
                    }
                }
            }
        }

        // 判断是否需要使用基本单位
        var except = baseOrderList.Where(x => x.needNum > 0 && x.useBase).ToList();
        var productIdList = except.Select(x => x.productId).ToList();
        if (productIdList.IsAny())
        {
            var baseList = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(x => productIdList.Contains(x.Pid) && x.Ratio == 1).ToListAsync();

            //List<ErpBuyorderRecommendDto> except = new List<ErpBuyorderRecommendDto>();
            // 替换 orderList 的数据
            foreach (var item in except)
            {
                var baseEntity = baseList.Find(x => x.Pid == item.productId);
                if (baseEntity != null)
                {
                    orderList.Add(new ErpBuyorderRecommendDto
                    {
                        mid = baseEntity.Id,
                        productId = baseEntity.Pid,
                        needNum = item.baseNeedNum,
                        orderNum = item.orderNum,  // * item.ratio,
                        remark = item.remark
                    });
                }
            }

            // 排除旧的
            orderList = orderList.Except(except).ToList();
        }

        #endregion





        //// 获取采购建议
        //var recommendList = await _repository.Context.Queryable<ViewErpProductRecommend>().Where(it => it.Oid == oid).ToListAsync();

        //if (recommendList == null || !recommendList.Any())
        //{
        //    return new List<ErpProductRecommendOutput>();
        //}

        var arr = orderList.Where(x => x.needNum > 0).Select(x => x.mid).ToArray();

        var data = await _repository.Context.Queryable<ErpProductmodelEntity>()
           .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
           .Where((a, b) => b.State == "1")
           .Where((a, b) => arr.Contains(a.Id))
           .Select((a, b) => new ErpProductRecommendOutput
           {
               id = a.Id,
               name = a.Name,
               costPrice = a.CostPrice,
               grossMargin = a.GrossMargin,
               minNum = a.MinNum,
               num = 0,// a.Num,
               package = a.Package,
               productCode = b.No,
               productName = b.Name,
               productUnit = b.Unit,
               maxNum = a.MaxNum > 0 ? a.MaxNum : 9999999,
               supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == b.Supplier).Select(x => x.Name),
               rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
           }).ToListAsync();

        foreach (var item in data)
        {
            var entitys = orderList.Where(x => x.mid == item.id).ToList();
            if (entitys.IsAny())
            {
                item.num = entitys.Sum(a => a.needNum);
                item.storeNum = storeList.Where(x => x.mid == item.id).Sum(a => a.srcStoreNum);

                item.remark = string.Join("+", entitys.Select(x => string.IsNullOrEmpty(x.remark) ? $"{float.Parse(x.orderNum.ToString())}" : $"{float.Parse(x.orderNum.ToString())}({x.remark})"));

            }
        }
        return data;
    }


    #region 导入明细数据
    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public dynamic TemplateDownload()
    {
        // 初始化 一条空数据 
        List<ErpBuyorderDetailListImportDataInput>? dataList = new List<ErpBuyorderDetailListImportDataInput>() { new ErpBuyorderDetailListImportDataInput() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("采购明细导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpBuyorderDetailListImportDataInput>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        ExcelExportHelper<ErpBuyorderDetailListImportDataInput>.Export(dataList, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 上传文件.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Uploader")]
    public async Task<dynamic> Uploader(IFormFile file)
    {
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        var flag = await _fileManager.UploadFileByType(stream, _filePath, _fileName);
        return new { name = _fileName, url = flag.Item2 ?? string.Format("/api/File/Image/{0}/{1}", string.Empty, _fileName) };
    }

    /// <summary>
    /// 导入预览.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ImportPreview")]
    public async Task<dynamic> ImportPreview(string fileName)
    {
        try
        {
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpBuyorderDetailListImportDataInput>();

            string? filePath = Path.Combine(FileVariable.TemporaryFilePath, fileName.Replace("@", "."));
            using (var stream = (await _fileManager.DownloadFileByType(filePath, fileName))?.FileStream)
            {
                //var excelData1 = ExcelImportHelper.ToDataTable(stream,true);
                // 得到数据
                var excelData = ExcelImportHelper.ToDataTable(stream, ExcelImportHelper.IsXls(fileName));
                foreach (DataColumn item in excelData.Columns)
                {
                    excelData.Columns[item.ToString()].ColumnName = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                }

                // 返回结果
                return new { dataRow = excelData };
            }

            //string? filePath = FileVariable.TemporaryFilePath;
            //string? savePath = Path.Combine(filePath, fileName);

            //// 得到数据
            //global::System.Data.DataTable? excelData = ExcelImportHelper.ToDataTable(savePath);
            //foreach (object? item in excelData.Columns)
            //{
            //    excelData.Columns[item.ToString()].ColumnName = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
            //}
            ////var dt = excelData.Clone();
            ////foreach (var g in excelData.AsEnumerable().GroupBy(x => new { name = x["name"], unit = x["unit"] }))
            ////{
            ////    var newRow = dt.NewRow();
            ////    newRow["name"] = g.Key.name;
            ////    newRow["unit"] = g.Key.unit;
            ////    g.GroupBy(x => x["num"]);
            ////}
            //// 返回结果
            //return new { dataRow = excelData };
        }
        catch (Exception e)
        {
            throw Oops.Oh(ErrorCode.D1801);
        }
    }

    /// <summary>
    /// 导出错误报告.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ExportExceptionData")]
    public async Task<dynamic> ExportExceptionData([FromBody] ImportDataInput<ErpBuyorderDetailListImportDataInput> list)
    {
        object[]? res = await ImportData(list.list);

        // 错误数据
        List<ErpBuyorderDetailListImportDataInput>? errorlist = res.Last() as List<ErpBuyorderDetailListImportDataInput>;

        ExcelConfig excelconfig = ExcelConfig.Default("采购明细导入错误报告.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpBuyorderDetailListImportDataInput>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        ExcelExportHelper<ErpBuyorderDetailListImportDataInput>.Export(errorlist, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    public async Task<dynamic> ImportData([FromBody] ImportDataInput<ErpBuyorderDetailListImportDataInput> list)
    {
        object[]? res = await ImportData(list.list);
        List<ErpBuyorderdetailInfoOutput>? addlist = res.First() as List<ErpBuyorderdetailInfoOutput>;
        List<ErpBuyorderDetailListImportDataInput>? errorlist = res.Last() as List<ErpBuyorderDetailListImportDataInput>;
        return new ImportResultOutput<ErpBuyorderdetailInfoOutput, ErpBuyorderDetailListImportDataInput>() { snum = addlist.Count, fnum = errorlist.Count, successResult = addlist, failResult = errorlist, resultType = errorlist.Count < 1 ? 0 : 1 };
    }


    /// <summary>
    /// 导入用户数据函数.
    /// </summary>
    /// <param name="list">list.</param>
    /// <returns>[成功列表,失败列表].</returns>
    private async Task<object[]> ImportData(List<ErpBuyorderDetailListImportDataInput> list)
    {
        List<ErpBuyorderDetailListImportDataInput> userInputList = list;

        #region 初步排除错误数据

        if (userInputList == null || userInputList.Count() < 1)
            throw Oops.Oh(ErrorCode.D5019);

        // 必填字段验证 (账号，姓名，所属组织)
        List<ErpBuyorderDetailListImportDataInput>? errorList = userInputList.Where(x => !x.name.IsNotEmptyOrNull() || !x.num.IsNotEmptyOrNull() || !x.unit.IsNotEmptyOrNull()).ToList();



        errorList = errorList.Distinct().ToList();
        userInputList = userInputList.Except(errorList).ToList();

        List<ErpBuyorderdetailInfoOutput> repositoryList = new List<ErpBuyorderdetailInfoOutput>();
        if (userInputList.Any())
        {
            // 判断商品+规格是否存在

            var q = _repository.Context.Queryable<ErpProductmodelEntity>()
                .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
                .Where((a, b) => b.State == "1")
                .Select((a, b) => new ErpBuyorderdetailInfoOutput
                {
                    gid = a.Id,
                    gidName = a.Name,
                    planNum = 0,
                    price = a.CostPrice,
                    remark = "",
                    productName = b.Name,
                    supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == b.Supplier).Select(x => x.Name)
                }).MergeTable();

            Expression<Func<ErpBuyorderdetailInfoOutput, bool>>? where = null;

            foreach (var item in userInputList)
            {
                Expression<Func<ErpBuyorderdetailInfoOutput, bool>> x = (it) => it.productName == item.name && it.gidName == item.unit;

                where = where == null ? x : where.Or(x);
            }

            repositoryList = await q.Where(where).ToListAsync();

            foreach (var item in userInputList)
            {
                //设置数量
                var entity = repositoryList.Find(x => x.productName == item.name && x.gidName == item.unit);
                if (entity != null)
                {
                    entity.planNum += (item.num ?? 0);
                    entity.remark += item.remark;
                }
                else
                {
                    // 没有商品信息的记录
                    errorList.Add(item);
                }
            }
        }



        #endregion

        return new object[] { repositoryList, errorList };
    }
    #endregion

    #region 导出明细数据
    /// <summary>
    /// 导出Excel.
    /// 有固定的模板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpBuyorderListQueryInput input, [FromServices] IDictionaryDataService dictionaryDataService)
    {
        List<DateTime> queryTaskBuyTime = input.taskBuyTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startTaskBuyTime = queryTaskBuyTime?.First();
        DateTime? endTaskBuyTime = queryTaskBuyTime?.Last();

        List<string> ids = new List<string>();

        if (input.dataType == 2)
        {
            ids = input.items.Split(",", true).ToList();
        }
        else if (input.dataType == 0)
        {
            var tempData = await _repository.Context.Queryable<ErpBuyorderEntity>()
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
            .OrderBy(it => it.TaskBuyTime, OrderByType.Desc)
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
            .Select((it, a, b, c) => new ErpBuyorderExportDetailOutput
            {
                no = it.No,
                //taskToUserId = it.TaskToUserId,
                taskBuyTime = it.TaskBuyTime,
                taskRemark = it.TaskRemark,
                taskToUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.TaskToUserId).Select(d => d.RealName),
                state = it.State == "1" ? "已完成" : "未完成",
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),

                amount = a.Amount,
                //buyTime = a.BuyTime,
                channel = a.Channel,
                gidName = b.Name,
                num = a.Num,
                payment = a.Payment,
                planNum = a.PlanNum,
                price = a.Price,
                productName = c.Name,
                remark = a.Remark,
                supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == a.Supplier).Select(x => x.Name),
                storeNum = a.StoreNum,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == c.Tid).Select(ddd => ddd.RootName),
                gidUnit = b.Unit
            });

        //var data = input.dataType == 0 ? await query.ToPageListAsync(input.currentPage, input.pageSize) : await query.ToListAsync();
        var data = await query.ToListAsync();

        // 处理单位
        var unitOptions = await dictionaryDataService.GetList("JLDW");
        foreach (var item in data)
        {
            if (item.gidUnit.IsNotEmptyOrNull())
            {
                var unit = unitOptions.Find(x => x.EnCode == item.gidUnit);
                if (unit != null)
                {
                    item.gidUnit = unit.FullName;
                }
            }
        }

        ExcelConfig excelconfig = ExcelConfig.Default($"采购明细导出_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xls");
        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);

        excelconfig.ColumnModel = Common.Extension.Extensions.GetExcelColumnModels<ErpBuyorderExportDetailOutput>();

        var fs = ExcelExportHelper<ErpBuyorderExportDetailOutput>.ExportMemoryStream(data, excelconfig);
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
    /// 导出Excel.
    /// 有固定的模板.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("ExportExcel/Detail")]
    public async Task<dynamic> ExportExcelDetail([FromBody] List<ErpBuyorderExportDetailInfoOutput> list)
    {
        if (!list.IsAny())
        {
            throw Oops.Oh(ErrorCode.COM1007);
        }
        ExcelConfig excelconfig = ExcelConfig.Default($"采购导出明细_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xls");
        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);

        excelconfig.ColumnModel = Common.Extension.Extensions.GetExcelColumnModels<ErpBuyorderExportDetailInfoOutput>();

        var fs = ExcelExportHelper<ErpBuyorderExportDetailInfoOutput>.ExportMemoryStream(list, excelconfig);
        var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }
        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT" + "|" + _userManager.TenantId) };
    }
    #endregion


    /// <summary>
    /// 获取当天采购任务汇总.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Day")]
    public async Task<dynamic> GetDayList([FromQuery] PageInputBase input)
    {
        DateTime? startTaskBuyTime = DateTime.Now;
        var data = await _repository.Context.Queryable<ErpBuyorderEntity>()
            .InnerJoin<ErpBuyorderdetailEntity>((a, b) => a.Id == b.Fid)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Gid == c.Id)
            .Where(a => SqlFunc.Between(a.TaskBuyTime, startTaskBuyTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), startTaskBuyTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .Select((a, b, c) => new ErpBuyorderDayOutput
            {
                id = b.Id,
                //oid = it.Oid,
                no = a.No,
                taskToUserId = a.TaskToUserId,
                //taskBuyTime = it.TaskBuyTime,
                //taskRemark = it.TaskRemark,
                taskToUserName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == a.TaskToUserId).Select(d => d.RealName),
                //state = it.State ?? "0",
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == a.Oid).Select(d => d.FullName),
                //buyerName = 
                channel = b.Channel,
                supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(d => d.Id == b.Supplier).Select(d => d.Name),
                planNum = b.PlanNum,
                gidName = c.Name,
                productName = SqlFunc.Subqueryable<ErpProductEntity>().Where(d => d.Id == c.Pid).Select(d => d.Name),
            })
            .ToPagedListAsync(input.currentPage, input.pageSize);

        // 获取采购员
        var idList = data.list?.Select(x => x.id).ToList() ?? new List<string>();

        if (idList.IsAny())
        {
            var users = await _repository.Context.Queryable<ErpBuyorderUserEntity>().Where(it => idList.Contains(it.Bid))
            .Select(it => new
            {
                it.Bid,
                Name = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.Uid).Select(d => d.RealName)
            }).ToListAsync();

            foreach (var item in data.list)
            {
                var list = users.Where(it => it.Bid == item.id).Select(it => it.Name).ToList();
                if (!string.IsNullOrEmpty(item.taskToUserName))
                {
                    list.Add(item.taskToUserName);
                }
                item.taskToUserName = string.Join(",", list.Distinct());
            }
        }

        return PageResult<ErpBuyorderDayOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 退回初始状态
    /// </summary>
    /// <returns></returns>
    [HttpPost("actions/{id}/sendBackInit")]
    [SqlSugarUnitOfWork]
    public async Task SendBackInit(string id)
    {
        var entity = await _repository.Context.Queryable<ErpBuyorderEntity>().Where(it => it.Id == id).FirstAsync() ?? throw Oops.Oh(ErrorCode.COM1005);
        var list = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().Where(it => it.Fid == id).ToListAsync();

        if (entity.State != "1")
        {
            throw Oops.Oh("订单状态异常！");
        }

        _repository.Context.Tracking(entity);
        entity.State = "0";


        if (list.IsAny())
        {
            foreach (var item in list)
            {
                _repository.Context.Tracking(item);
                item.BuyState = "0";
            }
            await _repository.Context.Updateable<ErpBuyorderdetailEntity>(list).ExecuteCommandAsync();
        }
        await _repository.Context.Updateable<ErpBuyorderEntity>(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新采购明细的单价金额
    /// </summary>
    /// <returns></returns>
    [HttpPut("actions/detail/{id}/amount")]
    [OperateLog("采购订单", "更新单价金额")]
    [SqlSugarUnitOfWork]
    public async Task UpdateDetailAmount(string id, [FromBody] ErpBuyorderdetailUpAmountInput input)
    {
        var detail = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(detail);
        detail.Price = input.price;
        detail.Amount = input.amount;

        if (input.isFree == 1)
        {
            detail.Price = 0;
            detail.Amount = 0;
            detail.IsFree = 1;
        }

        await _repository.Context.Updateable(detail).ExecuteCommandAsync();
        //更新采购单的单价和金额，采购单总金额
        //更新入库记录的单价和金额
        //更新特殊入库的单价和金额
        //更新出库成本

        //1、入库记录(采购入库和特殊入库)
        //var indetail = await _repository.Context.Queryable<ErpInrecordEntity>().SingleAsync(x => x.Id == detail.Id);
        var indetailList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => x.Id == detail.Id || SqlFunc.Subqueryable<ErpInrecordTsEntity>().Where(t => t.TsId == x.Id && t.InId == detail.Id).Any()).ToListAsync();

        if (indetailList.IsAny())
        {
            //// 合计特殊入库的金额 （关联采购入库单）
            var tsIdList = indetailList.Select(x => x.Id).ToArray();
            var ts_amount_sum = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                .InnerJoin<ErpBuyorderdetailEntity>((a, b) => a.InId == b.Id)
                .Where((a, b) => tsIdList.Contains(a.TsId))
                .Select((a, b) => new
                {
                    a.TsId,  // 特殊入库明细id
                    Num = a.Num,  // 关联特殊入库的数量
                    Amount = SqlFunc.Round(SqlFunc.IIF(a.Num == b.TsNum, b.Amount, a.Num * b.Price), 2),  // 特殊入库的金额
                    a.InId  // 采购明细id
                }).ToListAsync();

            decimal total = 0;
            foreach (var item in indetailList)
            {
                _repository.Context.Tracking(item);

                //if (ts_amount_sum.Any(x=>x.TsId == item.Id))
                //{
                //    // 特殊入库的数据，取合计值
                //    item.Amount = ts_amount_sum.Where(x => x.TsId == item.Id).Sum(x => x.Amount);
                //    item.Price = item.InNum == 0 ? 0 : Math.Round(item.Amount / item.InNum, 2);
                //}
                //else
                {
                    item.Price = detail.Price;
                    item.Amount = item.InNum * detail.Price;
                }                   

                total += item.Amount;
            }
            // 把剩余的金额放到最后一条（入库数量不为0的）
            if (total != detail.Amount)
            {
                var last = indetailList.LastOrDefault(x => x.InNum > 0);
                if (last != null)
                {
                    last.Amount += (detail.Amount - total);
                }
            }

            // 特殊入库的记录要合计其他的数据
            if (ts_amount_sum.IsAny())
            {
                foreach (var item in indetailList)
                {
                    if (ts_amount_sum.Any(x => x.TsId == item.Id))
                    {
                        var ots_amount = ts_amount_sum.Where(x => x.TsId == item.Id && x.InId != id).Sum(x => x.Amount); //特殊关联的特殊入库金额
                        item.Amount += ots_amount;
                        item.Price = item.InNum == 0 ? 0 : Math.Round(item.Amount / item.InNum, 2);
                    }
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
                        amount += (indetail.InNum == record.Num ? indetail.Amount : record.Num * indetail.Price);
                    }
                }
                _repository.Context.Tracking(item);
                item.CostAmount = amount;
            }

            await _repository.Context.Updateable<ErpOutrecordEntity>(outdetailList).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 批量改价（根据供应商定价）
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/{id}/EditPrice/Batch")]
    [SqlSugarUnitOfWork]
    public async Task BatchEditPriceBySupplier(string id, [FromBody] List<string> idList)
    {
        var ErpBuyorderdetailEntitys = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
            .Where(x => x.Fid == id && idList.Contains(x.Id))
            .Select(x => new ErpBuyorderdetailEntity
            {
                Id = x.Id,
                Supplier = x.Supplier,
                Price = x.Price,
                InNum = x.InNum,
                TsNum = x.TsNum,
                Amount = x.Amount,
                Gid = x.Gid
            })
            .ToListAsync();

        // 获取供应商的默认价格
        var Suppliers = ErpBuyorderdetailEntitys.Where(x => x.Supplier.IsNotEmptyOrNull()).Select(x => x.Supplier).Distinct().ToArray();

        var supplierPriceEntitys = await _repository.Context.Queryable<ErpSupplierpriceEntity>().Where(it => Suppliers.Contains(it.SupplierId) && it.PricingType == 2).ToListAsync();

        List<ErpBuyorderdetailUpAmountInput> updateList = new List<ErpBuyorderdetailUpAmountInput>();
        foreach (var item in ErpBuyorderdetailEntitys)
        {
            var priceEntity = supplierPriceEntitys.Find(x => x.SupplierId == item.Supplier && x.Gid == item.Gid);
            if (priceEntity != null && priceEntity.PricingType == 2 && priceEntity.Price > 0 && priceEntity.Price != item.Price)
            {
                // 执行改价
                updateList.Add(new ErpBuyorderdetailUpAmountInput
                {
                    id = item.Id,
                    price = priceEntity.Price,
                    amount = Math.Round(((item.InNum ?? 0) + item.TsNum) * (priceEntity.Price), 2),
                });
            }
        }

        if (updateList.IsAny())
        {
            foreach (var item in updateList)
            {
                await UpdateDetailAmount(item.id, item);
            }
        }

    }

    /// <summary>
    /// 批量改价（根据采购单）
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/EditPrice/Batch")]
    [SqlSugarUnitOfWork]
    public async Task BatchEditPrice([FromBody] List<string> idList)
    {
        var ErpBuyorderdetailEntitys = await _repository.Context.Queryable<ErpBuyorderdetailEntity>()
            .Where(x =>  idList.Contains(x.Fid))
            .Select(x => new ErpBuyorderdetailEntity
            {
                Id = x.Id,
                Supplier = x.Supplier,
                Price = x.Price,
                InNum = x.InNum,
                TsNum = x.TsNum,
                Amount = x.Amount,
                Gid = x.Gid
            })
            .ToListAsync();

        // 获取供应商的默认价格
        var Suppliers = ErpBuyorderdetailEntitys.Where(x => x.Supplier.IsNotEmptyOrNull()).Select(x => x.Supplier).Distinct().ToArray();

        var supplierPriceEntitys = await _repository.Context.Queryable<ErpSupplierpriceEntity>().Where(it => Suppliers.Contains(it.SupplierId) && it.PricingType == 2).ToListAsync();

        List<ErpBuyorderdetailUpAmountInput> updateList = new List<ErpBuyorderdetailUpAmountInput>();
        foreach (var item in ErpBuyorderdetailEntitys)
        {
            var priceEntity = supplierPriceEntitys.Find(x => x.SupplierId == item.Supplier && x.Gid == item.Gid);
            if (priceEntity != null && priceEntity.PricingType == 2 && priceEntity.Price > 0 && priceEntity.Price != item.Price)
            {
                // 执行改价
                updateList.Add(new ErpBuyorderdetailUpAmountInput
                {
                    id = item.Id,
                    price = priceEntity.Price,
                    amount = Math.Round(((item.InNum ?? 0) + item.TsNum) * (priceEntity.Price), 2),
                });
            }
        }

        if (updateList.IsAny())
        {
            foreach (var item in updateList)
            {
                await UpdateDetailAmountV2(item.id, item);
            }
        }

    }

    /// <summary>
    /// 更新采购明细的单价金额
    /// </summary>
    /// <returns></returns>
    public async Task UpdateDetailAmountV2(string id, [FromBody] ErpBuyorderdetailUpAmountInput input)
    {
        var detail = await _repository.Context.Queryable<ErpBuyorderdetailEntity>().SingleAsync(x => x.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(detail);
        detail.Price = input.price;
        detail.Amount = input.amount;
        _repository.Context.Updateable<ErpBuyorderdetailEntity>(detail).AddQueue();
        //更新采购单的单价和金额，采购单总金额
        //更新入库记录的单价和金额
        //更新特殊入库的单价和金额
        //更新出库成本

        //1、入库记录(采购入库和特殊入库)
        //var indetail = await _repository.Context.Queryable<ErpInrecordEntity>().SingleAsync(x => x.Id == detail.Id);
        var indetailList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => x.Id == detail.Id || SqlFunc.Subqueryable<ErpInrecordTsEntity>().Where(t => t.TsId == x.Id && t.InId == detail.Id).Any()).ToListAsync();

        if (indetailList.IsAny())
        {
            //// 合计特殊入库的金额 （关联采购入库单）
            var tsIdList = indetailList.Select(x => x.Id).ToArray();
            var ts_amount_sum = await _repository.Context.Queryable<ErpInrecordTsEntity>()
                .InnerJoin<ErpBuyorderdetailEntity>((a, b) => a.InId == b.Id)
                .Where((a, b) => tsIdList.Contains(a.TsId))
                .Select((a, b) => new
                {
                    a.TsId,  // 特殊入库明细id
                    Num = a.Num,  // 关联特殊入库的数量
                    Amount = SqlFunc.Round(SqlFunc.IIF(a.Num == b.TsNum, b.Amount, a.Num * b.Price), 2),  // 特殊入库的金额
                    a.InId  // 采购明细id
                }).ToListAsync();

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
                    last.Amount += (detail.Amount - total);
                }
            }

            // 特殊入库的记录要合计其他的数据
            if (ts_amount_sum.IsAny())
            {
                foreach (var item in indetailList)
                {
                    if (ts_amount_sum.Any(x => x.TsId == item.Id))
                    {
                        var ots_amount = ts_amount_sum.Where(x => x.TsId == item.Id && x.InId != id).Sum(x => x.Amount); //特殊关联的特殊入库金额
                        item.Amount += ots_amount;
                        item.Price = item.InNum == 0 ? 0 : Math.Round(item.Amount / item.InNum, 2);
                    }
                }
            }

            // 更新数据，获取主表记录
            foreach (var item in indetailList)
            {
                _repository.Context.Updateable<ErpInrecordEntity>(item).AddQueue();
            }

            var inrecordothers = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => indetailList.Any(t => t.InId == x.InId) && !indetailList.Any(t => t.Id == x.Id))
                 .Select(x => new ErpInrecordEntity
                 {
                     Id = x.Id,
                     InId = x.InId,
                     Amount = x.Amount
                 })
                 .ToListAsync();

            var inmainList = inrecordothers.Concat(indetailList).GroupBy(x => x.InId)
                 .Select(x => new ErpInorderEntity
                 {
                     Id = x.Key,
                     Amount =x.Sum(g=>g.Amount)
                 })
                 .ToList();
            //var inmainList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => indetailList.Any(t => t.InId == x.InId) && !indetailList.Any(t => t.Id == x.Id)) 
            //     .GroupBy(x => x.InId)
            //     .Select(x => new ErpInorderEntity
            //     {
            //         Id = x.InId,
            //         Amount = SqlFunc.AggregateSum(x.Amount)
            //     })
            //     .ToListAsync();
            foreach (var item in inmainList)
            {
                _repository.Context.Updateable<ErpInorderEntity>(item).UpdateColumns(x => new { x.Amount }).AddQueue();
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
                        amount += (indetail.InNum == record.Num ? indetail.Amount : record.Num * indetail.Price);
                    }
                }
                _repository.Context.Tracking(item);
                item.CostAmount = amount;

                _repository.Context.Updateable<ErpOutrecordEntity>(item).AddQueue();
            } 
            
        }

        await _repository.Context.SaveQueuesAsync();
    }
}
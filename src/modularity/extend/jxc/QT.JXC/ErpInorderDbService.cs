using Mapster;
using QT.DependencyInjection;
using QT.JXC.Interfaces;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：调拨入库表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpInorderDb", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpInorderDbService : IDynamicApiController, ITransient
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

    /// <summary>
    /// 初始化一个<see cref="ErpInorderService"/>类型的新实例.
    /// </summary>
    public ErpInorderDbService(
        ISqlSugarRepository<ErpInorderEntity> erpInorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IErpStoreService erpStoreService)
    {
        _repository = erpInorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _erpStoreService = erpStoreService;
    }


    /// <summary>
    /// 新建调拨入库表.
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

        //调拨出库实体
        ErpOutorderEntity outEntity = new()
        {
            Id = entity.Id,
            InType = "2", //调拨出库
            No = await _billRullService.GetBillNumber("QTErpOutOrder"),
            Oid = input.outOid
        };


        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<ErpInorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            //调拨出库实体
            var outNewEntity = await _repository.Context.Insertable<ErpOutorderEntity>(outEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var erpInrecordEntityList = new List<ErpInrecordEntity>();
            var erpOutrecordEntityList = new List<ErpOutrecordEntity>();
            if (input.erpInrecordList != null)
            {
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

                    // 生成调拨出库
                    var outItem = x.Adapt<ErpOutrecordCrInput>().Adapt<ErpOutrecordEntity>();
                    outItem.Id = item.Id;
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
                            num = item.Num,
                            records = x.storeDetailList.Adapt<List<ErpOutdetailRecordInInput>>()
                        });

                        outItem.CostAmount = cost.CostAmount;
                    }
                }

                await _repository.Context.Insertable(erpInrecordEntityList).ExecuteCommandAsync();

                //插入出库明细
                await _repository.Context.Insertable(erpOutrecordEntityList).ExecuteCommandAsync();
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
    /// 这个删除调出生成调入的操作
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


        // 对应的调出单状态为2才能删除
        var outEntity = await _repository.Context.Queryable<ErpOutorderEntity>().ClearFilter<ICompanyEntity>().InSingleAsync(id);
        if (outEntity!=null && outEntity.State != "2")
        {
            throw Oops.Oh($"对应的调出订单[{outEntity.No}]已同意，不允许删除调入订单！");
        }


        // 删除主表
        await _repository.Context.Deleteable<ErpInorderEntity>().Where(it => it.Id.Equals(id)).EnableDiffLogEvent().ExecuteCommandAsync();

        // 清空商品入库记录表数据
        await _repository.Context.Deleteable<ErpInrecordEntity>().Where(it => it.InId.Equals(id)).EnableDiffLogEvent().ExecuteCommandAsync();


        if (outEntity != null)
        {
            // 更新调出单状态为“1”
            outEntity.State = "1";
            await _repository.Context.Updateable<ErpOutorderEntity>(outEntity).UpdateColumns(it => it.State).ExecuteCommandAsync();
        }

    }

    /// <summary>
    /// 删除入库订单表.
    /// 这个删除，是调入生成调出的操作
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [NonAction]
    [SqlSugarUnitOfWork]
    private async Task DeleteV0(string id)
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

            //删除调拨出库主表记录
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
            await _repository.Context.Deleteable<ErpOutrecordEntity>(records).ExecuteCommandAsync();
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
}

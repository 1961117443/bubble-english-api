using Mapster;
using QT.DependencyInjection;
using QT.JXC.Interfaces;
using QT.Systems.Interfaces.System;

namespace QT.JXC;

/// <summary>
/// 业务实现：拆包入库表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpInorderCb", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpInorderCbService : IDynamicApiController, ITransient
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
    public ErpInorderCbService(
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
    /// 新建拆包入库表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpInorderCbCrInput input)
    {
        var entity = input.Adapt<ErpInorderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpCBRK");
        entity.InType = "7";
        //拆包出库实体
        ErpOutorderEntity outEntity = new()
        {
            Id = entity.Id,
            InType = "6", //拆包出库
            //No = entity.No, // 同一个流水号，方便查找 // await _billRullService.GetBillNumber("QTErpJGCK"),
            No = await _billRullService.GetBillNumber("QTErpCBCK"),
            //Oid = input.oid,
            //Remark = $"由加工入库单[{entity.No}]生成"
        };


        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            entity.Amount = input.erpInrecordList.Sum(a => a.amount);
            var newEntity = await _repository.Context.Insertable<ErpInorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            //拆包出库实体
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
                }

                await _repository.Context.Insertable(erpInrecordEntityList).ExecuteCommandAsync();
            }

            // 生成出库记录
            if (input.erpOutrecordList !=null)
            {
                foreach (var x in input.erpOutrecordList)
                {
                    // 生成出库
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
}

using QT.Application.Entitys.Dto.FreshDelivery.ErpInorder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderXs;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOutrecord;
using QT.Application.Entitys.FreshDelivery;
using QT.Application.Interfaces.FreshDelivery;
using QT.Common.Core.Security;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems;
using QT.Systems.Interfaces.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：出库订单表.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "加工出库", Name = "ErpOutorderJg", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpOutorderJgService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 初始化一个<see cref="ErpOutorderService"/>类型的新实例.
    /// </summary>
    public ErpOutorderJgService(
        ISqlSugarRepository<ErpInorderEntity> erpInorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        IErpStoreService erpStoreService)
    {
        _repository = erpInorderRepository;
        _billRullService = billRullService;
        _context = context;
        _userManager = userManager;
        _erpStoreService = erpStoreService;
    }

    private readonly IBillRullService _billRullService;
    private readonly ISqlSugarClient _context;
    private readonly IUserManager _userManager;
    private readonly IErpStoreService _erpStoreService;
    private readonly ErpInorderJgService _erpInorderJgService;
    private readonly ISqlSugarRepository<ErpInorderEntity> _repository;

    /// <summary>
    /// 新建调拨入库表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpInorderJgCrInput input)
    {
        // 把出库商品的数量分配到每一条加工入库记录上

        //_erpInorderJgService.Create()

        if (input.erpOutrecordList.Count > 1)
        {
            throw Oops.Oh("加工出库只支持一个批次进行分配");
        }
        var outrecordTotal = input.erpOutrecordList.Sum(x => x.num);
        var inrecordTotal = input.erpInrecordList.Sum(x => x.inNum);
        List<ErpInorderJgCrInput> erpInorderJgCrInputs = new List<ErpInorderJgCrInput>();
        var outrecord = input.erpOutrecordList[0];

        var erpInrecordIds = input.erpInrecordList.Select(x => x.id).ToArray();
        // 获取加工入库单实体
        var erpInrecordEntityList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => erpInrecordIds.Contains(x.Id)).ToListAsync();
        var erpInorderIds = erpInrecordEntityList.Select(x => x.InId).ToArray();
        var erpInorderEntityList = await _repository.Context.Queryable<ErpInorderEntity>().Where(x => erpInorderIds.Contains(x.Id)).ToListAsync();


        decimal use = 0;
        foreach (var inrecord in input.erpInrecordList)
        {
            ErpInorderJgCrInput erpInorderJgCrInput = new ErpInorderJgCrInput();

            input.Adapt(erpInorderJgCrInput);

            ErpOutrecordCrInput erpOutrecordCrInput = new ErpOutrecordCrInput()
            {
                gid = outrecord.gid,
                num = Math.Round(outrecordTotal * inrecord.inNum / inrecordTotal, 2),
                amount = 0,
                price = outrecord.price,
                remark = outrecord.remark,
                storeDetailList = outrecord.storeDetailList
            };
            use += erpOutrecordCrInput.num;      

            erpInorderJgCrInput.erpOutrecordList = new List<ErpOutrecordCrInput>()
            {
                erpOutrecordCrInput
            };
            erpInorderJgCrInput.erpInrecordList = new List<ErpInrecordCrInput>()
            {
                inrecord
            };



            erpInorderJgCrInputs.Add(erpInorderJgCrInput);
        }

        // 判断是否有差异
        var left = outrecordTotal - use;
        if (left!=0)
        {
            // 把数量加到最后一条
            var last = erpInorderJgCrInputs.Last();
            last.erpOutrecordList[0].num += left;
        }

        // 创建对应的出库记录
        foreach (var item in erpInorderJgCrInputs)
        {
            // 找到入库记录
            var inrecordEntity = erpInrecordEntityList.FirstOrDefault(x => x.Id == item.erpInrecordList[0].id);
            if (inrecordEntity!=null)
            {
                var erpInorderEntity = erpInorderEntityList.FirstOrDefault(x => x.Id == inrecordEntity.InId);
                //加工出库实体
                ErpOutorderEntity outEntity = new()
                {
                    Id = erpInorderEntity.Id,
                    InType = "3", //加工出库
                                  //No = entity.No, // 同一个流水号，方便查找 // await _billRullService.GetBillNumber("QTErpJGCK"),
                    No = await _billRullService.GetBillNumber("QTErpJGCK"),
                    Oid = erpInorderEntity.Oid,
                    Remark = $"由加工入库单[{erpInorderEntity.No}]生成"
                };

                //加工出库实体
                var outNewEntity = await _repository.Context.Insertable<ErpOutorderEntity>(outEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();
                List<ErpOutrecordEntity> erpOutrecordEntityList = new List<ErpOutrecordEntity>();
                foreach (var x in item.erpOutrecordList)
                {
                    // 生成加工出库
                    var outItem = x.Adapt<ErpOutrecordCrInput>().Adapt<ErpOutrecordEntity>();
                    outItem.Id = SnowflakeIdHelper.NextId();
                    outItem.OutId = outNewEntity.Id;
                    if (!string.IsNullOrEmpty(erpInorderEntity.Oid))
                    {
                        outItem.Oid = erpInorderEntity.Oid;
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
                await _repository.Context.Insertable<ErpOutrecordEntity>(erpOutrecordEntityList).ExecuteCommandAsync();

                // 更新入库明细的入库单价和金额
                var costAmount = erpOutrecordEntityList.Sum(x => x.CostAmount);
                if (costAmount > 0)
                {
                    inrecordEntity.Price = Math.Round(costAmount / inrecordEntity.InNum, 2);
                    inrecordEntity.Amount = costAmount;
                    await _repository.Context.Updateable<ErpInrecordEntity>(inrecordEntity).UpdateColumns(new string[] { "Price", "Amount" }).ExecuteCommandAsync();
                }
            }
           
        }


    }

    [HttpGet("QueryErpInorderJgList")]
    public async Task<dynamic> QueryErpInorderJgList([FromQuery] ErpOutorderJgListQueryErpInrecordInput input)
    {
        // 查询关联的加工入库记录，并且还没绑定加工出库单（出库单id==入库单id则判断为已绑定）
        List<DateTime> creatorTimeRange = input.creatorTimeRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCreatorTimeDate = creatorTimeRange?.First();
        DateTime? endCreatorTimeDate = creatorTimeRange?.Last();

        var erpInrecordList = await _repository.Context.Queryable<ErpInrecordEntity>()
          .InnerJoin<ErpProductmodelEntity>((w, a) => w.Gid == a.Id)
          .InnerJoin<ErpProductEntity>((w, a, b) => a.Pid == b.Id)
          .Where((w, a, b) => a.Rid == input.rid)
          .Where(w => SqlFunc.Subqueryable<ErpOutorderEntity>().Where(zz => zz.Id == w.InId && zz.InType == "3").NotAny())
          .Where(w => SqlFunc.Subqueryable<ErpInorderEntity>()
          .Where(zz => zz.Id == w.InId && zz.InType == "3")
          .WhereIF(input.oid.IsNotEmptyOrNull(), zz=>zz.Oid == input.oid)
          .WhereIF(creatorTimeRange != null, it => SqlFunc.Between(it.CreatorTime, startCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCreatorTimeDate.ParseToDateTime("yyyy-MM-dd 23:59:59")))
          .Any())
          .Select((w, a, b) => new ErpInrecordInfoOutput
          {
              gidName = a.Name,
              productName = b.Name,
              productPrice = a.SalePrice
          }, true)
          .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<ErpInrecordInfoOutput>.SqlSugarPageResult(erpInrecordList);
    }

}

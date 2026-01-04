using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Interfaces.System;
using QT.Logistics.Entitys.Dto.LogProductcheckMaster;
using QT.Logistics.Entitys.Dto.LogProductcheckRecord;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Logistics.Entitys.Dto.LogEnterpriseProduct;
using System.ComponentModel.DataAnnotations;
using QT.Logistics.Entitys.Dto.LogEnterpriseProductStore;
using QT.Systems.Entitys.Permission;
using NPOI.OpenXmlFormats.Dml;
using Aspose.Cells;
using System.Runtime.CompilerServices;

namespace QT.Logistics;

/// <summary>
/// 业务实现：盘点记录主表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "盘点管理", Name = "LogProductcheckMaster", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogProductcheckMasterService : ILogProductcheckMasterService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogProductcheckMasterEntity> _repository;

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
    private readonly ILogEnterpriseProductStoreService _logEnterpriseProductStoreService;

    /// <summary>
    /// 初始化一个<see cref="LogProductcheckMasterService"/>类型的新实例.
    /// </summary>
    public LogProductcheckMasterService(
        ISqlSugarRepository<LogProductcheckMasterEntity> logProductcheckMasterRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager,
        ILogEnterpriseProductStoreService logEnterpriseProductStoreService)
    {
        _repository = logProductcheckMasterRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
        _logEnterpriseProductStoreService = logEnterpriseProductStoreService;
    }
    #region 增删改查

    /// <summary>
    /// 获取盘点记录主表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogProductcheckMasterInfoOutput>();

        var logProductcheckRecordList = await _repository.Context.Queryable<LogProductcheckRecordEntity>()
            .InnerJoin<LogEnterpriseProductmodelEntity>((w, x) => w.Gid == x.Id)
            .Where(w => w.CId == output.id)
            .Select((w, x) => new LogProductcheckRecordInfoOutput
            {
                gid = w.Gid,
                gidName = x.Name,
                id = w.Id,
                loseNum = w.LoseNum,
                realNum = w.RealNum,
                remark = w.Remark,
                systemNum = w.SystemNum,
                unit = x.Unit,
                productName = SqlFunc.Subqueryable<LogEnterpriseProductEntity>().Where(ddd => ddd.Id == x.Pid).Select(ddd => ddd.Name)
            })
            .ToListAsync();
        output.logProductcheckRecordList = logProductcheckRecordList; //.Adapt<List<LogProductcheckRecordInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取盘点记录主表列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogProductcheckMasterListQueryInput input)
    {
        List<DateTime> queryCheckTime = input.checkTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startCheckTime = queryCheckTime?.First();
        DateTime? endCheckTime = queryCheckTime?.Last();
        var data = await _repository.Context.Queryable<LogProductcheckMasterEntity>()
            .WhereIF(queryCheckTime != null, it => SqlFunc.Between(it.CheckTime, startCheckTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endCheckTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogProductcheckMasterListOutput
            {
                id = it.Id,
                checkTime = it.CheckTime,
                remark = it.Remark,
                storeRomeId = it.StoreRomeId,
                no = it.No,
                storeRomeIdName = SqlFunc.Subqueryable<LogStoreroomEntity>().Where(x=>x.Id==it.StoreRomeId).Select(x=>x.Name),
                auditTime = it.AuditTime,
                auditUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(x=>x.Id == it.AuditUserId).Select(x=>x.RealName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogProductcheckMasterListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建盘点记录主表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogProductcheckMasterCrInput input)
    {
        var entity = input.Adapt<LogProductcheckMasterEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpProductcheck");

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<LogProductcheckMasterEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var logProductcheckRecordEntityList = input.logProductcheckRecordList.Adapt<List<LogProductcheckRecordEntity>>();
            if (logProductcheckRecordEntityList != null)
            {
                foreach (var item in logProductcheckRecordEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.CId = newEntity.Id;
                }

                await _repository.Context.Insertable<LogProductcheckRecordEntity>(logProductcheckRecordEntityList).ExecuteCommandAsync();
            }

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1000);
        }
    }

    /// <summary>
    /// 更新盘点记录主表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogProductcheckMasterUpInput input)
    {
        var entity = input.Adapt<LogProductcheckMasterEntity>();
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogProductcheckMasterEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空盘点记录原有数据
            await _repository.Context.Deleteable<LogProductcheckRecordEntity>().Where(it => it.CId == entity.Id).ExecuteCommandAsync();

            // 新增盘点记录新数据
            var logProductcheckRecordEntityList = input.logProductcheckRecordList.Adapt<List<LogProductcheckRecordEntity>>();
            if (logProductcheckRecordEntityList != null)
            {
                foreach (var item in logProductcheckRecordEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.CId = entity.Id;
                }

                await _repository.Context.Insertable<LogProductcheckRecordEntity>(logProductcheckRecordEntityList).ExecuteCommandAsync();
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
    /// 删除盘点记录主表.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        //if(!await _repository.Context.Queryable<LogProductcheckMasterEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}

        var entity = await _repository.SingleAsync(it => it.Id == id) ?? throw Oops.Oh(ErrorCode.COM1005);

        // 订单物品明细表数据
        var logProductcheckRecords = await _repository.Context.Queryable<LogProductcheckRecordEntity>().Where(it => it.CId == entity.Id).ToListAsync();

        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogProductcheckMasterEntity>(entity).CallEntityMethod(it => it.Delete())
               .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();

            if (logProductcheckRecords.IsAny())
            {
                await _repository.Context.Updateable<LogProductcheckRecordEntity>(logProductcheckRecords).CallEntityMethod(it => it.Delete())
                    .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
            }

            //var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            //await _repository.Context.Deleteable<LogProductcheckMasterEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            //    // 清空盘点记录表数据
            //await _repository.Context.Deleteable<LogProductcheckRecordEntity>().Where(it => it.CId.Equals(entity.Id)).ExecuteCommandAsync();

            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1002);
        }
    }
    #endregion

    /// <summary>
    /// 选择商品信息，分页查询，计算出出库数量 
    /// </summary>
    /// <param name="id">仓库id</param>
    /// <param name="pageInput"></param>
    /// <returns></returns>
    [HttpGet("Actions/StoreRome/{id}")]
    public async Task<dynamic> QueryProductByStoreRomeId([Required, FromRoute] string id, [FromQuery] LogEnterpriseProductListSelectorQueryInput pageInput)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseProductmodelEntity>()
            .LeftJoin<LogEnterpriseProductEntity>((a, b) => a.Pid == b.Id)
            .Where((a, b) => b.State == 1)
            .WhereIF(!string.IsNullOrEmpty(pageInput.keyword), (a, b) => a.Name.Contains(pageInput.keyword)
            || b.Name.Contains(pageInput.keyword) || b.FirstChar.Contains(pageInput.keyword))
            .Select((a, b) => new LogEnterpriseProductListSelectorOutput
            {
                id = a.Id,
                name = a.Name,
                costPrice = a.CostPrice,
                minNum = a.MinNum,
                num = 0,// a.Num,
                productName = b.Name,
                salePrice = a.SalePrice,
                unit = a.Unit,
                maxNum = a.MaxNum > 0 ? a.MaxNum : 9999999,
            }).ToPagedListAsync(pageInput.currentPage, pageInput.pageSize);


        if (data.list.IsAny())
        {
            var storeList = await _logEnterpriseProductStoreService.GetLogEnterpriseProductStoreSumAsync(new LogEnterpriseProductStoreDetailQueryInput
            {
                gids = data.list.Select(it => it.id).ToList(),
                storeRoomId = id
            });


            if (storeList.IsAny())
            {
                foreach (var item in data.list)
                {
                    item.num = storeList.Find(x => x.id == item.id)?.storeNum ?? 0;
                }
            }
        }


        return PageResult<LogEnterpriseProductListSelectorOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 审核盘点记录主表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("Audit/{id}")]
    public async Task Audit(string id, [FromServices] ILogEnterpriseProductStoreService logEnterpriseProductStoreService)
    {
        var entity = await _repository.Context.Queryable<LogProductcheckMasterEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        if (entity.AuditTime.HasValue)
        {
            throw Oops.Oh("单据已审核！");
        }

        // 从表记录
        var slaves = await _repository.Context.Queryable<LogProductcheckRecordEntity>().Where(x => x.CId == entity.Id).ToListAsync();


        // 判断商品的系统库存数量是否发生变动
        var storeList = await logEnterpriseProductStoreService.GetLogEnterpriseProductStoreSumAsync(new LogEnterpriseProductStoreDetailQueryInput
        {
            gids = slaves.Select(x => x.Gid).ToList()
        });

        List<string> changeGidList = new List<string>();
        foreach (var slave in slaves)
        {
            var store = storeList.Find(x => x.id == slave.Gid);
            var storeNum = store?.storeNum ?? 0;
            if (storeNum != slave.SystemNum)
            {
                changeGidList.Add(slave.Gid);
            }
        }

        if (changeGidList.IsAny())
        {
            var changeList = await _repository.Context.Queryable<LogEnterpriseProductEntity>()
                .InnerJoin<LogEnterpriseProductmodelEntity>((a, b) => a.Id == b.Pid)
                .Where((a, b) => changeGidList.Contains(b.Id))
                .Select((a, b) => a.Name).ToListAsync();

            throw Oops.Oh($"下列商品发生变动，请重新做盘点，\r\n{string.Join("，", changeList.Distinct())}");
        }


        #region 【1、生成报损出库和报溢入库单】
        var erpInorder = new LogEnterpriseInorderEntity()
        {
            Id = entity.Id,
            EId = entity.EId,
            No = entity.No,
            InType = "4",
            Remark = $"由盘点单【{entity.No}】审核生成",
            InTime = entity.CheckTime
        };
        var erpOutorder = new LogEnterpriseOutorderEntity()
        {
            Id = entity.Id,
            EId = entity.EId,
            No = entity.No,
            OutType = "4",
            Remark = $"由盘点单【{entity.No}】审核生成",
            OutTime = entity.CheckTime
        };
        // 1、生成报损出库和报溢入库单
        List<LogEnterpriseInrecordEntity> erpInrecords = new List<LogEnterpriseInrecordEntity>();
        List<LogEnterpriseOutrecordEntity> erpOutrecords = new List<LogEnterpriseOutrecordEntity>();
        foreach (var item in slaves)
        {
            //报溢入库
            if (item.LoseNum > 0)
            {
                erpInrecords.Add(new LogEnterpriseInrecordEntity
                {
                    Id = item.Id,
                    Gid = item.Gid,
                    InNum = item.LoseNum,
                    StoreRomeId = item.StoreRomeId ?? entity.StoreRomeId,
                    InId = entity.Id
                });
            }
            else if (item.LoseNum < 0)
            {
                erpOutrecords.Add(new LogEnterpriseOutrecordEntity
                {
                    Id = item.Id,
                    Gid = item.Gid,
                    Num = Math.Abs(item.LoseNum),
                    OutId = entity.Id,
                    StoreRomeId = item.StoreRomeId ?? entity.StoreRomeId
                });
            }
        }
        #endregion
        // 必须先出库，再入库
        try
        {
            // 开启事务
            _db.BeginTran();
            #region 【2、更新数据库】
            // 报损出库，找出入库记录
            if (erpOutrecords.Any())
            {
                //主表
                await _repository.Context.Insertable(erpOutorder).ExecuteCommandAsync();
                //插入报损记录
                await _repository.Context.Insertable(erpOutrecords).ExecuteCommandAsync();
            }

            //插入报损记录
            if (erpInrecords.Any())
            {
                //主表
                await _repository.Context.Insertable(erpInorder).ExecuteCommandAsync();
                //从表
                await _repository.Context.Insertable(erpInrecords).ExecuteCommandAsync();
            }



            // 2、更新单据状态
            entity.AuditTime = DateTime.Now;
            entity.AuditUserId = _userManager.UserId;

            await _repository.Context.Updateable(entity).UpdateColumns(x => new { x.AuditUserId, x.AuditTime }).ExecuteCommandAsync();

            #endregion
            // 关闭事务
            _db.CommitTran();
        }
        catch (Exception)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.COM1002);
        }
    }
}
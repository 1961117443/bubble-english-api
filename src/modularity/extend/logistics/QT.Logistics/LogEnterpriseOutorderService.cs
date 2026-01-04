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
using QT.Logistics.Entitys.Dto.LogEnterpriseOutorder;
using QT.Logistics.Entitys.Dto.LogEnterpriseOutrecord;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Logistics.Entitys.Dto.LogEnterpriseInrecord;

namespace QT.Logistics;

/// <summary>
/// 业务实现：商家商品出库表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "商家商品出库管理", Name = "LogEnterpriseOutorder", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseOutorderService : ILogEnterpriseOutorderService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogEnterpriseOutorderEntity> _repository;

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

    /// <summary>
    /// 初始化一个<see cref="LogEnterpriseOutorderService"/>类型的新实例.
    /// </summary>
    public LogEnterpriseOutorderService(
        ISqlSugarRepository<LogEnterpriseOutorderEntity> logEnterpriseOutorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logEnterpriseOutorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    #region 增删改查
    /// <summary>
    /// 获取商家商品出库表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogEnterpriseOutorderInfoOutput>();

        var logEnterpriseOutrecordList = await _repository.Context
            .Queryable<LogEnterpriseOutrecordEntity>()
            .LeftJoin<LogEnterpriseProductmodelEntity>((w, a) => w.Gid == a.Id)
            .Where(w => w.OutId == output.id)
            .Select((w, a) => new LogEnterpriseOutrecordInfoOutput
            {
                productName = SqlFunc.Subqueryable<LogEnterpriseProductEntity>().Where(it => it.Id == a.Pid).Select(it => it.Name),
                unit = a.Unit,
                amount = w.Amount,
                gid = w.Gid,
                id = w.Id,
                num = w.Num,
                gidName = a.Name,
                price = w.Price,
                remark = w.Remark,
                storeRomeId = w.StoreRomeId,
            })
            .ToListAsync();
        output.logEnterpriseOutrecordList = logEnterpriseOutrecordList; //.Adapt<List<LogEnterpriseOutrecordInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取商家商品出库表列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseOutorderListQueryInput input)
    {
        List<DateTime> queryOutTime = input.outTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startOutTime = queryOutTime?.First();
        DateTime? endOutTime = queryOutTime?.Last();
        var data = await _repository.Context.Queryable<LogEnterpriseOutorderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.outType), it => it.OutType.Equals(input.outType))
            .WhereIF(queryOutTime != null, it => SqlFunc.Between(it.OutTime, startOutTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endOutTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.OutType.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogEnterpriseOutorderListOutput
            {
                id = it.Id,
                no = it.No,
                outType = it.OutType,
                outTime = it.OutTime,
                remark = it.Remark,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseOutorderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建商家商品出库表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogEnterpriseOutorderCrInput input)
    {
        var entity = input.Adapt<LogEnterpriseOutorderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpOutOrder");

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<LogEnterpriseOutorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var logEnterpriseOutrecordEntityList = input.logEnterpriseOutrecordList.Adapt<List<LogEnterpriseOutrecordEntity>>();
            if (logEnterpriseOutrecordEntityList != null)
            {
                foreach (var item in logEnterpriseOutrecordEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.OutId = newEntity.Id;
                }

                await _repository.Context.Insertable<LogEnterpriseOutrecordEntity>(logEnterpriseOutrecordEntityList).ExecuteCommandAsync();
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
    /// 更新商家商品出库表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogEnterpriseOutorderUpInput input)
    {
        var entity = input.Adapt<LogEnterpriseOutorderEntity>();
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogEnterpriseOutorderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            await _repository.Context.CUDSaveAsnyc<LogEnterpriseOutrecordEntity>(it => it.OutId == entity.Id, input.logEnterpriseOutrecordList, it => it.OutId = entity.Id);

            //// 清空商家商品出库明细原有数据
            //await _repository.Context.Deleteable<LogEnterpriseOutrecordEntity>().Where(it => it.OutId == entity.Id).ExecuteCommandAsync();

            //// 新增商家商品出库明细新数据
            //var logEnterpriseOutrecordEntityList = input.logEnterpriseOutrecordList.Adapt<List<LogEnterpriseOutrecordEntity>>();
            //if (logEnterpriseOutrecordEntityList != null)
            //{
            //    foreach (var item in logEnterpriseOutrecordEntityList)
            //    {
            //        item.Id = SnowflakeIdHelper.NextId();
            //        item.OutId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<LogEnterpriseOutrecordEntity>(logEnterpriseOutrecordEntityList).ExecuteCommandAsync();
            //}

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
    /// 删除商家商品出库表.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!await _repository.Context.Queryable<LogEnterpriseOutorderEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<LogEnterpriseOutorderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            // 清空商家商品出库明细表数据
            await _repository.Context.Deleteable<LogEnterpriseOutrecordEntity>().Where(it => it.OutId.Equals(entity.Id)).ExecuteCommandAsync();

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
    /// 根据规格获取最后的入库信息
    /// </summary>
    /// <param name="gid"></param>
    /// <returns></returns>
    [HttpGet("Actions/QueryInrecord")]
    public async Task<dynamic> GetLastInStoreRomeByGid([FromQuery]string gid)
    {
        var gids = gid.Split(",");
        var list = await _repository.Context.Queryable<LogEnterpriseInrecordEntity>()
                .GroupBy(it => it.Gid)//MergeTable之前不要有OrderBy
                .Select(it => new
                {
                    gid = it.Gid,
                    id = SqlFunc.AggregateMax(it.Id)
                })
                .MergeTable()
                .LeftJoin<LogEnterpriseInrecordEntity>((a, b) => a.id == b.Id)
                .Select((a, b) => new { gid = b.Gid, storeRomeId = b.StoreRomeId })
                .ToListAsync();

        return list;
        //return await _repository.Context.Queryable<LogEnterpriseInrecordEntity>()
        //    .Where(it => gids.Contains(it.Gid))
        //    .OrderBy(it => it.Id, OrderByType.Desc)
        //    .Take(1)
        //    .PartitionBy(it=>it.Gid)
        //    .ToListAsync();
    }
}
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
using QT.Logistics.Entitys.Dto.LogEnterpriseInorder;
using QT.Logistics.Entitys.Dto.LogEnterpriseInrecord;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Contracts;

namespace QT.Logistics;

/// <summary>
/// 业务实现：商家入库订单表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "商家商品入库管理", Name = "LogEnterpriseInorder", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseInorderService : ILogEnterpriseInorderService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogEnterpriseInorderEntity> _repository;

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
    /// 初始化一个<see cref="LogEnterpriseInorderService"/>类型的新实例.
    /// </summary>
    public LogEnterpriseInorderService(
        ISqlSugarRepository<LogEnterpriseInorderEntity> logEnterpriseInorderRepository,
        IBillRullService billRullService,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logEnterpriseInorderRepository;
        _billRullService = billRullService;
        _db = context.AsTenant();
        _userManager = userManager;

        _repository.Context.QueryFilter.AddTableFilter<IDeleteTime>(it => it.DeleteTime == null);
    }

    /// <summary>
    /// 获取商家入库订单表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogEnterpriseInorderInfoOutput>();

        var logEnterpriseInrecordList = await _repository.Context
            .Queryable<LogEnterpriseInrecordEntity>()
            .LeftJoin<LogEnterpriseProductmodelEntity>((w, a) => w.Gid == a.Id)
            .Where(w => w.InId == output.id)
            .Select((w, a) => new LogEnterpriseInrecordInfoOutput
            {
                productName = SqlFunc.Subqueryable<LogEnterpriseProductEntity>().Where(it => it.Id == a.Pid).Select(it => it.Name),
                unit = a.Unit,
                amount = w.Amount,
                gid = w.Gid,
                id = w.Id,
                inNum = w.InNum,
                gidName = a.Name,
                price = w.Price,
                remark = w.Remark,
                storeRomeId = w.StoreRomeId,
            })
            .ToListAsync();
        output.logEnterpriseInrecordList = logEnterpriseInrecordList;

        return output;
    }

    /// <summary>
    /// 获取商家入库订单表列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseInorderListQueryInput input)
    {
        List<DateTime> queryInTime = input.inTime?.Split(',').ToObject<List<DateTime>>();
        DateTime? startInTime = queryInTime?.First();
        DateTime? endInTime = queryInTime?.Last();
        var data = await _repository.Context.Queryable<LogEnterpriseInorderEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.inType), it => it.InType.Equals(input.inType))
            .WhereIF(queryInTime != null, it => SqlFunc.Between(it.InTime, startInTime.ParseToDateTime("yyyy-MM-dd 00:00:00"), endInTime.ParseToDateTime("yyyy-MM-dd 23:59:59")))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.No.Contains(input.keyword)
                || it.InType.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogEnterpriseInorderListOutput
            {
                id = it.Id,
                no = it.No,
                inType = it.InType,
                inTime = it.InTime,
                remark = it.Remark,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseInorderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建商家入库订单表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogEnterpriseInorderCrInput input)
    {
        var entity = input.Adapt<LogEnterpriseInorderEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.No = await _billRullService.GetBillNumber("QTErpInOrder");

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<LogEnterpriseInorderEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var logEnterpriseInrecordEntityList = input.logEnterpriseInrecordList.Adapt<List<LogEnterpriseInrecordEntity>>();
            if(logEnterpriseInrecordEntityList != null)
            {
                foreach (var item in logEnterpriseInrecordEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.InId = newEntity.Id;
                }

                await _repository.Context.Insertable<LogEnterpriseInrecordEntity>(logEnterpriseInrecordEntityList).ExecuteCommandAsync();
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
    /// 更新商家入库订单表.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogEnterpriseInorderUpInput input)
    {
        var entity = input.Adapt<LogEnterpriseInorderEntity>();
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogEnterpriseInorderEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            await _repository.Context.CUDSaveAsnyc<LogEnterpriseInrecordEntity, LogEnterpriseInrecordCrInput>(it => it.InId == entity.Id, input.logEnterpriseInrecordList, it => it.InId = entity.Id);

            //// 清空商家商品入库明细原有数据
            //await _repository.Context.Deleteable<LogEnterpriseInrecordEntity>().Where(it => it.InId == entity.Id).ExecuteCommandAsync();

            //// 新增商家商品入库明细新数据
            //var logEnterpriseInrecordEntityList = input.logEnterpriseInrecordList.Adapt<List<LogEnterpriseInrecordEntity>>();
            //if(logEnterpriseInrecordEntityList != null)
            //{
            //    foreach (var item in logEnterpriseInrecordEntityList)
            //    {
            //        item.Id = SnowflakeIdHelper.NextId();
            //        item.InId = entity.Id;
            //    }

            //    await _repository.Context.Insertable<LogEnterpriseInrecordEntity>(logEnterpriseInrecordEntityList).ExecuteCommandAsync();
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
    /// 删除商家入库订单表.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if(!await _repository.Context.Queryable<LogEnterpriseInorderEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }       

        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<LogEnterpriseInorderEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

                // 清空商家商品入库明细表数据
            await _repository.Context.Deleteable<LogEnterpriseInrecordEntity>().Where(it => it.InId.Equals(entity.Id)).ExecuteCommandAsync();

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
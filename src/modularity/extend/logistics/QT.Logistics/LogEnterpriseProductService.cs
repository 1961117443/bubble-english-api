using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogEnterpriseProduct;
using QT.Logistics.Entitys.Dto.LogEnterpriseProductmodel;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：商家商品信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "商家商品信息管理", Name = "LogEnterpriseProduct", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseProductService : ILogEnterpriseProductService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogEnterpriseProductEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogEnterpriseProductService"/>类型的新实例.
    /// </summary>
    public LogEnterpriseProductService(
        ISqlSugarRepository<LogEnterpriseProductEntity> logEnterpriseProductRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logEnterpriseProductRepository;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    #region 增删改查
    /// <summary>
    /// 获取商家商品信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogEnterpriseProductInfoOutput>();

        var logEnterpriseProductmodelList = await _repository.Context.Queryable<LogEnterpriseProductmodelEntity>().Where(w => w.Pid == output.id).ToListAsync();
        output.logEnterpriseProductmodelList = logEnterpriseProductmodelList.Adapt<List<LogEnterpriseProductmodelInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取商家商品信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseProductListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogEnterpriseProductEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.tid), it => it.Tid.Equals(input.tid))
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), it => it.FirstChar.Contains(input.firstChar))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Tid.Contains(input.keyword)
                || it.Name.Contains(input.keyword)
                || it.FirstChar.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogEnterpriseProductListOutput
            {
                id = it.Id,
                tid = it.Tid,
                name = it.Name,
                firstChar = it.FirstChar,
                producer = it.Producer,
                remark = it.Remark,
                storage = it.Storage,
                retention = it.Retention,
                state = it.State ?? 0,
                tidName = SqlFunc.Subqueryable<LogEnterpriseProducttypeEntity>().Where(x => x.Id == it.Tid).Select(x => x.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseProductListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建商家商品信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogEnterpriseProductCrInput input)
    {
        var entity = input.Adapt<LogEnterpriseProductEntity>();
        //if (await _repository.Where(it => it.Name == entity.Name).AnyAsync())
        //{
        //    throw Oops.Oh(ErrorCode.COM1004);
        //}

        entity.Id = SnowflakeIdHelper.NextId();
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<LogEnterpriseProductEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var logEnterpriseProductmodelEntityList = input.logEnterpriseProductmodelList.Adapt<List<LogEnterpriseProductmodelEntity>>();
            if (logEnterpriseProductmodelEntityList != null)
            {
                foreach (var item in logEnterpriseProductmodelEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Pid = newEntity.Id;
                }

                await _repository.Context.Insertable<LogEnterpriseProductmodelEntity>(logEnterpriseProductmodelEntityList).ExecuteCommandAsync();
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
    /// 更新商家商品信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogEnterpriseProductUpInput input)
    {
        var entity = input.Adapt<LogEnterpriseProductEntity>();
        //if (await _repository.Where(it => it.Name == entity.Name && it.Id != entity.Id).AnyAsync())
        //{
        //    throw Oops.Oh(ErrorCode.COM1004);
        //}
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<LogEnterpriseProductEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            await _repository.Context.CUDSaveAsnyc<LogEnterpriseProductmodelEntity>(it => it.Pid == entity.Id, input.logEnterpriseProductmodelList, it => it.Pid = entity.Id);

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
    /// 删除商家商品信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!await _repository.Context.Queryable<LogEnterpriseProductEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<LogEnterpriseProductEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            // 清空商家商品规格表数据
            await _repository.Context.Deleteable<LogEnterpriseProductmodelEntity>().Where(it => it.Pid.Equals(entity.Id)).ExecuteCommandAsync();

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
    /// 选择商品信息，分页查询
    /// </summary>
    [HttpGet("Actions/PageSelector")]
    public async Task<dynamic> QueryProduct([FromQuery] LogEnterpriseProductListSelectorQueryInput pageInput)
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

        return PageResult<LogEnterpriseProductListSelectorOutput>.SqlSugarPageResult(data);
    }
}
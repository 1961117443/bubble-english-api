using Mapster;
using QT.DependencyInjection;
using QT.JXC.Interfaces;
using QT.Systems.Entitys.Permission;

namespace QT.JXC;

/// <summary>
/// 业务实现：分拣员.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpSorter", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpSorterService : IErpSorterService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpSorterEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ErpSorterService"/>类型的新实例.
    /// </summary>
    public ErpSorterService(
        ISqlSugarRepository<ErpSorterEntity> erpSorterRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = erpSorterRepository;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    /// <summary>
    /// 获取分拣员.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpSorterInfoOutput>();

        var erpDeliveryrouteSorterList = await _repository.Context.Queryable<ErpDeliveryrouteSorterEntity>().Where(w => w.Sid == output.id).ToListAsync();
        output.erpDeliveryrouteSorterList = erpDeliveryrouteSorterList.Adapt<List<ErpDeliveryrouteSorterInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取分拣员列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpSorterListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpSorterEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.mobile), it => it.Mobile.Contains(input.mobile))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Mobile.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpSorterListOutput
            {
                id = it.Id,
                oid = it.Oid,
                name = it.Name,
                mobile = it.Mobile,
                loginId = it.LoginId,
                loginPwd = it.LoginPwd,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpSorterListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建分拣员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ErpSorterCrInput input)
    {
        var entity = input.Adapt<ErpSorterEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<ErpSorterEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var erpDeliveryrouteSorterEntityList = input.erpDeliveryrouteSorterList.Adapt<List<ErpDeliveryrouteSorterEntity>>();
            if (erpDeliveryrouteSorterEntityList != null)
            {
                foreach (var item in erpDeliveryrouteSorterEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Sid = newEntity.Id;
                }

                await _repository.Context.Insertable(erpDeliveryrouteSorterEntityList).ExecuteCommandAsync();
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
    /// 更新分拣员.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpSorterUpInput input)
    {
        var entity = input.Adapt<ErpSorterEntity>();
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<ErpSorterEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空线路分拣员（中间表）原有数据
            await _repository.Context.Deleteable<ErpDeliveryrouteSorterEntity>().Where(it => it.Sid == entity.Id).ExecuteCommandAsync();

            // 新增线路分拣员（中间表）新数据
            var erpDeliveryrouteSorterEntityList = input.erpDeliveryrouteSorterList.Adapt<List<ErpDeliveryrouteSorterEntity>>();
            if (erpDeliveryrouteSorterEntityList != null)
            {
                foreach (var item in erpDeliveryrouteSorterEntityList)
                {
                    item.Id = item.Id ?? SnowflakeIdHelper.NextId();
                    item.Sid = entity.Id;
                }

                await _repository.Context.Insertable(erpDeliveryrouteSorterEntityList).ExecuteCommandAsync();
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
    /// 删除分拣员.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!await _repository.Context.Queryable<ErpSorterEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        try
        {
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<ErpSorterEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            // 清空线路分拣员（中间表）表数据
            await _repository.Context.Deleteable<ErpDeliveryrouteSorterEntity>().Where(it => it.Sid.Equals(entity.Id)).ExecuteCommandAsync();

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

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        List<ErpSorterEntity>? data = await _repository.AsQueryable()
            .OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();

        return new { list = data.Adapt<List<ErpSorterListOutput>>() };
    }
}
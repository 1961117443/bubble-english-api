using QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryCustomer;
using QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryroute;
using QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryrouteDeliveryman;
using QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryrouteSorter;
using QT.Application.Entitys.Dto.FreshDelivery.ErpStoreDelivery;
using QT.Application.Entitys.FreshDelivery;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：配送路线.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "配送路线", Name = "ErpDeliveryroute", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpDeliveryrouteService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpDeliveryrouteEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ErpDeliveryrouteService"/>类型的新实例.
    /// </summary>
    public ErpDeliveryrouteService(
        ISqlSugarRepository<ErpDeliveryrouteEntity> erpDeliveryrouteRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = erpDeliveryrouteRepository;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    /// <summary>
    /// 获取配送路线.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpDeliveryrouteInfoOutput>();

        var erpDeliveryCustomerList = await _repository.Context.Queryable<ErpDeliveryCustomerEntity>().Includes(w=>w.ErpCustomer).Where(w => w.Did == output.id).ToListAsync();
        output.erpDeliveryCustomerList = erpDeliveryCustomerList.Adapt<List<ErpDeliveryCustomerInfoOutput>>();

        var erpDeliveryrouteDeliverymanList = await _repository.Context.Queryable<ErpDeliveryrouteDeliverymanEntity>().Includes(w=>w.ErpDeliveryman).Where(w => w.Did == output.id).ToListAsync();
        output.erpDeliveryrouteDeliverymanList = erpDeliveryrouteDeliverymanList.Adapt<List<ErpDeliveryrouteDeliverymanInfoOutput>>();

        var erpDeliveryrouteSorterList = await _repository.Context.Queryable<ErpDeliveryrouteSorterEntity>().Includes(w=>w.ErpSorter).Where(w => w.Did == output.id).ToListAsync();
        output.erpDeliveryrouteSorterList = erpDeliveryrouteSorterList.Adapt<List<ErpDeliveryrouteSorterInfoOutput>>();

        var erpStoreDeliveryList = await _repository.Context.Queryable<ErpStoreDeliveryEntity>().Includes(w=>w.ErpStoreroom).Where(w => w.Did == output.id).ToListAsync();
        output.erpStoreDeliveryList = erpStoreDeliveryList.Adapt<List<ErpStoreDeliveryInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取配送路线列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpDeliveryrouteListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpDeliveryrouteEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpDeliveryrouteListOutput
            {
                id = it.Id,
                name = it.Name,
                sortName = it.SortName,
                remark = it.Remark,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d => d.Id == it.Oid).Select(d => d.FullName),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpDeliveryrouteListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建配送路线.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpDeliveryrouteCrInput input)
    {
        var entity = input.Adapt<ErpDeliveryrouteEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<ErpDeliveryrouteEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var erpDeliveryCustomerEntityList = input.erpDeliveryCustomerList.Adapt<List<ErpDeliveryCustomerEntity>>();
            if(erpDeliveryCustomerEntityList != null)
            {
                foreach (var item in erpDeliveryCustomerEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Did = newEntity.Id;
                }

                await _repository.Context.Insertable<ErpDeliveryCustomerEntity>(erpDeliveryCustomerEntityList).ExecuteCommandAsync();
            }

            var erpDeliveryrouteDeliverymanEntityList = input.erpDeliveryrouteDeliverymanList.Adapt<List<ErpDeliveryrouteDeliverymanEntity>>();
            if(erpDeliveryrouteDeliverymanEntityList != null)
            {
                foreach (var item in erpDeliveryrouteDeliverymanEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Did = newEntity.Id;
                }

                await _repository.Context.Insertable<ErpDeliveryrouteDeliverymanEntity>(erpDeliveryrouteDeliverymanEntityList).ExecuteCommandAsync();
            }

            var erpDeliveryrouteSorterEntityList = input.erpDeliveryrouteSorterList.Adapt<List<ErpDeliveryrouteSorterEntity>>();
            if(erpDeliveryrouteSorterEntityList != null)
            {
                foreach (var item in erpDeliveryrouteSorterEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Did = newEntity.Id;
                }

                await _repository.Context.Insertable<ErpDeliveryrouteSorterEntity>(erpDeliveryrouteSorterEntityList).ExecuteCommandAsync();
            }

            var erpStoreDeliveryEntityList = input.erpStoreDeliveryList.Adapt<List<ErpStoreDeliveryEntity>>();
            if(erpStoreDeliveryEntityList != null)
            {
                foreach (var item in erpStoreDeliveryEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Did = newEntity.Id;
                }

                await _repository.Context.Insertable<ErpStoreDeliveryEntity>(erpStoreDeliveryEntityList).ExecuteCommandAsync();
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
    /// 更新配送路线.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpDeliveryrouteUpInput input)
    {
        var entity = input.Adapt<ErpDeliveryrouteEntity>();
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            await _repository.Context.Updateable<ErpDeliveryrouteEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空路线客户（中间表）原有数据
            await _repository.Context.Deleteable<ErpDeliveryCustomerEntity>().Where(it => it.Did == entity.Id).ExecuteCommandAsync();

            // 新增路线客户（中间表）新数据
            var erpDeliveryCustomerEntityList = input.erpDeliveryCustomerList.Adapt<List<ErpDeliveryCustomerEntity>>();
            if(erpDeliveryCustomerEntityList != null)
            {
                foreach (var item in erpDeliveryCustomerEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Did = entity.Id;
                }

                await _repository.Context.Insertable<ErpDeliveryCustomerEntity>(erpDeliveryCustomerEntityList).ExecuteCommandAsync();
            }

            // 清空线路送货员（中间表）原有数据
            await _repository.Context.Deleteable<ErpDeliveryrouteDeliverymanEntity>().Where(it => it.Did == entity.Id).ExecuteCommandAsync();

            // 新增线路送货员（中间表）新数据
            var erpDeliveryrouteDeliverymanEntityList = input.erpDeliveryrouteDeliverymanList.Adapt<List<ErpDeliveryrouteDeliverymanEntity>>();
            if(erpDeliveryrouteDeliverymanEntityList != null)
            {
                foreach (var item in erpDeliveryrouteDeliverymanEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Did = entity.Id;
                }

                await _repository.Context.Insertable<ErpDeliveryrouteDeliverymanEntity>(erpDeliveryrouteDeliverymanEntityList).ExecuteCommandAsync();
            }

            // 清空线路分拣员（中间表）原有数据
            await _repository.Context.Deleteable<ErpDeliveryrouteSorterEntity>().Where(it => it.Did == entity.Id).ExecuteCommandAsync();

            // 新增线路分拣员（中间表）新数据
            var erpDeliveryrouteSorterEntityList = input.erpDeliveryrouteSorterList.Adapt<List<ErpDeliveryrouteSorterEntity>>();
            if(erpDeliveryrouteSorterEntityList != null)
            {
                foreach (var item in erpDeliveryrouteSorterEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Did = entity.Id;
                }

                await _repository.Context.Insertable<ErpDeliveryrouteSorterEntity>(erpDeliveryrouteSorterEntityList).ExecuteCommandAsync();
            }

            // 清空仓库覆盖路线(中间表）原有数据
            await _repository.Context.Deleteable<ErpStoreDeliveryEntity>().Where(it => it.Did == entity.Id).ExecuteCommandAsync();

            // 新增仓库覆盖路线(中间表）新数据
            var erpStoreDeliveryEntityList = input.erpStoreDeliveryList.Adapt<List<ErpStoreDeliveryEntity>>();
            if(erpStoreDeliveryEntityList != null)
            {
                foreach (var item in erpStoreDeliveryEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Did = entity.Id;
                }

                await _repository.Context.Insertable<ErpStoreDeliveryEntity>(erpStoreDeliveryEntityList).ExecuteCommandAsync();
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
    /// 删除配送路线.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        if(!await _repository.Context.Queryable<ErpDeliveryrouteEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }       

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<ErpDeliveryrouteEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

                // 清空路线客户（中间表）表数据
            await _repository.Context.Deleteable<ErpDeliveryCustomerEntity>().Where(it => it.Did.Equals(entity.Id)).ExecuteCommandAsync();

                // 清空线路送货员（中间表）表数据
            await _repository.Context.Deleteable<ErpDeliveryrouteDeliverymanEntity>().Where(it => it.Did.Equals(entity.Id)).ExecuteCommandAsync();

                // 清空线路分拣员（中间表）表数据
            await _repository.Context.Deleteable<ErpDeliveryrouteSorterEntity>().Where(it => it.Did.Equals(entity.Id)).ExecuteCommandAsync();

                // 清空仓库覆盖路线(中间表）表数据
            await _repository.Context.Deleteable<ErpStoreDeliveryEntity>().Where(it => it.Did.Equals(entity.Id)).ExecuteCommandAsync();

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
    /// 下拉数据源
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public Task<List<ErpDeliveryrouteListOutput>> Selector()
    {
        return _repository.Entities.Select<ErpDeliveryrouteListOutput>().ToListAsync();
    }
}
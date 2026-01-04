using QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryman;
using QT.Application.Entitys.Dto.FreshDelivery.ErpDeliveryrouteDeliveryman;
using QT.Application.Entitys.FreshDelivery;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Permission;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：送货员.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "配送员管理", Name = "ErpDeliveryman", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpDeliverymanService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpDeliverymanEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ErpDeliverymanService"/>类型的新实例.
    /// </summary>
    public ErpDeliverymanService(
        ISqlSugarRepository<ErpDeliverymanEntity> erpDeliverymanRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = erpDeliverymanRepository;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    /// <summary>
    /// 获取送货员.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpDeliverymanInfoOutput>();

        var erpDeliveryrouteDeliverymanList = await _repository.Context.Queryable<ErpDeliveryrouteDeliverymanEntity>().Where(w => w.Mid == output.id).ToListAsync();
        output.erpDeliveryrouteDeliverymanList = erpDeliveryrouteDeliverymanList.Adapt<List<ErpDeliveryrouteDeliverymanInfoOutput>>();

        return output;
    }

    /// <summary>
    /// 获取送货员列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpDeliverymanListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpDeliverymanEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.mobile), it => it.Mobile.Contains(input.mobile))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Mobile.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpDeliverymanListOutput
            {
                id = it.Id,
                oid = it.Oid,
                name = it.Name,
                mobile = it.Mobile,
                loginId = it.LoginId,
                loginPwd = it.LoginPwd,
                oidName = SqlFunc.Subqueryable<OrganizeEntity>().Where(d=>d.Id == it.Oid).Select(d=>d.FullName),
                deliveryCar = it.DeliveryCar,
                carCaptainId = it.CarCaptainId,
                carCaptainIdName = SqlFunc.Subqueryable<UserEntity>().Where(d => d.Id == it.CarCaptainId).Select(d => d.RealName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpDeliverymanListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建送货员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpDeliverymanCrInput input)
    {
        var entity = input.Adapt<ErpDeliverymanEntity>();
        entity.Id = SnowflakeIdHelper.NextId();

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<ErpDeliverymanEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var erpDeliveryrouteDeliverymanEntityList = input.erpDeliveryrouteDeliverymanList.Adapt<List<ErpDeliveryrouteDeliverymanEntity>>();
            if(erpDeliveryrouteDeliverymanEntityList != null)
            {
                foreach (var item in erpDeliveryrouteDeliverymanEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Mid = newEntity.Id;
                }

                await _repository.Context.Insertable<ErpDeliveryrouteDeliverymanEntity>(erpDeliveryrouteDeliverymanEntityList).ExecuteCommandAsync();
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
    /// 更新送货员.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpDeliverymanUpInput input)
    {
        var entity = input.Adapt<ErpDeliverymanEntity>();
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            await _repository.Context.Updateable<ErpDeliverymanEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空线路送货员（中间表）原有数据
            await _repository.Context.Deleteable<ErpDeliveryrouteDeliverymanEntity>().Where(it => it.Mid == entity.Id).ExecuteCommandAsync();

            // 新增线路送货员（中间表）新数据
            var erpDeliveryrouteDeliverymanEntityList = input.erpDeliveryrouteDeliverymanList.Adapt<List<ErpDeliveryrouteDeliverymanEntity>>();
            if(erpDeliveryrouteDeliverymanEntityList != null)
            {
                foreach (var item in erpDeliveryrouteDeliverymanEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Mid = entity.Id;
                }

                await _repository.Context.Insertable<ErpDeliveryrouteDeliverymanEntity>(erpDeliveryrouteDeliverymanEntityList).ExecuteCommandAsync();
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
    /// 删除送货员.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        if(!await _repository.Context.Queryable<ErpDeliverymanEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }       

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
            await _repository.Context.Deleteable<ErpDeliverymanEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

                // 清空线路送货员（中间表）表数据
            await _repository.Context.Deleteable<ErpDeliveryrouteDeliverymanEntity>().Where(it => it.Mid.Equals(entity.Id)).ExecuteCommandAsync();

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
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        List<ErpDeliverymanEntity>? data = await _repository.AsQueryable()
            .OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();

        return new { list = data.Adapt<List<ErpDeliverymanListOutput>>() };
    }
}
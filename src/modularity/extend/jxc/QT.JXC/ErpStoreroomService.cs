using Mapster;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp;
using QT.JXC.Entitys.Entity.ERP;
using QT.JXC.Interfaces;

namespace QT.JXC;

/// <summary>
/// 业务实现：仓库信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpStoreroom", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpStoreroomService : IErpStoreroomService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpStoreroomEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ErpStoreroomService"/>类型的新实例.
    /// </summary>
    public ErpStoreroomService(
        ISqlSugarRepository<ErpStoreroomEntity> erpStoreroomRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = erpStoreroomRepository;
        _db = context.AsTenant();
        _userManager = userManager;
    }

    /// <summary>
    /// 获取仓库信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpStoreroomInfoOutput>();

        var erpStoreareaList = await _repository.Context.Queryable<ErpStoreareaEntity>().Where(w => w.Sid == output.id).ToListAsync();
        output.erpStoreareaList = erpStoreareaList.Adapt<List<ErpStoreareaInfoOutput>>();

        var erpStoreDeliveryList = await _repository.Context.Queryable<ErpStoreDeliveryEntity>().Where(w => w.Sid == output.id).ToListAsync();
        output.erpStoreDeliveryList = erpStoreDeliveryList.Adapt<List<ErpStoreDeliveryInfoOutput>>();

        output.erpStoreCompanyList = await _repository.Context.Queryable<ErpStoreCompanyEntity>().Where(w => w.Sid == output.id).Select(w => w.Oid).ToListAsync();
        return output;
    }

    /// <summary>
    /// 获取仓库信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpStoreroomListQueryInput input)
    {
        /*
        var data = await _repository.Context.Queryable<ErpStoreroomEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), it => it.FirstChar.Contains(input.firstChar))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.FirstChar.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpStoreroomListOutput
            {
                id = it.Id,
                fid = it.Fid,
                name = it.Name,
                firstChar = it.FirstChar,
                address = it.Address,
                phone = it.Phone,
                admin = it.Admin,
                admintel = it.Admintel,
                fidName = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(d => d.Id == it.Fid).Select(d => d.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpStoreroomListOutput>.SqlSugarPageResult(data);
        */
       var data = await  _repository.Context.Queryable<ErpStoreroomEntity>()
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpStoreroomListOutput
            {
                id = it.Id,
                fid = it.Fid,
                name = it.Name,
                firstChar = it.FirstChar,
                address = it.Address,
                phone = it.Phone,
                admin = it.Admin,
                admintel = it.Admintel,
                fidName = SqlFunc.Subqueryable<ErpStoreroomEntity>().Where(d => d.Id == it.Fid).Select(d => d.Name)
            }).ToListAsync();

        if (!string.IsNullOrEmpty(input.name))
        {
            data = data.TreeWhere(x => x.name.Contains(input.name),x=>x.id,x=>x.fid);
        }

        if (!string.IsNullOrEmpty(input.firstChar))
        {
            data = data.TreeWhere(x => x.firstChar.Contains(input.firstChar, StringComparison.OrdinalIgnoreCase), x => x.id, x => x.fid);
        }
        if (!string.IsNullOrEmpty(input.keyword))
        {
            data = data.TreeWhere(x => x.firstChar.Contains(input.firstChar, StringComparison.OrdinalIgnoreCase) || x.name.Contains(input.name), x => x.id, x => x.fid);
        }
        
        var treeList = data.Adapt<List<ErpStoreroomListTreeOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 新建仓库信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ErpStoreroomCrInput input)
    {
        var entity = input.Adapt<ErpStoreroomEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        // 判断仓库名称是否存在
        if (await _repository.Context.Queryable<ErpStoreroomEntity>().AnyAsync(x => x.Name == input.name))
        {
            throw Oops.Oh("仓库名称已存在，不允许重复录入！");
        }


        try
        {
            // 开启事务
            _db.BeginTran();

            var newEntity = await _repository.Context.Insertable<ErpStoreroomEntity>(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();

            var erpStoreareaEntityList = input.erpStoreareaList.Adapt<List<ErpStoreareaEntity>>();
            if(erpStoreareaEntityList != null)
            {
                foreach (var item in erpStoreareaEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Sid = newEntity.Id;
                }

                await _repository.Context.Insertable(erpStoreareaEntityList).ExecuteCommandAsync();
            }

            var erpStoreDeliveryEntityList = input.erpStoreDeliveryList.Adapt<List<ErpStoreDeliveryEntity>>();
            if(erpStoreDeliveryEntityList != null)
            {
                foreach (var item in erpStoreDeliveryEntityList)
                {
                    item.Id = SnowflakeIdHelper.NextId();
                    item.Sid = newEntity.Id;
                }

                await _repository.Context.Insertable(erpStoreDeliveryEntityList).ExecuteCommandAsync();
            }

            var erpStoreCompanyList = input.erpStoreCompanyList?.Select(x => new ErpStoreCompanyEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                Sid = newEntity.Id,
                Oid = x
            }).ToList();
            if (input.erpStoreCompanyList != null && input.erpStoreCompanyList.Any())
            {
                await _repository.Context.Insertable<ErpStoreCompanyEntity>(erpStoreCompanyList).ExecuteCommandAsync();
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
    /// 更新仓库信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpStoreroomUpInput input)
    {
        var entity = input.Adapt<ErpStoreroomEntity>();

        if (await _repository.Context.Queryable<ErpStoreroomEntity>().AnyAsync(x => x.Name == entity.Name && x.Id!=entity.Id))
        {
            throw Oops.Oh("仓库名称已存在，不允许重复录入！");
        }

        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Updateable<ErpStoreroomEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            // 清空库区信息原有数据
            await _repository.Context.Deleteable<ErpStoreareaEntity>().Where(it => it.Sid == entity.Id).ExecuteCommandAsync();

            // 新增库区信息新数据
            var erpStoreareaEntityList = input.erpStoreareaList.Adapt<List<ErpStoreareaEntity>>();
            if(erpStoreareaEntityList != null)
            {
                foreach (var item in erpStoreareaEntityList)
                {
                    item.Id ??= SnowflakeIdHelper.NextId();
                    item.Sid = entity.Id;
                }

                await _repository.Context.Insertable(erpStoreareaEntityList).ExecuteCommandAsync();
            }

            // 清空仓库覆盖路线(中间表）原有数据
            await _repository.Context.Deleteable<ErpStoreDeliveryEntity>().Where(it => it.Sid == entity.Id).ExecuteCommandAsync();

            // 新增仓库覆盖路线(中间表）新数据
            var erpStoreDeliveryEntityList = input.erpStoreDeliveryList.Adapt<List<ErpStoreDeliveryEntity>>();
            if(erpStoreDeliveryEntityList != null)
            {
                foreach (var item in erpStoreDeliveryEntityList)
                {
                    item.Id ??= SnowflakeIdHelper.NextId();
                    item.Sid = entity.Id;
                }

                await _repository.Context.Insertable(erpStoreDeliveryEntityList).ExecuteCommandAsync();
            }

            // 清空仓库覆盖路线(中间表）原有数据

            var erpStoreCompanyListDb = await _repository.Context.Queryable<ErpStoreCompanyEntity>().Where(it => it.Sid == entity.Id).ToListAsync();
            await _repository.Context.Deleteable<ErpStoreCompanyEntity>().Where(it => it.Sid == entity.Id).ExecuteCommandAsync();

            // 新增仓库覆盖路线(中间表）新数据
            var erpStoreCompanyList = input.erpStoreCompanyList?.Select(x => new ErpStoreCompanyEntity
            {
                //Id = SnowflakeIdHelper.NextId(),
                //Sid = entity.Id,
                Oid = x
            }).ToList();
            if (erpStoreCompanyList != null)
            {
                foreach (var item in erpStoreCompanyList)
                {
                    //判断原纪录是否存在
                    var dbEntity = erpStoreCompanyListDb?.Find(a => a.Oid == item.Oid);
                    item.Id = dbEntity == null ? SnowflakeIdHelper.NextId() : dbEntity.Id;
                    item.Sid = entity.Id;
                }

                await _repository.Context.Insertable<ErpStoreCompanyEntity>(erpStoreCompanyList).ExecuteCommandAsync();
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
    /// 删除仓库信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if(!await _repository.Context.Queryable<ErpStoreroomEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        var list = await _repository.Context.Queryable<ErpStoreroomEntity>().ToChildListAsync(x => x.Fid, id);

        try
        {
            var idList = list.Select(x => x.Id).ToArray();
            // 开启事务
            _db.BeginTran();

            var entity = await _repository.AsQueryable().FirstAsync(it => idList.Contains(it.Id));
            await _repository.Context.Deleteable<ErpStoreroomEntity>().Where(it => idList.Contains(it.Id)).ExecuteCommandAsync();

                // 清空库区信息表数据
            await _repository.Context.Deleteable<ErpStoreareaEntity>().Where(it => idList.Contains(it.Sid)).ExecuteCommandAsync();

                // 清空仓库覆盖路线(中间表）表数据
            await _repository.Context.Deleteable<ErpStoreDeliveryEntity>().Where(it => idList.Contains(it.Sid)).ExecuteCommandAsync();

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
    /// 下拉数据源
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<List<ErpStoreroomListOutput>> Selector()
    {
        var list = await _repository.Entities.WithCache().Select<ErpStoreroomListOutput>().ToListAsync();

        //获取公司关联
        var relations = await _repository.Context.Queryable<ErpStoreCompanyEntity>().WithCache().ToListAsync();

        foreach (var item in list)
        {
            item.oid = relations.Where(x => x.Sid == item.id).Select(x => x.Oid).ToArray();
        }

        return list;
    }

    /// <summary>
    /// 下拉数据源
    /// </summary>
    /// <returns></returns>
    [HttpGet("area/Selector")]
    public Task<List<ErpStoreareaInfoOutput>> AreaSelector()
    {
        return _repository.Context.Queryable<ErpStoreareaEntity>().WithCache().Select<ErpStoreareaInfoOutput>().ToListAsync();
    }
}
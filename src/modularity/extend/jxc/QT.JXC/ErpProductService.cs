using Mapster;
using Microsoft.AspNetCore.Authorization;
using QT.Common.Core.Manager.Files;
using QT.Common.Extension;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto;
using QT.JXC.Entitys.Dto.Erp;
using QT.JXC.Entitys.Entity;
using QT.JXC.Entitys.Entity.ERP;
using QT.JXC.Entitys.Views;
using QT.JXC.Interfaces;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Dto.SysLog;
using QT.Systems.Entitys.Dto.System.SysLog;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using System.Data;

namespace QT.JXC;

/// <summary>
/// 业务实现：商品信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpProduct", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpProductService : IErpProductService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpProductEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 初始化一个<see cref="ErpProductService"/>类型的新实例.
    /// </summary>
    public ErpProductService(
        ISqlSugarRepository<ErpProductEntity> erpProductRepository,
        ISqlSugarClient context,
        IUserManager userManager, IFileManager fileManager, IDictionaryDataService dictionaryDataService)
    {
        _repository = erpProductRepository;
        _db = context.AsTenant();
        _userManager = userManager;
        _fileManager = fileManager;
        _dictionaryDataService = dictionaryDataService;
    }

    /// <summary>
    /// 获取商品信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var output = (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpProductInfoOutput>();

        var erpProductmodelList = await _repository.Context.Queryable<ErpProductmodelEntity>()
            .Where(w => w.Pid == output.id)
            .ToListAsync();
        output.erpProductmodelList = erpProductmodelList.Adapt<List<ErpProductmodelInfoOutput>>();

        var list = output.erpProductmodelList?.Select(it => it.rid)?.ToList();
        if (list.IsAny())
        {
            var relations = await _repository.Context.Queryable<ErpProductEntity>()
                .InnerJoin<ErpProductmodelEntity>((a, b) => a.Id == b.Pid)
                .Where((a, b) => list.Contains(b.Id))
                .Select((a, b) => new
                {
                    id = b.Id,
                    name = a.Name,
                    gname = b.Name
                })
                .ToListAsync();
            //var relations = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it=> list.Contains(it.Id)).ToListAsync();
            foreach (var item in output.erpProductmodelList)
            {
                if (!string.IsNullOrEmpty(item.rid))
                {
                    var r = relations.Find(x => x.id == item.rid);
                    if (r != null)
                    {
                        item.ridName = $"{r.name}({r.gname})";
                    }
                }
            }
        }

        var erpProductpicList = await _repository.Context.Queryable<ErpProductpicEntity>().Where(w => w.Pid == output.id).ToListAsync();
        output.erpProductpicList = erpProductpicList.Adapt<List<ErpProductpicInfoOutput>>();

        var erpProductvideoList = await _repository.Context.Queryable<ErpProductvideoEntity>().Where(w => w.Pid == output.id).ToListAsync();
        output.erpProductvideoList = erpProductvideoList.Adapt<List<ErpProductvideoInfoOutput>>();

        output.erpProductcompanyList = await _repository.Context.Queryable<ErpProductcompanyEntity>().Where(w => w.Pid == output.id).Select<ErpProductcompanyInfoOutput>().ToListAsync();

        var itemIdList = output.erpProductmodelList?.Select(x => x.id).ToList();
        if (itemIdList.IsAny())
        {
            var companyList = await _repository.Context.Queryable<ErpProductcompanyEntity>().Where(w => itemIdList.Contains(w.Pid)).ToListAsync();

            foreach (var item in output.erpProductmodelList)
            {
                item.erpProductcompanyList = companyList.FindAll(x => x.Pid == item.id)?.Adapt<List<ErpProductcompanyInfoOutput>>()?.Select(x => x.oid).ToList() ?? new List<string>();
            }
        }


        return output;
    }

    /// <summary>
    /// 获取商品信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpProductListQueryInput input)
    {
        List<string> tidList = new List<string>();
        if (!string.IsNullOrEmpty(input.tid))
        {
            var list = await _repository.Context.Queryable<ErpProducttypeEntity>().ToChildListAsync(x => x.Fid, input.tid);

            if (list != null && list.Any())
            {
                tidList = list.Select(x => x.Id).ToList();
            }
        }


        var data = await _repository.Context.Queryable<ErpProductEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.nickname), it => it.Nickname.Contains(input.nickname))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), it => it.FirstChar.Contains(input.firstChar))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Nickname.Contains(input.keyword)
                || it.FirstChar.Contains(input.keyword)
                || it.No.Contains(input.keyword)
                )
            .WhereIF(tidList.Any(), it => tidList.Contains(it.Tid))
            .WhereIF(!string.IsNullOrEmpty(input.relationCompanyId), it => SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(d => d.Pid == it.Id && d.Oid == input.relationCompanyId).Any())
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpProductListOutput
            {
                id = it.Id,
                tid = it.Tid,
                name = it.Name,
                nickname = it.Nickname,
                firstChar = it.FirstChar,
                no = it.No,
                unit = it.Unit,
                producer = it.Producer,
                sort = it.Sort,
                storage = it.Storage,
                retention = it.Retention,
                supplier = it.Supplier,
                state = SqlFunc.IIF(it.State == "0", "0", "1"),
                saletype = it.Saletype,
                num = it.Num,
                tidName = SqlFunc.Subqueryable<ErpProducttypeEntity>().Where(d => d.Id == it.Tid).Select(d => d.Name),
                supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(d => d.Id == it.Supplier).Select(d => d.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);

        if (data.list.IsAny())
        {
            var idList = data.list.Select(x => x.id).ToArray();
            var erpProductpicList = await _repository.Context.Queryable<ErpProductpicEntity>().Where(it => idList.Contains(it.Pid)).ToListAsync();
            foreach (var item in data.list)
            {
                item.imageList = erpProductpicList.Where(x => x.Pid == item.id).Select(x => x.Pic).ToArray();
            }
        }

        return PageResult<ErpProductListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建商品信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    [OperateLog("商品信息", "新增")]
    [SqlSugarUnitOfWork]
    public async Task Create([FromBody] ErpProductCrInput input)
    {
        var entity = input.Adapt<ErpProductEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        string mainLogKey = $"{nameof(ErpProductEntity)}:{entity.Id}";
        var newEntity = await _repository.Context.Insertable<ErpProductEntity>(entity).IgnoreColumns(ignoreNullColumn: true).EnableDiffLogEvent(mainLogKey).ExecuteReturnEntityAsync();

        var erpProductmodelEntityList = input.erpProductmodelList.Adapt<List<ErpProductmodelEntity>>();
        if (erpProductmodelEntityList != null)
        {
            foreach (var item in erpProductmodelEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.Pid = newEntity.Id;

                if (!item.Ratio.HasValue)
                {
                    if (item.Unit == newEntity.Unit)
                    {
                        item.Ratio = 1;
                    }
                }

                await _repository.Context.Insertable<ErpProductmodelEntity>(item).EnableDiffLogEvent($"{mainLogKey}:{nameof(ErpProductmodelEntity)}:{item.Id}").ExecuteCommandAsync();
            }

            //await _repository.Context.Insertable<ErpProductmodelEntity>(erpProductmodelEntityList).EnableDiffLogEvent($"{mainLogKey}:{nameof(ErpProductmodelEntity)}").ExecuteCommandAsync();
        }

        var erpProductpicEntityList = input.erpProductpicList.Adapt<List<ErpProductpicEntity>>();
        if (erpProductpicEntityList != null)
        {
            foreach (var item in erpProductpicEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.Pid = newEntity.Id;

                await _repository.Context.Insertable<ErpProductpicEntity>(item).EnableDiffLogEvent($"{mainLogKey}:{nameof(ErpProductmodelEntity)}:{item.Id}").ExecuteCommandAsync();
            }

            //await _repository.Context.Insertable<ErpProductpicEntity>(erpProductpicEntityList).EnableDiffLogEvent().ExecuteCommandAsync();
        }

        var erpProductvideoEntityList = input.erpProductvideoList.Adapt<List<ErpProductvideoEntity>>();
        if (erpProductvideoEntityList != null)
        {
            foreach (var item in erpProductvideoEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.Pid = newEntity.Id;

                await _repository.Context.Insertable<ErpProductvideoEntity>(item).EnableDiffLogEvent($"{mainLogKey}:{nameof(ErpProductvideoEntity)}:{item.Id}").ExecuteCommandAsync();
            }

            //await _repository.Context.Insertable<ErpProductvideoEntity>(erpProductvideoEntityList).EnableDiffLogEvent().ExecuteCommandAsync();
        }

        var erpProductcompanyEntityList = input.erpProductcompanyList.Adapt<List<ErpProductcompanyEntity>>();
        if (erpProductcompanyEntityList != null)
        {
            foreach (var item in erpProductcompanyEntityList)
            {
                item.Id = SnowflakeIdHelper.NextId();
                item.Pid = newEntity.Id;
            }

            await _repository.Context.Insertable(erpProductcompanyEntityList).EnableDiffLogEvent($"{mainLogKey}:{nameof(ErpProductcompanyEntity)}").ExecuteCommandAsync();
        }

        // 子表的关联公司记录
        var modelProductcompanyEntityList = new List<ErpProductcompanyEntity>();
        foreach (var item in input.erpProductmodelList)
        {
            if (item.erpProductcompanyList.IsAny())
            {
                foreach (var x in item.erpProductcompanyList)
                {
                    modelProductcompanyEntityList.Add(new ErpProductcompanyEntity
                    {
                        Oid = x,
                        Pid = newEntity.Id,
                        Id = SnowflakeIdHelper.NextId()
                    });
                }
            }
        }

        if (modelProductcompanyEntityList.IsAny())
        {
            await _repository.Context.Insertable(modelProductcompanyEntityList).EnableDiffLogEvent($"{mainLogKey}:{nameof(ErpProductcompanyEntity)}").ExecuteCommandAsync();
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
    /// 更新商品信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [OperateLog("商品信息", "修改")]
    [SqlSugarUnitOfWork]
    public async Task Update(string id, [FromBody] ErpProductUpInput input)
    {
        // 获取原有的数据
        var oldEntity = await _repository.Context.Queryable<ErpProductEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        _repository.Context.Tracking(oldEntity);
        var entity = input.Adapt(oldEntity);
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);

        var oldDetailEntitys = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it => it.Pid == id).ToListAsync();
        // 判断是否有发生过业务        
        if (oldDetailEntitys.IsAny())
        {
            var oldDetailIds = oldDetailEntitys.Select(it => it.Id).ToArray();
            //入库业务
            var flagList = await _repository.Context.Queryable<ErpInrecordEntity>().Where(it => oldDetailIds.Contains(it.Gid)).Select(it => it.Gid).ToListAsync();
            if (!flagList.IsAny())
            {
                // 出库业务
                flagList = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(it => oldDetailIds.Contains(it.Gid)).Select(it => it.Gid).ToListAsync();
            }
            // 
            if (!flagList.IsAny())
            {
                // 订单业务
                flagList = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(it => oldDetailIds.Contains(it.Mid)).Select(it => it.Mid).ToListAsync();
            }
            if (flagList.IsAny())
            {
                // 发生过业务，不允许修改 商品的单位、规格的单位和转化比
                foreach (var item in oldDetailEntitys)
                {
                    if (!flagList.Any(x => x == item.Id))
                    {
                        continue;
                    }
                    var model = input.erpProductmodelList.Find(x => x.id == item.Id);
                    if (model == null)
                    {
                        throw Oops.Oh("商品发生过业务，不允许删除该规格！");
                    }
                    var modelE = model.Adapt<ErpProductmodelEntity>();
                    var ratio = item.Ratio ?? 0;
                    var mratio = modelE.Ratio ?? 0;
                    //if (/*modelE.Unit != item.Unit || */mratio != ratio)
                    //{
                    //    throw Oops.Oh("商品发生过业务，不允许修改该规格的单位和转化比！");
                    //}
                }
                if (oldEntity.Unit != entity.Unit)
                {
                    throw Oops.Oh("商品发生过业务，不允许修改单位！");
                }
            }
        }

        List<SysLogFieldEntity> sysLogFieldEntities = new List<SysLogFieldEntity>();
        var changes = _repository.Context.GetChangeFields(oldEntity);

        var dt = DateTime.Now;

        if (changes.IsAny())
        {
            foreach (var item in changes)
            {
                var l = item.Adapt<SysLogFieldEntity>();
                l.ObjectId = entity.Id;
                l.CreatorTime = dt;
                sysLogFieldEntities.Add(l);
            }
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();



        await _repository.Context.Updateable<ErpProductEntity>(entity).IgnoreColumns(ignoreAllNullColumns: true).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}").ExecuteCommandAsync();

        // 清空商品规格原有数据
        //await _repository.Context.Deleteable<ErpProductmodelEntity>().Where(it => it.Pid == entity.Id).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductmodelEntity)}").ExecuteCommandAsync();

        // 新增商品规格新数据
        //var erpProductmodelEntityList = input.erpProductmodelList.Adapt<List<ErpProductmodelEntity>>();
        //if (erpProductmodelEntityList != null)
        //{
        //    foreach (var item in erpProductmodelEntityList)
        //    {
        //        item.Id = item.Id ?? SnowflakeIdHelper.NextId();
        //        item.Pid = entity.Id;
        //    }

        //    await _repository.Context.Insertable<ErpProductmodelEntity>(erpProductmodelEntityList).ExecuteCommandAsync();
        //}

        var modelProductcompanyEntityList = new List<ErpProductcompanyEntity>();

        if (input.erpProductmodelList.IsAny())
        {
            var erpProductmodelEntityList = new List<ErpProductmodelEntity>();
            foreach (var item in input.erpProductmodelList)
            {
                var newItem = item.Adapt<ErpProductmodelEntity>();
                newItem.Id ??= SnowflakeIdHelper.NextId();
                newItem.Pid = entity.Id;

                if (!newItem.Ratio.HasValue)
                {
                    if (newItem.Unit == entity.Unit)
                    {
                        newItem.Ratio = 1;
                    }
                }

                erpProductmodelEntityList.Add(newItem);
                if (item.erpProductcompanyList.IsAny())
                {
                    foreach (var x in item.erpProductcompanyList)
                    {
                        modelProductcompanyEntityList.Add(new ErpProductcompanyEntity
                        {
                            Oid = x,
                            Id = SnowflakeIdHelper.NextId(),
                            Pid = newItem.Id
                        });
                    }
                }
            }
            //await _repository.Context.Insertable<ErpProductmodelEntity>(erpProductmodelEntityList).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductmodelEntity)}").ExecuteCommandAsync();

            await _repository.Context.CUDSaveHardAsnyc<ErpProductmodelEntity, ErpProductmodelCrInput>(x => x.Pid == entity.Id, input.erpProductmodelList, onAdd: x => x.Pid = entity.Id, onUpdate: (m) =>
            {
                if (!m.Ratio.HasValue)
                {
                    if (m.Unit == entity.Unit)
                    {
                        m.Ratio = 1;
                    }
                }

                var cs = _repository.Context.GetChangeFields(m);
                if (cs.IsAny())
                {
                    foreach (var item in cs)
                    {
                        var l = item.Adapt<SysLogFieldEntity>();
                        l.ObjectId = m.Id;
                        l.CreatorTime = dt;
                        sysLogFieldEntities.Add(l);
                    }
                }

            });
        }

        if (modelProductcompanyEntityList.IsAny())
        {
            var array = modelProductcompanyEntityList.Select(x => x.Pid).Distinct().ToList();
            await _repository.Context.Deleteable<ErpProductcompanyEntity>().Where(it => array.Contains(it.Pid)).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductcompanyEntity)}").ExecuteCommandAsync();

            await _repository.Context.Insertable(modelProductcompanyEntityList).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductcompanyEntity)}").ExecuteCommandAsync();
        }


        // 清空商品图片原有数据
        await _repository.Context.Deleteable<ErpProductpicEntity>().Where(it => it.Pid == entity.Id).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductpicEntity)}").ExecuteCommandAsync();

        // 新增商品图片新数据
        var erpProductpicEntityList = input.erpProductpicList.Adapt<List<ErpProductpicEntity>>();
        if (erpProductpicEntityList != null)
        {
            foreach (var item in erpProductpicEntityList)
            {
                item.Id = item.Id ?? SnowflakeIdHelper.NextId();
                item.Pid = entity.Id;
            }

            await _repository.Context.Insertable(erpProductpicEntityList).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductpicEntity)}").ExecuteCommandAsync();
        }

        // 清空商品视频原有数据
        await _repository.Context.Deleteable<ErpProductvideoEntity>().Where(it => it.Pid == entity.Id).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductvideoEntity)}").ExecuteCommandAsync();

        // 新增商品视频新数据
        var erpProductvideoEntityList = input.erpProductvideoList.Adapt<List<ErpProductvideoEntity>>();
        if (erpProductvideoEntityList != null)
        {
            foreach (var item in erpProductvideoEntityList)
            {
                item.Id = item.Id ?? SnowflakeIdHelper.NextId();
                item.Pid = entity.Id;
            }

            await _repository.Context.Insertable(erpProductvideoEntityList).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductvideoEntity)}").ExecuteCommandAsync();
        }


        // 清空商品公司关联原有数据
        await _repository.Context.Deleteable<ErpProductcompanyEntity>().Where(it => it.Pid == entity.Id).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductcompanyEntity)}").ExecuteCommandAsync();

        // 新增商品公司关联新数据
        var erpProductcompanyEntityList = input.erpProductcompanyList.Adapt<List<ErpProductcompanyEntity>>();
        if (erpProductcompanyEntityList != null)
        {
            foreach (var item in erpProductcompanyEntityList)
            {
                item.Id = item.Id ?? SnowflakeIdHelper.NextId();
                item.Pid = entity.Id;
            }

            await _repository.Context.Insertable(erpProductcompanyEntityList).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}:{nameof(ErpProductcompanyEntity)}").ExecuteCommandAsync();
        }

        if (sysLogFieldEntities.IsAny())
        {
            sysLogFieldEntities.ForEach(x => x.UserId = _userManager.UserId);
            await _repository.Context.Insertable(sysLogFieldEntities).ExecuteCommandAsync();
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
    /// 删除商品信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [OperateLog("商品信息", "删除")]
    [SqlSugarUnitOfWork]
    public async Task Delete(string id)
    {
        if (!await _repository.Context.Queryable<ErpProductEntity>().AnyAsync(it => it.Id == id))
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }
        // 判断商品下的规格是否发生过业务
        var erpProductmodelList = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(x => x.Pid == id).Select(x => x.Id).ToListAsync();
        if (erpProductmodelList.IsAny())
        {
            if (await _repository.Context.Queryable<ErpOrderdetailEntity>().AnyAsync(x => erpProductmodelList.Contains(x.Mid)))
            {
                throw Oops.Oh("商品发生过业务，不允许删除！");
            }
            if (await _repository.Context.Queryable<ErpInrecordEntity>().AnyAsync(x => erpProductmodelList.Contains(x.Gid)))
            {
                throw Oops.Oh("商品发生过业务，不允许删除！");
            }
            if (await _repository.Context.Queryable<ErpOutrecordEntity>().AnyAsync(x => erpProductmodelList.Contains(x.Gid)))
            {
                throw Oops.Oh("商品发生过业务，不允许删除！");
            }
        }

        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        var entity = await _repository.AsQueryable().FirstAsync(it => it.Id.Equals(id));
        var items = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(it => it.Pid.Equals(entity.Id)).Select(x => x.Id).ToListAsync() ?? new List<string>();
        await _repository.Context.Deleteable<ErpProductEntity>().Where(it => it.Id.Equals(id)).EnableDiffLogEvent($"{nameof(ErpProductEntity)}:{entity.Id}").ExecuteCommandAsync();

        // 清空商品规格表数据
        await _repository.Context.Deleteable<ErpProductmodelEntity>().Where(it => it.Pid.Equals(entity.Id)).EnableDiffLogEvent().ExecuteCommandAsync();

        // 清空商品图片表数据
        await _repository.Context.Deleteable<ErpProductpicEntity>().Where(it => it.Pid.Equals(entity.Id)).EnableDiffLogEvent().ExecuteCommandAsync();

        // 清空商品视频表数据
        await _repository.Context.Deleteable<ErpProductvideoEntity>().Where(it => it.Pid.Equals(entity.Id)).EnableDiffLogEvent().ExecuteCommandAsync();

        items.Add(entity.Id);
        await _repository.Context.Deleteable<ErpProductcompanyEntity>().Where(it => items.Contains(it.Pid)).EnableDiffLogEvent().ExecuteCommandAsync();
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

    [HttpDelete("batch")]
    [SqlSugarUnitOfWork]
    public async Task<int> Delete([FromBody] List<string> input)
    {
        if (!input.IsAny())
        {
            return 0;
        }
        if (input.Count > 100)
        {
            throw Oops.Oh("最多删除100条记录！");
        }
        //if (!await _repository.Context.Queryable<ErpProductEntity>().AnyAsync(it => it.Id == id))
        //{
        //    throw Oops.Oh(ErrorCode.COM1005);
        //}
        // 判断商品下的规格是否发生过业务
        var erpProductmodelList = await _repository.Context.Queryable<ErpProductmodelEntity>().Where(x => input.Contains(x.Pid)).Select(x => new ErpProductmodelEntity
        {
            Id = x.Id,
            Pid = x.Pid
        }).ToListAsync();

        if (erpProductmodelList.IsAny())
        {
            var list1 = await _repository.Context.Queryable<ErpOrderdetailEntity>().Where(x => erpProductmodelList.Any(p => p.Id == x.Mid)).Select(x => x.Mid).ToListAsync();
            var list2 = await _repository.Context.Queryable<ErpInrecordEntity>().Where(x => erpProductmodelList.Any(p => p.Id == x.Gid)).Select(x => x.Gid).ToListAsync();
            var list3 = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(x => erpProductmodelList.Any(p => p.Id == x.Gid)).Select(x => x.Gid).ToListAsync();

            for (int i = input.Count - 1; i >= 0; i--)
            {
                var tempList = erpProductmodelList.Where(x => x.Pid == input[i]).ToList();
                if (tempList.IsAny())
                {
                    if (list1.Any(x => tempList.Any(p => p.Id == x)) || list2.Any(x => tempList.Any(p => p.Id == x)) || list3.Any(x => tempList.Any(p => p.Id == x)))
                    {
                        erpProductmodelList = erpProductmodelList.Except(tempList).ToList();
                        input.RemoveAt(i);
                    }
                }
            }
            //if (await _repository.Context.Queryable<ErpOrderdetailEntity>().AnyAsync(x => erpProductmodelList.Any(p=>p.Id == x.Mid)))
            //{
            //    throw Oops.Oh("商品发生过业务，不允许删除！");
            //}
            //if (await _repository.Context.Queryable<ErpInrecordEntity>().AnyAsync(x => erpProductmodelList.Any(p => p.Id == x.Gid)))
            //{
            //    throw Oops.Oh("商品发生过业务，不允许删除！");
            //}
            //if (await _repository.Context.Queryable<ErpOutrecordEntity>().AnyAsync(x => erpProductmodelList.Any(p => p.Id == x.Gid)))
            //{
            //    throw Oops.Oh("商品发生过业务，不允许删除！");
            //}
        }

        if (input.IsAny())
        {
            // 商品信息
            await _repository.Context.Deleteable<ErpProductEntity>().Where(it => input.Contains(it.Id)).EnableDiffLogEvent().ExecuteCommandAsync();

            // 清空商品规格表数据
            await _repository.Context.Deleteable<ErpProductmodelEntity>().Where(it => input.Contains(it.Pid)).EnableDiffLogEvent().ExecuteCommandAsync();

            // 清空商品图片表数据
            await _repository.Context.Deleteable<ErpProductpicEntity>().Where(it => input.Contains(it.Pid)).EnableDiffLogEvent().ExecuteCommandAsync();

            // 清空商品视频表数据
            await _repository.Context.Deleteable<ErpProductvideoEntity>().Where(it => input.Contains(it.Pid)).EnableDiffLogEvent().ExecuteCommandAsync();

            //items.Add(entity.Id);
            var relations = erpProductmodelList.SelectMany(x => new List<string> { x.Id, x.Pid }).ToList();

            relations = relations.Concat(input).Distinct().ToList();
            await _repository.Context.Deleteable<ErpProductcompanyEntity>().Where(it => relations.Contains(it.Pid)).EnableDiffLogEvent().ExecuteCommandAsync();

            return input.Count;
        }
        return 0;
    }
    /// <summary>
    /// 选择商品信息，分页查询
    /// </summary>
    [HttpGet("PageSelector")]
    public async Task<dynamic> QueryProduct([FromQuery] ErpProductSelectorQueryInput pageInput)
    {
        string customerType = string.Empty;
        if (!string.IsNullOrEmpty(pageInput.cid))
        {
            customerType = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == pageInput.cid).Select(x => x.Type).FirstAsync();
        }

        List<string> tidList = new List<string>();
        if (!string.IsNullOrEmpty(pageInput.tid))
        {
            var list = await _repository.Context.Queryable<ErpProducttypeEntity>().ToChildListAsync(x => x.Fid, pageInput.tid);

            if (list != null && list.Any())
            {
                tidList = list.Select(x => x.Id).ToList();
            }
        }

        // 计算单价
        var q1 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .Where(xxx => xxx.Oid == _userManager.CompanyId)
            .WhereIF(pageInput.gid.IsNotEmptyOrNull(), xxx => xxx.Gid == pageInput.gid);
        var q2 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .WhereIF(pageInput.gid.IsNotEmptyOrNull(), xxx => xxx.Gid == pageInput.gid)
            .Where(yyy => SqlFunc.IsNullOrEmpty(yyy.Oid) && !SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId && xxx.Tid == yyy.Tid && xxx.Gid == yyy.Gid).Any());
        var qur = _repository.Context.Union(q1, q2);

        var data = await _repository.Context.Queryable<ErpProductmodelEntity>()
            .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
            .LeftJoin<ErpProductpriceEntity>((a, b, c) => a.Id == c.Gid && c.Cid == pageInput.cid)
            .LeftJoin(qur, (a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
            .LeftJoin<ViewErpProducttypeEx>((a, b, c, d, e) => b.Tid == e.Id)
            //.LeftJoin<ErpProductcustomertypepriceEntity>((a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
            .Where((a, b) => b.State == "1")

            // 这种方式没有走索引
            //.Where((a, b) => SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => (pc.Pid == b.Id || pc.Pid == a.Id) && pc.Oid == _userManager.CompanyId).Any()
            //                                                  || !SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => (pc.Pid == b.Id || pc.Pid == a.Id)).Any())
            .Where((a, b) =>
            SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => pc.Pid == b.Id && pc.Oid == _userManager.CompanyId).Any() ||
            SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => pc.Pid == a.Id && pc.Oid == _userManager.CompanyId).Any() ||
            !(SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => pc.Pid == b.Id).Any() ||
            SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => pc.Pid == a.Id).Any()))
            .WhereIF(!string.IsNullOrEmpty(pageInput.gid), (a, b) => a.Id == pageInput.gid)
            .WhereIF(tidList.Any(), (a, b) => tidList.Contains(b.Tid))
            .WhereIF(pageInput.filterPrice == true && !string.IsNullOrEmpty(pageInput.cid), (a, b) =>
                SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(ddd => ddd.Gid == a.Id && ddd.Tid == customerType && ddd.Oid == _userManager.CompanyId).Any()
            || SqlFunc.Subqueryable<ErpProductpriceEntity>().Where(ddd => ddd.Gid == a.Id && ddd.Cid == pageInput.cid && ddd.Oid == _userManager.CompanyId).Any()
            )
            .WhereIF(pageInput.filterPrice == true, (a, b) =>
                SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(ddd => ddd.Gid == a.Id && ddd.Oid == _userManager.CompanyId).Any()
            || SqlFunc.Subqueryable<ErpProductpriceEntity>().Where(ddd => ddd.Gid == a.Id && ddd.Oid == _userManager.CompanyId).Any()
            )
            .WhereIF(!string.IsNullOrEmpty(pageInput.keyword), (a, b) => a.Name.Contains(pageInput.keyword)
            || b.Name.Contains(pageInput.keyword) || b.No.Contains(pageInput.keyword) || b.Nickname.Contains(pageInput.keyword) || b.FirstChar.Contains(pageInput.keyword)
            || SqlFunc.Subqueryable<ErpProducttypeEntity>().Where(ddd => ddd.Id == b.Tid && (ddd.Code.Contains(pageInput.keyword) || ddd.Name.Contains(pageInput.keyword) || ddd.FirstChar.Contains(pageInput.keyword))).Any())
            .WhereIF(pageInput.hasNum == true, (a, b) => SqlFunc.Subqueryable<ErpInrecordEntity>().Where(ddd => ddd.Gid == a.Id && ddd.Num > 0).Any())
            .OrderBy(a => a.UsageCount > 0 ? 0 : 1)
            .OrderByDescending(a => a.UsageCount)
            .OrderBy((a) => a.SalePrice > 0 ? 0 : 1)
            .OrderByDescending((a, b, c, d, e) => new { e.ROrder, e.Order }) //根据商品分类排序
            .OrderBy((a) => a.Id)
            .Select((a, b, c, d, e) => new ErpProductSelectorOutput
            {
                id = a.Id,
                name = a.Name,
                costPrice = a.CostPrice,
                grossMargin = a.GrossMargin,
                minNum = a.MinNum,
                num = 0,// a.Num,
                package = a.Package,
                productId = b.Id,
                productCode = b.No,
                productName = b.Name,
                //salePrice = string.IsNullOrEmpty(c.Id) ? a.SalePrice : c.Price,
                //salePrice = c.Price > 0 ? c.Price : (d.Discount > 0 ? (a.SalePrice * d.Discount * 0.01m) : (d.Price > 0 ? d.Price : a.SalePrice)), //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
                salePrice = c.Price > 0 ? c.Price : d.PricingType == 1 ? a.SalePrice * d.Discount * 0.01m : d.PricingType == 2 ? d.Price : a.SalePrice, //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
                productUnit = a.CustomerUnit ?? a.Unit,
                maxNum = a.MaxNum > 0 ? a.MaxNum : 9999999,
                supplier = b.Supplier,
                supplierName = SqlFunc.Subqueryable<ErpSupplierEntity>().Where(x => x.Id == b.Supplier).Select(x => x.Name),
                ratio = a.Ratio,
                rootProducttype = e.RootName,
                //rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName),
                unit = a.Unit,
                customerUnit = a.CustomerUnit
            }).ToPagedListAsync(pageInput.currentPage, pageInput.pageSize);

        //var idList = data.list?.Select(x => x.productId).Distinct().ToArray();
        //if (idList.IsAny())
        //{
        //    var erpProductpicList = await _repository.Context.Queryable<ErpProductpicEntity>().Where(it => idList.Contains(it.Pid)).ToListAsync();
        //    foreach (var item in data.list)
        //    {
        //        if (item.productId)
        //        {

        //        }
        //    }
        //}

        // 加载图片
        if (data.list.IsAny())
        {
            var idList = data.list.Select(x => x.productId).Distinct().ToArray();
            if (idList.IsAny())
            {
                var erpProductpicList = await _repository.Context.Queryable<ErpProductpicEntity>().Where(it => idList.Contains(it.Pid)).ToListAsync();
                foreach (var item in data.list)
                {
                    item.imageList = erpProductpicList.Where(it => it.Pid == item.productId).Select(it => it.Pic).ToArray();
                }
            }
        }


        // 有公司这个条件，带出库存数
        if (!string.IsNullOrEmpty(pageInput.oid) && data.list.Any())
        {
            var idList = data.list.Select(x => x.id).ToArray();
            var storeList = await _repository.Context.Queryable<ErpInrecordEntity>()
                .InnerJoin<ErpInorderEntity>((x, y) => x.InId == y.Id)
                .ClearFilter<ICompanyEntity>()
                .Where((x, y) => y.Oid == pageInput.oid && idList.Contains(x.Gid) && x.Num > 0)
                .GroupBy((x, y) => x.Gid)
                .Select((x, y) => new
                {
                    gid = x.Gid,
                    num = SqlFunc.AggregateSum(x.Num)
                }).ToListAsync();

            foreach (var item in storeList)
            {
                var entity = data.list.FirstOrDefault(a => a.id == item.gid);
                if (entity != null)
                {
                    entity.num = item.num;
                }
            }
        }

        return PageResult<ErpProductSelectorOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 根据规格查询价格，分页查询
    /// 客户定价
    /// </summary>
    [HttpGet("customer-price/{id}")]
    public async Task<dynamic> QueryPriceByCustomer([FromRoute] string id, [FromQuery] ErpProductSelectorQueryInput pageInput)
    {
        var list = await _repository.Context.Queryable<ErpProductpriceEntity>()
                     .Where(x => x.Gid == id)
                     .Select(x => new ErpProductCustomerPriceOutput
                     {
                         cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == x.Cid).Select(d => d.Name),
                         price = x.Price
                     }, true)
                     .ToPagedListAsync(pageInput.currentPage, pageInput.pageSize);

        return PageResult<ErpProductCustomerPriceOutput>.SqlSugarPageResult(list);
    }

    #region 导入导出
    /// <summary>
    /// 模板下载.
    /// </summary>
    /// <returns></returns>
    [HttpGet("TemplateDownload")]
    public dynamic TemplateDownload()
    {
        // 初始化 一条空数据 
        List<ErpProductListImportDataInput>? dataList = new List<ErpProductListImportDataInput>() { new ErpProductListImportDataInput() { } };

        ExcelConfig excelconfig = ExcelConfig.Default("商品信息导入模板.xls");
        foreach (KeyValuePair<string, string> item in Common.Extension.Extensions.GetPropertityToMaps<ErpProductListImportDataInput>())
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName!);
        ExcelExportHelper<ErpProductListImportDataInput>.Export(dataList, excelconfig, addPath);

        return new { name = excelconfig.FileName, url = "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    [HttpPost("ImportData")]
    public async Task<dynamic> ImportData(IFormFile file)
    {
        var _filePath = _fileManager.GetPathByType(string.Empty);
        var _fileName = DateTime.Now.ToString("yyyyMMdd") + "_" + SnowflakeIdHelper.NextId() + Path.GetExtension(file.FileName);
        var stream = file.OpenReadStream();
        await _fileManager.UploadFileByType(stream, _filePath, _fileName);

        try
        {
            Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpProductListImportDataInput>();

            //string? filePath = FileVariable.TemporaryFilePath;
            //string? savePath = Path.Combine(filePath, _fileName);

            // 得到数据
            var excelData = ExcelImportHelper.ToDataTable(file.OpenReadStream(), ExcelImportHelper.IsXls(_fileName));
            if (excelData != null)
            {
                foreach (DataColumn item in excelData.Columns)
                {
                    var key = FileEncode.Where(x => x.Value == item.ToString()).FirstOrDefault().Key;
                    if (!string.IsNullOrEmpty(key))
                    {
                        item.ColumnName = key;
                        //excelData.Columns[item.ToString()].ColumnName = ;
                    }
                }

            }

            var list = _repository.Context.Utilities.DataTableToList<ErpProductListImportDataInput>(excelData);

            //转化为实体 ErpProductEntity\ErpProductmodelEntity 
            List<ErpProductEntity> insertErpProductEntities = new List<ErpProductEntity>();
            List<ErpProductEntity> updateErpProductEntities = new List<ErpProductEntity>();
            List<ErpProductmodelEntity> insertErpProductmodelEntities = new List<ErpProductmodelEntity>();
            List<ErpProductmodelEntity> updateErpProductmodelEntities = new List<ErpProductmodelEntity>();
            List<ErpProductcompanyEntity> erpProductcompanyEntitys = new List<ErpProductcompanyEntity>();

            //商品分类、计量单位、供应商
            var erpProducttypeEntities = await _repository.Context.Queryable<ErpProducttypeEntity>().ToListAsync();
            var unitOptions = await _dictionaryDataService.GetList("JLDW");
            var erpSupplierEntities = await _repository.Context.Queryable<ErpSupplierEntity>().ToListAsync();

            //数据库中的记录
            var dbErpProductEntities = await _repository.Context.Queryable<ErpProductEntity>().ToListAsync();
            var dbErpProductmodelEntities = await _repository.Context.Queryable<ErpProductmodelEntity>().ToListAsync();

            var companys = await _repository.Context.Queryable<OrganizeEntity>().Where(it => it.DeleteTime == null).Select(it => new { it.Id, it.FullName }).ToListAsync();

            Dictionary<string, List<ErpProductmodelEntity>> kv = new Dictionary<string, List<ErpProductmodelEntity>>();
            Dictionary<string, ErpProductEntity> kvProduct = new Dictionary<string, ErpProductEntity>();

            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.name))
                {
                    throw Oops.Oh("商品名称不能为空！");
                }
                if (string.IsNullOrEmpty(item.model_name))
                {
                    throw Oops.Oh("规格不能为空！");
                }
                // 没有商品信息
                if (!kv.ContainsKey(item.name))
                {
                    // 判断商品是否已存在
                    var erpProductEntity = dbErpProductEntities.Find(x => x.Name == item.name);
                    if (erpProductEntity != null)
                    {
                        _repository.Context.Tracking(erpProductEntity);

                        item.Adapt(erpProductEntity);

                        updateErpProductEntities.Add(erpProductEntity);
                    }
                    else
                    {
                        erpProductEntity = item.Adapt<ErpProductEntity>();
                        erpProductEntity.Id = SnowflakeIdHelper.NextId();
                        erpProductEntity.FirstChar = PinyinHelper.PinyinString(erpProductEntity.Name);
                        insertErpProductEntities.Add(erpProductEntity);
                    }

                    #region 商品信息 ErpProductEntity
                    //设置分类、单位、供应商
                    var erpProducttype = erpProducttypeEntities.Find(x => x.Name == item.tidName);
                    if (erpProducttype != null)
                    {
                        erpProductEntity.Tid = erpProducttype.Id;
                    }
                    var erpSupplier = erpSupplierEntities.Find(x => x.Name == item.supplierName);
                    if (erpSupplier != null)
                    {
                        erpProductEntity.Supplier = erpSupplier.Id;
                    }
                    var unitOpt = unitOptions.Find(x => x.FullName == item.unit);
                    if (unitOpt != null)
                    {
                        erpProductEntity.Unit = unitOpt.EnCode;
                    }

                    #endregion

                    #region 关联公司
                    if (!string.IsNullOrEmpty(item.relationCompany))
                    {
                        var arr = item.relationCompany.Split(",");
                        var clist = companys.Where(x => arr.Contains(x.FullName)).Select(x => new ErpProductcompanyEntity
                        {
                            Id = SnowflakeIdHelper.NextId(),
                            Oid = x.Id,
                            Pid = erpProductEntity.Id
                        });

                        if (clist.IsAny())
                        {
                            erpProductcompanyEntitys.AddRange(clist);
                        }
                    }
                    #endregion

                    kv.Add(item.name, new List<ErpProductmodelEntity>());
                    kvProduct.Add(item.name, erpProductEntity);
                }

                #region 商品规格 ErpProductmodelEntity
                {
                    var pid = kvProduct[item.name].Id;
                    var erpProductmodelEntity = dbErpProductmodelEntities.Find(x => x.Name == item.model_name && x.Pid == pid);
                    if (erpProductmodelEntity != null)
                    {
                        _repository.Context.Tracking(erpProductmodelEntity);
                        item.Adapt(erpProductmodelEntity);

                        updateErpProductmodelEntities.Add(erpProductmodelEntity);
                    }
                    else
                    {
                        erpProductmodelEntity = item.Adapt<ErpProductmodelEntity>();
                        erpProductmodelEntity.Id = SnowflakeIdHelper.NextId();
                        insertErpProductmodelEntities.Add(erpProductmodelEntity);
                    }

                    //设置单位
                    var unitOpt = unitOptions.Find(x => x.FullName == item.model_unit);
                    if (unitOpt != null)
                    {
                        erpProductmodelEntity.Unit = unitOpt.EnCode;
                    }

                    erpProductmodelEntity.Pid = pid;

                    kv[item.name].Add(erpProductmodelEntity);
                }
                #endregion
            }

            // 开启事务
            _db.BeginTran();

            //更新数据库
            if (insertErpProductEntities.Any())
            {
                await _repository.Context.Insertable(insertErpProductEntities).EnableDiffLogEvent().ExecuteCommandAsync();
            }
            if (updateErpProductEntities.Any())
            {
                await _repository.Context.Updateable(updateErpProductEntities).EnableDiffLogEvent().ExecuteCommandAsync();
            }
            if (insertErpProductmodelEntities.Any())
            {
                await _repository.Context.Insertable(insertErpProductmodelEntities).EnableDiffLogEvent().ExecuteCommandAsync();
            }
            if (updateErpProductmodelEntities.Any())
            {
                await _repository.Context.Updateable(updateErpProductmodelEntities).EnableDiffLogEvent().ExecuteCommandAsync();
            }
            if (erpProductcompanyEntitys.IsAny())
            {
                var temp = erpProductcompanyEntitys.Select(x => x.Pid).Distinct().ToArray();
                await _repository.Context.Deleteable<ErpProductcompanyEntity>().Where(x => temp.Contains(x.Pid)).ExecuteCommandAsync();
                await _repository.Context.Insertable(erpProductcompanyEntitys).EnableDiffLogEvent().ExecuteCommandAsync();
            }
            // 关闭事务
            _db.CommitTran();

            return new { np = insertErpProductEntities.Count, up = updateErpProductEntities.Count, npm = insertErpProductmodelEntities.Count, upm = updateErpProductmodelEntities.Count };
            //return new UserImportResultOutput() { snum = list.Count, fnum = errorlist.Count, failResult = errorlist, resultType = errorlist.Count < 1 ? 0 : 1 };
        }
        catch (AppFriendlyException e)
        {
            _db.RollbackTran();

            throw e;
        }
        catch (Exception e)
        {
            // 回滚事务
            _db.RollbackTran();

            throw Oops.Oh(ErrorCode.D1805);
        }
    }

    /// <summary>
    /// 导出Excel.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    public async Task<dynamic> ExportExcel([FromQuery] ErpProductListQueryInput input)
    {
        List<string> ids = new List<string>();
        if (input.dataType == 0)
        {
            List<string> tidList = new List<string>();
            if (!string.IsNullOrEmpty(input.tid))
            {
                var tlist = await _repository.Context.Queryable<ErpProducttypeEntity>().ToChildListAsync(x => x.Fid, input.tid);

                if (tlist != null && tlist.Any())
                {
                    tidList = tlist.Select(x => x.Id).ToList();
                }
            }

            var pageIdList = await _repository.Context.Queryable<ErpProductEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.nickname), it => it.Nickname.Contains(input.nickname))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), it => it.FirstChar.Contains(input.firstChar))
            .WhereIF(!string.IsNullOrEmpty(input.no), it => it.No.Contains(input.no))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Nickname.Contains(input.keyword)
                || it.FirstChar.Contains(input.keyword)
                || it.No.Contains(input.keyword)
                )
            .WhereIF(tidList.Any(), it => tidList.Contains(it.Tid))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpProductListOutput
            {
                id = it.Id
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);

            ids = pageIdList.list.Select(x => x.id).ToList();
        }

        var sqlQuery = _repository.Context.Queryable<ErpProductmodelEntity>()
            .InnerJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
            .LeftJoin<ErpProducttypeEntity>((a, b, c) => b.Tid == c.Id)
            .LeftJoin<ErpSupplierEntity>((a, b, c, d) => b.Supplier == d.Id)
            .WhereIF(ids != null && ids.Any(), (a, b) => ids.Contains(b.Id))
            .OrderBy((a, b) => new { mid = b.Id, did = a.Id })
            .Select((a, b, c, d) => new ErpProductListExportDataOutput
            {
                id = b.Id,
                name = b.Name,
                nickname = b.Nickname,
                no = b.No,
                model_barCode = a.BarCode,
                model_costPrice = a.CostPrice,
                model_grossMargin = a.GrossMargin,
                model_maxNum = a.MaxNum,
                model_minNum = a.MinNum,
                model_name = a.Name,
                model_package = a.Package,
                model_ratio = a.Ratio,
                model_salePrice = a.SalePrice,
                model_unit = a.Unit,
                producer = b.Producer,
                retention = b.Retention,
                saletype = b.Saletype,
                sort = b.Sort,
                state = b.State,
                storage = b.Storage,
                unit = b.Unit,
                tidName = c.Name,
                supplierName = d.Name,
                rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
            });

        //List<ErpProductListImportDataInput> list = input.dataType == "0" ? await sqlQuery.ToPageListAsync(input.currentPage, input.pageSize) : await sqlQuery.ToListAsync();
        List<ErpProductListExportDataOutput> list = await sqlQuery.ToListAsync();

        // 获取关联公司
        var companys = await _repository.Context.Queryable<ErpProductcompanyEntity>()
            .InnerJoin<OrganizeEntity>((a, b) => a.Oid == b.Id)
            .Select((a, b) => new
            {
                id = a.Pid,
                name = b.FullName
            })
            .ToListAsync();


        var unitOptions = await _dictionaryDataService.GetList("JLDW");

        list.ForEach(item =>
        {
            item.unit = unitOptions.Find(x => x.EnCode == item.unit)?.FullName ?? "";
            item.model_unit = unitOptions.Find(x => x.EnCode == item.model_unit)?.FullName ?? "";
            item.state = item.state == "1" ? "启用" : "停用";

            var list = companys.Where(x => x.id == item.id).Select(x => x.name).ToArray();
            if (list.IsAny())
            {
                item.relationCompany = string.Join(",", list);
            }
        });

        ExcelConfig excelconfig = ExcelConfig.Default(string.Format("{0:yyyy-MM-dd}_商品信息.xls", DateTime.Now));
        Dictionary<string, string>? FileEncode = Common.Extension.Extensions.GetPropertityToMaps<ErpProductListExportDataOutput>();
        //var selectKey = input.selectKey.Split(',').ToList();
        foreach (KeyValuePair<string, string> item in FileEncode)
        {
            string? column = item.Key;
            string? excelColumn = item.Value;
            excelconfig.ColumnModel!.Add(new ExcelColumnModel() { Column = column, ExcelColumn = excelColumn });
        }

        string? addPath = Path.Combine(FileVariable.TemporaryFilePath, excelconfig.FileName);
        var fs = ExcelExportHelper<ErpProductListExportDataOutput>.ExportMemoryStream(list, excelconfig);
        var flag = await _fileManager.UploadFileByType(fs, FileVariable.TemporaryFilePath, excelconfig.FileName);
        if (flag.Item1)
        {
            fs.Flush();
            fs.Close();
        }

        return new { name = excelconfig.FileName, url = flag.Item2 ?? "/api/file/Download?encryption=" + DESCEncryption.Encrypt(_userManager.UserId + "|" + excelconfig.FileName + "|" + addPath, "QT") };
    }
    #endregion

    /// <summary>
    /// 测试视图.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("View")]
    [AllowAnonymous]
    public async Task<dynamic> GetViewList([FromQuery] ErpProductListQueryInput input)
    {
        return await _repository.Context.Queryable<ViewErpProduct>().ToPagedListAsync(1, 10);
    }

    /// <summary>
    /// 商品客户类型定价列表需要根据公司过滤
    /// </summary>
    /// <returns></returns>
    [HttpGet("ErpProductcustomertypeprice/OidList")]
    public async Task<dynamic> ErpProductcustomertypepriceOidList([FromQuery] ErpProductcustomertypepriceListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .LeftJoin<ErpProductmodelEntity>((it, a) => it.Gid == a.Id)
            .LeftJoin<ErpProductEntity>((it, a, b) => a.Pid == b.Id)
            .WhereIF(!string.IsNullOrEmpty(input.gid), it => it.Gid == input.gid)
            //.Where( it => (it.Oid == input.oid || it.Oid == null))
            //.WhereIF(!string.IsNullOrEmpty(input.oid), it => it.Oid == input.oid)
            .WhereIF(!string.IsNullOrEmpty(input.tid), it => it.Tid.Equals(input.tid))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Gid.Contains(input.keyword)
                || it.Tid.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select((it, a, b) => new ErpProductcustomertypepriceListOutput
            {
                id = it.Id,
                gid = it.Gid,
                tid = it.Tid,
                discount = it.Discount,
                price = it.Price,
                gidName = a.Name,
                productName = b.Name,
                pricingType = it.PricingType,
                oid = it.Oid
            }, true).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToListAsync();
        return new { list = data };
    }

    /// <summary>
    /// 根据规格查询价格，根据公司过滤
    /// 客户定价
    /// </summary>
    [HttpGet("ErpCustomerprice/OidList")]
    public async Task<dynamic> ErpCustomerOidList([FromQuery] ErpProductSelectorQueryInput pageInput)
    {
        var list = await _repository.Context.Queryable<ErpProductpriceEntity>()
            .ClearFilter<ICompanyEntity>()
            .WhereIF(!string.IsNullOrEmpty(pageInput.oid), x => SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Oid == pageInput.oid && d.Id == x.Cid).Any())
                     .Where(x => x.Gid == pageInput.gid)
                     .Select(x => new ErpProductCustomerPriceOutput
                     {
                         cidName = SqlFunc.Subqueryable<ErpCustomerEntity>().Where(d => d.Id == x.Cid).Select(d => d.Name),
                         price = x.Price
                     }, true)
                     .ToListAsync();

        return new { list };
    }

    /// <summary>
    /// 商品客户类型定价批量保存
    /// </summary>
    /// <returns></returns>
    [HttpPost("ErpProductcustomertypeprice/BatchSave/{id}")]
    public async Task ErpProductcustomertypepriceBatchSave([FromRoute] string id, [FromBody] ErpProductcustomertypepriceBatchSaveInput input)
    {
        //判断公司id+客户类型+规格是否存在
        if (input.list.IsAny())
        {
            var dups = input.list.GroupBy(x => new { x.oid, x.gid, x.tid })
                .Select(x => new { x.Key, Count = x.Count() })
                .Where(x => x.Count > 1);

            if (dups.Any())
            {
                throw Oops.Oh("公司+客户类型不能重复！");
            }
        }
        try
        {
            _db.BeginTran();


            //修改的数据
            var upList = input.list.Where(x => !string.IsNullOrEmpty(x.id) && x.id != "0").Adapt<List<ErpProductcustomertypepriceEntity>>();
            if (upList.Any())
            {
                await _repository.Context.Updateable(upList)
                     .IgnoreColumns(ignoreAllNullColumns: true)
                     .ExecuteCommandAsync();
            }

            //新增的数据
            var addList = input.list.Where(x => string.IsNullOrEmpty(x.id) || x.id == "0").Adapt<List<ErpProductcustomertypepriceEntity>>();
            if (addList.Any())
            {
                addList.ForEach(x => x.Id = SnowflakeIdHelper.NextId());

                await _repository.Context.Insertable(addList)
                    .IgnoreColumns(ignoreNullColumn: true)
                    .ExecuteCommandAsync();
            }

            // 删除数据
            if (input.delList.IsAny())
            {
                await _repository.Context.Deleteable<ErpProductcustomertypepriceEntity>().Where(x => input.delList.Contains(x.Id)).ExecuteCommandAsync();
            }

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
    /// 客户定价批量保存
    /// </summary>
    /// <returns></returns>
    [HttpPost("ErpCustomerprice/BatchSave/{id}")]
    public async Task ErpCustomerBatchSave([FromRoute] string id, [FromBody] ErpProductCustomerpriceBatchSaveInput input)
    {
        //判断客户id+规格是否存在
        if (input.list.IsAny())
        {
            var dups = input.list.GroupBy(x => new { x.cid, x.gid })
                .Select(x => new { x.Key, Count = x.Count() })
                .Where(x => x.Count > 1);

            if (dups.Any())
            {
                throw Oops.Oh("客户不能重复！");
            }
        }
        try
        {
            _db.BeginTran();



            //修改的数据
            var upList = input.list.Where(x => !string.IsNullOrEmpty(x.id) && x.id != "0").Adapt<List<ErpProductpriceEntity>>();
            if (upList.Any())
            {
                await _repository.Context.Updateable(upList)
                     .IgnoreColumns(ignoreAllNullColumns: true)
                     .ExecuteCommandAsync();
            }

            //新增的数据
            var addList = input.list.Where(x => string.IsNullOrEmpty(x.id) || x.id == "0").Adapt<List<ErpProductpriceEntity>>();
            if (addList.Any())
            {
                addList.ForEach(x => x.Id = SnowflakeIdHelper.NextId());

                await _repository.Context.Insertable(addList)
                    .IgnoreColumns(ignoreNullColumn: true)
                    .ExecuteCommandAsync();
            }

            // 删除数据
            if (input.delList.IsAny())
            {
                await _repository.Context.Deleteable<ErpProductpriceEntity>().Where(x => input.delList.Contains(x.Id)).ExecuteCommandAsync();
            }

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
    /// 选择商品信息的销售单价
    /// </summary>
    [NonAction]
    public async Task<List<QueryProductSalePriceOutput>> QueryProductSalePrice([FromQuery] ErpProductSelectorQueryInput pageInput)
    {
        string customerType = string.Empty;
        if (!string.IsNullOrEmpty(pageInput.cid))
        {
            customerType = await _repository.Context.Queryable<ErpCustomerEntity>().Where(x => x.Id == pageInput.cid).Select(x => x.Type).FirstAsync();
        }

        List<string> tidList = new List<string>();
        if (!string.IsNullOrEmpty(pageInput.tid))
        {
            var list = await _repository.Context.Queryable<ErpProducttypeEntity>().ToChildListAsync(x => x.Fid, pageInput.tid);

            if (list != null && list.Any())
            {
                tidList = list.Select(x => x.Id).ToList();
            }
        }

        // 计算单价
        var q1 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .Where(xxx => xxx.Oid == _userManager.CompanyId)
            .WhereIF(pageInput.gid.IsNotEmptyOrNull(), xxx => xxx.Gid == pageInput.gid);
        var q2 = _repository.Context.Queryable<ErpProductcustomertypepriceEntity>()
            .WhereIF(pageInput.gid.IsNotEmptyOrNull(), xxx => xxx.Gid == pageInput.gid)
            .Where(yyy => SqlFunc.IsNullOrEmpty(yyy.Oid) && !SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(xxx => xxx.Oid == _userManager.CompanyId && xxx.Tid == yyy.Tid && xxx.Gid == yyy.Gid).Any());
        var qur = _repository.Context.Union(q1, q2);

        var data = await _repository.Context.Queryable<ErpProductmodelEntity>()
            .LeftJoin<ErpProductEntity>((a, b) => a.Pid == b.Id)
            .LeftJoin<ErpProductpriceEntity>((a, b, c) => a.Id == c.Gid && c.Cid == pageInput.cid)
            .LeftJoin(qur, (a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
            .LeftJoin<ViewErpProducttypeEx>((a, b, c, d, e) => b.Tid == e.Id)
            //.LeftJoin<ErpProductcustomertypepriceEntity>((a, b, c, d) => a.Id == d.Gid && d.Tid == customerType)
            .Where((a, b) => b.State == "1")

            // 这种方式没有走索引
            //.Where((a, b) => SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => (pc.Pid == b.Id || pc.Pid == a.Id) && pc.Oid == _userManager.CompanyId).Any()
            //                                                  || !SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => (pc.Pid == b.Id || pc.Pid == a.Id)).Any())
            .Where((a, b) =>
            SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => pc.Pid == b.Id && pc.Oid == _userManager.CompanyId).Any() ||
            SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => pc.Pid == a.Id && pc.Oid == _userManager.CompanyId).Any() ||
            !(SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => pc.Pid == b.Id).Any() ||
            SqlFunc.Subqueryable<ErpProductcompanyEntity>().Where(pc => pc.Pid == a.Id).Any()))
            .WhereIF(!string.IsNullOrEmpty(pageInput.gid), (a, b) => a.Id == pageInput.gid)
            .WhereIF(pageInput.gidList.IsAny(), (a, b) => pageInput.gidList.Contains(a.Id))
            .WhereIF(tidList.Any(), (a, b) => tidList.Contains(b.Tid))
            .WhereIF(pageInput.filterPrice == true && !string.IsNullOrEmpty(pageInput.cid), (a, b) =>
                SqlFunc.Subqueryable<ErpProductcustomertypepriceEntity>().Where(ddd => ddd.Gid == a.Id && ddd.Tid == customerType && ddd.Oid == _userManager.CompanyId).Any()
            || SqlFunc.Subqueryable<ErpProductpriceEntity>().Where(ddd => ddd.Gid == a.Id && ddd.Cid == pageInput.cid && ddd.Oid == _userManager.CompanyId).Any()
            )
            .WhereIF(!string.IsNullOrEmpty(pageInput.keyword), (a, b) => a.Name.Contains(pageInput.keyword)
            || b.Name.Contains(pageInput.keyword) || b.No.Contains(pageInput.keyword) || b.Nickname.Contains(pageInput.keyword) || b.FirstChar.Contains(pageInput.keyword))
            .WhereIF(pageInput.hasNum == true, (a, b) => SqlFunc.Subqueryable<ErpInrecordEntity>().Where(ddd => ddd.Gid == a.Id && ddd.Num > 0).Any())
            .OrderByDescending((a, b, c, d, e) => new { e.ROrder, e.Order }) //根据商品分类排序
            .OrderBy((a) => a.Id)
            .Select((a, b, c, d, e) => new QueryProductSalePriceOutput
            {
                id = a.Id,
                salePrice = c.Price > 0 ? c.Price : d.PricingType == 1 ? a.SalePrice * d.Discount * 0.01m : d.PricingType == 2 ? d.Price : a.SalePrice, //优先使用客户定价，然后是客户类型折扣，再是客户类型价格，最后是原价
            }).Take(999)
            .ToListAsync();

        return data;
    }

    /// <summary>
    /// 修复出库成本
    /// </summary>
    /// <param name="enCode"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    [HttpPost("Actions/CalcCostAmount")]
    [AllowAnonymous]
    public async Task CalcCostAmount()
    {
        //db.Ado.UseStoredProcedure().SqlQuery<Class1>("sp_school", nameP, ageP)
        int count = await _repository.Ado.UseStoredProcedure().ExecuteCommandAsync("usp_erp_outorder_amount");
    }

    [HttpGet("changeLog")]
    public async Task<PageResult<SysLogFieldDto>> GetChangeLogFields([FromQuery] LogListQuery input)
    {
        var data = await _repository.Context.Queryable<SysLogFieldEntity>()
            .Where(x => x.ObjectId == input.logKey || SqlFunc.Subqueryable<ErpProductmodelEntity>().Where(q => q.Id == x.ObjectId && q.Pid == input.logKey).Any())
            .OrderByDescending(x => x.CreatorTime)
            .Select(x => new SysLogFieldDto
            {
                id = SqlFunc.ToString(x.Id),
                description = x.Description,
                fieldName = x.FieldName,
                newValue = x.NewValue,
                oldValue = x.OldValue,
                tableName = x.TableName,
                userName = SqlFunc.Subqueryable<UserEntity>().Where(z => z.Id == x.UserId).Select(z => z.RealName),
                createTime = x.CreatorTime
            })
            .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<SysLogFieldDto>.SqlSugarPagedList(data);
    }
}
using Mapster;
using QT.ClayObject;
using QT.DependencyInjection;
using QT.JXC.Entitys.Dto.Erp.ErpProducttype;
using QT.JXC.Interfaces;

namespace QT.JXC;

/// <summary>
/// 业务实现：商品分类管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpProducttype", Order = 200)]
[Route("api/Erp/[controller]")]
public class ErpProducttypeService : IErpProducttypeService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpProducttypeEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="ErpProducttypeService"/>类型的新实例.
    /// </summary>
    public ErpProducttypeService(
        ISqlSugarRepository<ErpProducttypeEntity> erpProducttypeRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        ICacheManager cacheManager)
    {
        _repository = erpProducttypeRepository;
        _userManager = userManager;
        _cacheManager = cacheManager;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取商品分类管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpProducttypeInfoOutput>();
    }

    /// <summary>
    /// 获取商品分类管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpProducttypeListQueryInput input)
    {
        List<string> tidList = new List<string>();
        if (!string.IsNullOrEmpty(input.fid))
        {
            var list = await _repository.Context.Queryable<ErpProducttypeEntity>().ToChildListAsync(x => x.Fid, input.fid);

            if (list != null && list.Any())
            {
                tidList = list.Select(x => x.Id).ToList();
            }
        }

        var data = await _repository.Context.Queryable<ErpProducttypeEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name) || it.FirstChar.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), it => it.FirstChar.Contains(input.firstChar))
            .WhereIF(tidList.Any(), it => tidList.Contains(it.Id))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Code.Contains(input.keyword)
                || it.FirstChar.Contains(input.keyword)
                )
            .OrderByDescending(it => it.Order)
            //.OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpProducttypeListOutput
            {
                id = it.Id,
                fid = it.Fid,
                name = it.Name,
                code = it.Code,
                firstChar = it.FirstChar,
                remark = it.Remark,
                order = it.Order,
                fidName = SqlFunc.Subqueryable<ErpProducttypeEntity>().Where(d => d.Id == it.Fid).Select(d => d.Name)
            })
            .OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpProducttypeListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建商品分类管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ErpProducttypeCrInput input)
    {
        var entity = input.Adapt<ErpProducttypeEntity>();
        if (await _repository.Context.Queryable<ErpProducttypeEntity>().AnyAsync(x => x.Name == entity.Name))
        {
            throw Oops.Oh("分类已存在，不允许重复录入！");
        }
        entity.Id = SnowflakeIdHelper.NextId();
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新商品分类管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpProducttypeUpInput input)
    {
        var entity = input.Adapt<ErpProducttypeEntity>();
        if (await _repository.Context.Queryable<ErpProducttypeEntity>().AnyAsync(x => x.Name == entity.Name && x.Id != entity.Id))
        {
            throw Oops.Oh("分类已存在，不允许重复录入！");
        }
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除商品分类管理.
    /// 删除对应的商品信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var list = await _repository.Context.Queryable<ErpProducttypeEntity>().ToChildListAsync(x => x.Fid, id);
        if (list==null || !list.Any())
        {
            throw Oops.Oh(ErrorCode.COM1002);
        }
        //var isOk = await _repository.Context.Deleteable<ErpProducttypeEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        //if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
        var idList = list.Select(x => x.Id).ToArray();
        var productIdList = await _repository.Context.Queryable<ErpProductEntity>().Where(x => idList.Contains(x.Tid)).Select(x => x.Id).ToListAsync();
        try
        {
            // 开启事务
            _db.BeginTran();

            await _repository.Context.Deleteable<ErpProducttypeEntity>().Where(it => idList.Contains(it.Id)).ExecuteCommandAsync();

            //如果有商品，一起删掉
            if (productIdList?.Count>0)
            {
                await _repository.Context.Deleteable<ErpProductEntity>().Where(it => productIdList.Contains(it.Id)).ExecuteCommandAsync();

                // 清空商品规格表数据
                await _repository.Context.Deleteable<ErpProductmodelEntity>().Where(it => productIdList.Contains(it.Pid)).ExecuteCommandAsync();

                // 清空商品图片表数据
                await _repository.Context.Deleteable<ErpProductpicEntity>().Where(it => productIdList.Contains(it.Pid)).ExecuteCommandAsync();

                // 清空商品视频表数据
                await _repository.Context.Deleteable<ErpProductvideoEntity>().Where(it => productIdList.Contains(it.Pid)).ExecuteCommandAsync();
            }

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
    [HttpGet("Selector/{id}")]
    public async Task<dynamic> GetSelector(string id)
    {
        //var data = await _cacheManager.GetAsync<List<ErpProducttypeEntity>>($"{nameof(ErpProducttypeEntity)}:Selector:{id}");
        //if (data == null)
        //{
        //    data = await _repository.AsQueryable().ToListAsync();
        //    data = data.OrderBy(o => o.Code).OrderByDescending(a => a.CreatorTime).ToList();
        //    await _cacheManager.SetAsync($"{nameof(ErpProducttypeEntity)}:Selector:{id}", data);
        //}
        List<ErpProducttypeEntity>? data = await _repository.AsQueryable().OrderBy(o => o.Code)
            .OrderBy(a=>a.Order, OrderByType.Desc)
            //.OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .ToListAsync();
        if (!"0".Equals(id)) data.RemoveAll(it => it.Id == id);

        List<ProducttypeSelectorOutput>? treeList = data.Select(item =>
        {
            var dto = item.Adapt<ProducttypeSelectorOutput>();
            dto.parentId = item.Fid;
            //dto.fullName = $"[{item.Order ?? 0}]{item.Name}";
            dto.fullName = item.Name;
            dto.data = item;
            dto.order = item.Order ?? 0;
            return dto;
        }).ToList();

        return new { list = treeList.OrderByDescending(x => x.order).ThenBy(x=>x.code).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取下拉框 只显示一级分类.
    /// </summary>
    /// <returns></returns>
    [HttpGet("RootType/Selector")]
    public async Task<dynamic> GetRootSelector()
    {
        List<ErpProducttypeEntity>? data = await _repository.AsQueryable()
            .OrderBy(o => o.Code)
            .Where(a=>SqlFunc.IsNullOrEmpty(a.Fid))
            .OrderBy(a => a.Order, OrderByType.Desc)
            //.OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .ToListAsync();

        return new
        {
            list = data.Select(item =>
            {
                var dto = item.Adapt<ProducttypeSelectorOutput>();
                dto.parentId = item.Fid;
                //dto.fullName = $"[{item.Order ?? 0}]{item.Name}";
                dto.fullName = item.Name;
                dto.data = item;
                dto.order = item.Order ?? 0;
                return dto;
            }).ToList()
        };
    }

    /// <summary>
    /// 获取商品分类管理无分页列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    private async Task<dynamic> GetNoPagingList([FromQuery] ErpProducttypeListQueryInput input)
    {
        return await _repository.Context.Queryable<ErpProducttypeEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), it => it.FirstChar.Contains(input.firstChar))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Code.Contains(input.keyword)
                || it.FirstChar.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpProducttypeListOutput
            {
                id = it.Id,
                fid = it.Fid,
                name = it.Name,
                code = it.Code,
                firstChar = it.FirstChar,
                remark = it.Remark,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToListAsync();
    }
    /// <summary>
    /// 导出商品分类管理.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Export")]
    public async Task<dynamic> Export([FromQuery] ErpProducttypeListQueryInput input)
    {
        var exportData = new List<ErpProducttypeListOutput>();
        if (input.dataType == 0)
            exportData = Clay.Object(await GetList(input)).Solidify<PageResult<ErpProducttypeListOutput>>().list;
        else
            exportData = await GetNoPagingList(input);
        List<ParamsModel> paramList = "[{\"value\":\"父级ID\",\"field\":\"fid\"},{\"value\":\"名称\",\"field\":\"name\"},{\"value\":\"分类编号\",\"field\":\"code\"},{\"value\":\"拼音首字母\",\"field\":\"firstChar\"},{\"value\":\"分类说明\",\"field\":\"remark\"},]".ToList<ParamsModel>();
        ExcelConfig excelconfig = new ExcelConfig();
        excelconfig.FileName = "商品分类管理.xls";
        excelconfig.HeadFont = "微软雅黑";
        excelconfig.HeadPoint = 10;
        excelconfig.IsAllSizeColumn = true;
        excelconfig.ColumnModel = new List<ExcelColumnModel>();
        foreach (var item in input.selectKey.Split(',').ToList())
        {
            var isExist = paramList.Find(p => p.field == item);
            if (isExist != null)
                excelconfig.ColumnModel.Add(new ExcelColumnModel() { Column = isExist.field, ExcelColumn = isExist.value });
        }

        var addPath = FileVariable.TemporaryFilePath + excelconfig.FileName;
        ExcelExportHelper<ErpProducttypeListOutput>.Export(exportData, excelconfig, addPath);
        var fileName = _userManager.UserId + "|" + addPath + "|xls";
        return new
        {
            name = excelconfig.FileName,
            url = "/api/File/Download?encryption=" + DESCEncryption.Encrypt(fileName, "QT")
        };
    }

    /// <summary>
    /// 更新节点顺序
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/ChangeOrder")]
    public async Task ChangeOrder([FromBody] ErpProducttypeChangeOrderInput input)
    {
        await _repository.Context.Updateable<ErpProducttypeEntity>()
            .SetColumns(x => new ErpProducttypeEntity { Order = input.order })
            .Where(x => x.Id == input.id)
            .ExecuteCommandAsync();
    }
}
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogProducttype;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common;
using QT.Common.Contracts;
using QT.Logistics.Entitys.Dto.LogStoreroom;

namespace QT.Logistics;

/// <summary>
/// 业务实现：商品分类.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "商品分类管理", Name = "LogProducttype", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogProducttypeService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogProducttypeEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogProducttypeService"/>类型的新实例.
    /// </summary>
    public LogProducttypeService(
        ISqlSugarRepository<LogProducttypeEntity> logProducttypeRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logProducttypeRepository;
        _userManager = userManager;

        _repository.Context.QueryFilter.AddTableFilter<IDeleteTime>(it => it.DeleteTime == null);
    }

    #region 增删改查
    /// <summary>
    /// 获取商品分类.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogProducttypeInfoOutput>();
    }

    /// <summary>
    /// 获取商品分类列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogProducttypeListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogProducttypeEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), it => it.FirstChar.Contains(input.firstChar))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.FirstChar.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogProducttypeListOutput
            {
                id = it.Id,
                fid = it.Fid,
                name = it.Name,
                firstChar = it.FirstChar,
                remark = it.Remark,
                fidName = SqlFunc.Subqueryable<LogProducttypeEntity>().Where(x=>x.Id == it.Fid).Select(x=>x.Name),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogProducttypeListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建商品分类.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogProducttypeCrInput input)
    {
        var entity = input.Adapt<LogProducttypeEntity>();
        if (await _repository.Where(it => it.Name == entity.Name).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        entity.Id = SnowflakeIdHelper.NextId();
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新商品分类.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogProducttypeUpInput input)
    {
        var entity = input.Adapt<LogProducttypeEntity>();
        if (entity.Id == entity.Fid)
        {
            throw Oops.Oh("上级分类不能选择当前分类！");
        }
        if (await _repository.Where(it => it.Name == entity.Name && it.Id!=entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除商品分类.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Updateable<LogProducttypeEntity>(new LogProducttypeEntity { Id = id})
            .CallEntityMethod(it => it.Delete())
            .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId })
            .ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion

    /// <summary>
    /// 下拉选择 树形下拉
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector/Tree")]
    public async Task<dynamic> TreeSelector()
    {
        var data = await _repository.ToListAsync();

        var treeList = data.Adapt<List<LogProducttypeTreeListOutput>>();

        return new { list = treeList.ToTree("0") };
    }
}
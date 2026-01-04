using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcStoreroom;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.Dto.Module;
using QT.Common.Const;

namespace QT.JZRC;

/// <summary>
/// 业务实现：档案室管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.JZRC, Tag = "JZRC", Name = "JzrcStoreroom", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcStoreroomService : IJzrcStoreroomService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcStoreroomEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcStoreroomService"/>类型的新实例.
    /// </summary>
    public JzrcStoreroomService(
        ISqlSugarRepository<JzrcStoreroomEntity> jzrcStoreroomRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcStoreroomRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取档案室管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcStoreroomInfoOutput>();
    }

    /// <summary>
    /// 获取档案室管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcStoreroomListQueryInput input)
    {
        //var data = await _repository.Context.Queryable<JzrcStoreroomEntity>()
        //    .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
        //    .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
        //        it.Name.Contains(input.keyword)
        //        )
        //    .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
        //    .Select(it => new JzrcStoreroomListOutput
        //    {
        //        id = it.Id,
        //        name = it.Name,
        //        remark = it.Remark,
        //        pId = it.PId,
        //    }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        //return PageResult<JzrcStoreroomListOutput>.SqlSugarPageResult(data);

        var data = await _repository.AsQueryable().OrderBy(o => o.Id).ToListAsync();
 
        if (!string.IsNullOrEmpty(input.name))
            data = data.TreeWhere(t => t.Name.Contains(input.name), t => t.Id, t => t.PId);
        var treeList = data.Adapt<List<JzrcStoreroomListOutput>>();

        //treeList.ForEach(x =>
        //{
        //    x.pIdName = data.Find(w => w.Id == x.pId)?.Name ?? "";
        //});

        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 新建档案室管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcStoreroomCrInput input)
    {
        var entity = input.Adapt<JzrcStoreroomEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        await ValidateEntity(entity);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新档案室管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcStoreroomUpInput input)
    {
        var entity = input.Adapt<JzrcStoreroomEntity>();
        await ValidateEntity(entity);
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }



    /// <summary>
    /// 删除档案室管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        //var isOk = await _repository.Context.Deleteable<JzrcStoreroomEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        //if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        var entity = await _repository.Context.Queryable<JzrcStoreroomEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        _repository.Context.Tracking(entity);
        entity.Delete();
        var isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 校验实体
    /// </summary>
    /// <returns></returns>
    private async Task ValidateEntity(JzrcStoreroomEntity entity)
    {
        if (await _repository.Context.Queryable<JzrcStoreroomEntity>().AnyAsync(x => x.Id != entity.Id && x.PId == entity.PId && x.Name == entity.Name))
        {
            throw Oops.Oh("相同层级，存在相同的名称！");
        }
    }

    /// <summary>
    /// 获取菜单下拉框.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Actions/Selector/{id}")]
    public async Task<dynamic> GetSelector(string id)
    {
        var data = await _repository.AsQueryable().OrderBy(o => o.Id).ToListAsync();
        
        if (!id.Equals("0"))
            data.RemoveAll(x => x.Id == id);

        var treeList = data.Adapt<List<JzrcStoreroomSelectorOutput>>();
        return new { list = treeList.ToTree("-1") };
    }
}
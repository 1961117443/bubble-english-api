using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.VisualData.Entity;
using QT.VisualData.Entitys.Dto.ScreenCategory;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;

namespace QT.VisualData;

/// <summary>
/// 业务实现：大屏.
/// </summary>
[ApiDescriptionSettings(Tag = "BladeVisual", Name = "category", Order = 160)]
[Route("api/blade-visual/[controller]")]
public class ScreenCategoryService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<VisualCategoryEntity> _visualCategoryRepository;

    /// <summary>
    /// 初始化一个<see cref="ScreenCategoryService"/>类型的新实例.
    /// </summary>
    public ScreenCategoryService(ISqlSugarRepository<VisualCategoryEntity> visualCategoryRepository)
    {
        _visualCategoryRepository = visualCategoryRepository;
    }

    #region Get

    /// <summary>
    /// 获取大屏分类分页列表.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("page")]
    public async Task<dynamic> GetPagetList([FromQuery] ScreenCategoryListQueryInput input)
    {
        SqlSugarPagedList<ScreenCategoryListOutput>? data = await _visualCategoryRepository.AsQueryable().Where(v => v.IsDeleted == 0).Select(v => new ScreenCategoryListOutput { id = v.Id, categoryKey = v.CategoryKey, categoryValue = v.CategoryValue, isDeleted = v.IsDeleted }).ToPagedListAsync(input.current, input.size);
        return new { current = data.pagination.PageIndex, pages = data.pagination.Total / data.pagination.PageSize, records = data.list, size = data.pagination.PageSize, total = data.pagination.Total };
    }

    /// <summary>
    /// 获取大屏分类列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<dynamic> GetList([FromQuery] ScreenCategoryListQueryInput input)
    {
        SqlSugarPagedList<ScreenCategoryListOutput>? list = await _visualCategoryRepository.AsQueryable().Where(v => v.IsDeleted == 0)
            .Select(v => new ScreenCategoryListOutput
            {
                id = v.Id,
                categoryKey = v.CategoryKey,
                categoryValue = v.CategoryValue,
                isDeleted = v.IsDeleted
            }).ToPagedListAsync(input.current, input.size);
        return list.list;
    }

    /// <summary>
    /// 详情.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("detail")]
    public async Task<dynamic> GetInfo(string id)
    {
        VisualCategoryEntity? entity = await _visualCategoryRepository.AsQueryable().FirstAsync(v => v.Id == id);
        return entity.Adapt<ScreenCategoryInfoOutput>();
    }

    #endregion

    #region Post

    /// <summary>
    /// 新增.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("save")]
    public async Task Create([FromBody] ScreenCategoryCrInput input)
    {
        bool isExist = await _visualCategoryRepository.AnyAsync(v => v.CategoryValue == input.categoryValue && v.IsDeleted == 0);
        if (isExist) throw Oops.Oh(ErrorCode.D2200);
        VisualCategoryEntity? entity = input.Adapt<VisualCategoryEntity>();
        entity.IsDeleted = 0;
        entity.Id = YitIdHelper.NextId().ToString();
        int isOk = await _visualCategoryRepository.Context.Insertable(entity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("update")]
    public async Task Update([FromBody] ScreenCategoryUpInput input)
    {
        bool isExist = await _visualCategoryRepository.AnyAsync(v => v.CategoryValue == input.categoryValue && v.Id != input.Id && v.IsDeleted == 0);
        if (isExist) throw Oops.Oh(ErrorCode.D2200);
        VisualCategoryEntity? entity = input.Adapt<VisualCategoryEntity>();
        int isOk = await _visualCategoryRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPost("remove")]
    public async Task Delete(string ids)
    {
        List<VisualCategoryEntity>? entity = await _visualCategoryRepository.AsQueryable().In(v => v.Id, ids.Split(',').ToArray()).Where(v => v.IsDeleted == 0).ToListAsync();
        _ = entity ?? throw Oops.Oh(ErrorCode.COM1005);
        int isOk = await _visualCategoryRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.Delete()).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    #endregion
}

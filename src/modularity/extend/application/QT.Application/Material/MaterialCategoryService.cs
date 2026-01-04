using QT.Common.Const;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.Iot.Application.Dto.MaterialCategory;
using QT.Iot.Application.Entity;

namespace QT.Iot.Application.Service;

/// <summary>
/// 业务实现：物资类别.
/// </summary>
[ApiDescriptionSettings("扩展应用", Tag = "物资类别", Name = "MaterialCategory", Order = 300)]
[Route("api/iot/apply/material-category")]
public class MaterialCategoryService : QTBaseService<MaterialCategoryEntity, MaterialCategoryCrInput, MaterialCategoryUpInput, MaterialCategoryOutput, MaterialCategoryListQueryInput, MaterialCategoryListOutput>, IDynamicApiController
{
    public MaterialCategoryService(ISqlSugarRepository<MaterialCategoryEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }


    public override async Task<PageResult<MaterialCategoryListOutput>> GetList([FromQuery] MaterialCategoryListQueryInput input)
    {
        var data = await _repository.Context.Queryable<MaterialCategoryEntity>().ToListAsync();

        if (!string.IsNullOrEmpty(input.keyword))
            data = data.TreeWhere(t => t.Name.Contains(input.keyword) || t.Code.Contains(input.keyword), t => t.Id, t => t.ParentId);
        var treeList = data.Adapt<List<MaterialCategoryListOutput>>();
        return new PageResult<MaterialCategoryListOutput> { list = treeList.ToTree("-1"), pagination = new PageResult() };
    }


    /// <summary>
    /// 获取菜单下拉框.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="category">菜单分类（参数有Web,App），默认显示所有分类.</param>
    /// <returns></returns>
    [HttpGet("Selector/{id}")]
    public async Task<dynamic> GetSelector(string id)
    {
        var data = await _repository.Entities.ToListAsync();
        if (!id.Equals("0"))
            data.RemoveAll(x => x.Id == id);
        var treeList = data.Adapt<List<MaterialCategoryListOutput>>();
        return new { list = treeList.ToTree("-1") };
    }
}

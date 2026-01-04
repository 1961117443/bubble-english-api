using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Asset.Dto.AssetCategory;
using QT.Asset.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Entitys.Dto.Department;
using QT.Systems.Entitys.Permission;
using SqlSugar;
using QT.Common.Security;
using QT.Systems.Entitys.Dto.Module;
using QT.Common.Extension;
using QT.Asset.Dto.AssetAttributeDefinition;
using QT.Common.Core.Security;
using QT.FriendlyException;
using QT.Common.Enum;

namespace QT.Asset;

/// <summary>
/// 分类管理
/// </summary>
[ApiDescriptionSettings("资产管理", Tag = "分类管理", Name = "AssetCategory", Order = 601)]
[Route("api/extend/asset/[controller]")]
public class AssetCategoryService : QTBaseService<AssetCategoryEntity, AssetCategoryCrInput, AssetCategoryUpInput, AssetCategoryInfoOutput, AssetCategoryListPageInput, AssetCategoryListOutput>, IDynamicApiController, ITransient
{
    /// <summary>
    /// 
    /// </summary>
    private readonly ISqlSugarRepository<AssetCategoryEntity> _repository;

    /// <summary>
    /// 初始化分类服务实例
    /// </summary>
    /// <param name="repository">分类实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public AssetCategoryService(ISqlSugarRepository<AssetCategoryEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public override async Task<AssetCategoryInfoOutput> GetInfo(string id)
    {
        var entity = await base.GetInfo(id);


        var list = await _repository.Context.Queryable<AssetAttributeDefinitionEntity>().Where(x => x.CategoryId == id).ToListAsync();

        entity.fields = list.Adapt<List<AssetAttributeDefinitionDto>>();

        return entity;
    }

    protected override async Task BeforeCreate(AssetCategoryCrInput input, AssetCategoryEntity entity)
    {
        if (input.fields.IsAny())
        {
            var fields = input.fields.Adapt<List<AssetAttributeDefinitionEntity>>();
            fields.ForEach(it =>
            {
                it.CategoryId = entity.Id;
                it.Id = SnowflakeIdHelper.NextId().ToString();
            });
            await _repository.Context.Insertable<AssetAttributeDefinitionEntity>(fields).ExecuteCommandAsync();
        }
    }

    protected async override Task AfterUpdate(AssetCategoryUpInput input, AssetCategoryEntity entity)
    {
        await _repository.Context.CUDSaveAsnyc<AssetAttributeDefinitionEntity, AssetAttributeDefinitionDto>(it => it.CategoryId == entity.Id, input.fields, it => it.CategoryId = entity.Id);
    }



    public override async Task<PageResult<AssetCategoryListOutput>> GetList([FromQuery] AssetCategoryListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetCategoryEntity>()
            .Select<AssetCategoryListOutput>(it => new AssetCategoryListOutput
            {
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<AssetCategoryListOutput>.SqlSugarPagedList(data);
    }

    [HttpGet("tree")]
    public async Task<List<AssetCategorySelectorOutput>> GetTreeList([FromQuery] AssetCategoryListPageInput input)
    {
        var data = await _repository.Context.Queryable<AssetCategoryEntity>().ToListAsync();
         

        if (!string.IsNullOrEmpty(input.name))
            data = data.TreeWhere(t => t.Name.Contains(input.name) , t => t.Id, t => t.ParentId);

        var treeList = data.Adapt<List<AssetCategorySelectorOutput>>();
        return treeList.ToTree("-1");
    }

    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector/{id}")]
    public async Task<dynamic> GetSelector(string id)
    {
        var data = await _repository.AsQueryable().Where(t => t.DeleteMark == null).OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();
        if (!"0".Equals(id)) data.RemoveAll(it => it.Id == id);

        List<AssetCategorySelectorOutput>? treeList = data.Adapt<List<AssetCategorySelectorOutput>>();
 
        return new { list = treeList.ToList().ToTree("-1") };
    }


}
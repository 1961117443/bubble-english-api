using Mapster;
using Microsoft.AspNetCore.Mvc;
using QT.Archive.Dto.Building;
using QT.Archive.Entity;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Module;
using SqlSugar;

namespace QT.Archive;

/// <summary>
/// 档案馆管理
/// </summary>
[ApiDescriptionSettings("档案管理", Tag = "档案馆管理", Name = "Building", Order = 601)]
[Route("api/extend/archive/Building")]
public class BuildingService : QTBaseService<ArchivesBuildingEntity, BuildingCrInput, BuildingUpInput, BuildingInfoOutput, BuildingListPageInput, BuildingListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ArchivesBuildingEntity> _repository;

    /// <summary>
    /// 初始化档案馆管理服务实例
    /// </summary>
    /// <param name="repository">档案馆实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public BuildingService(ISqlSugarRepository<ArchivesBuildingEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }


    /// <summary>
    /// 获取树形列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("tree/list")]
    public async Task<List<BuildingListTreeOutput>> GetTreeList([FromQuery] BuildingListPageInput input)
    {
        var data = await _repository.Context.Queryable< ArchivesBuildingEntity>().ToListAsync();
      
        if (!string.IsNullOrEmpty(input.keyword))
            data = data.TreeWhere(t => t.Name.Contains(input.keyword) || t.Code.Contains(input.keyword), t => t.Id, t => t.Pid);

        var treeList = data.Adapt<List<BuildingListTreeOutput>>();
        return treeList.ToTree(t => t.id, "-1");
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
        var data = await _repository.Context.Queryable<ArchivesBuildingEntity>().ToListAsync();
      
        if (!id.Equals("0"))
            data.RemoveAll(x => x.Id == id);
        var treeList = data.Adapt<List<BuildingListTreeOutput>>();
        return new { list = treeList.ToTree(t => t.id, "-1") };
    }

    protected override async Task BeforeCreate(BuildingCrInput input, ArchivesBuildingEntity entity)
    {
        // 检查相同层级是否有相同的编号
        if (await _repository.AsQueryable().Where(x => x.Pid == entity.Pid && x.Code == entity.Code).AnyAsync())
        {
            throw Oops.Oh($"编号【{entity.Code}】已存在");
        }
    }


    protected async override Task BeforeUpdate(BuildingUpInput input, ArchivesBuildingEntity entity)
    {
        // 检查相同层级是否有相同的编号
        if (await _repository.AsQueryable().Where(x => x.Pid == entity.Pid && x.Code == entity.Code && x.Id!=entity.Id).AnyAsync())
        {
            throw Oops.Oh($"编号【{entity.Code}】已存在");
        }
    }
}




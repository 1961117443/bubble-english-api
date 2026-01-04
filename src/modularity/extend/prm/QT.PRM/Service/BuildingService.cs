using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Building;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 楼栋管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "楼栋管理", Name = "Building", Order = 601)]
[Route("api/extend/prm/[controller]")]
public class BuildingService : QTBaseService<BuildingEntity, BuildingCrInput, BuildingUpInput, BuildingInfoOutput, BuildingListPageInput, BuildingListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<BuildingEntity> _repository;

    /// <summary>
    /// 初始化楼栋服务实例
    /// </summary>
    /// <param name="repository">楼栋实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public BuildingService(ISqlSugarRepository<BuildingEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public override async Task<PageResult<BuildingListOutput>> GetList([FromQuery] BuildingListPageInput input)
    {
        var data = await _repository.Context.Queryable<BuildingEntity>()
            .Select<BuildingListOutput>(it=> new BuildingListOutput
            {
                communityIdName = SqlFunc.Subqueryable<CommunityEntity>().Where(ddd=>ddd.Id == it.CommunityId).Select(ddd=>ddd.Name),
            },true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<BuildingListOutput>.SqlSugarPagedList(data);
    }

    [HttpGet("option-list")]
    public async Task<List<BuildingListOutput>> GetOptionList()
    {
        return await _repository.AsQueryable()
            .InnerJoin<CommunityEntity>((a, b) => a.CommunityId == b.Id)
            .Select((a, b) => new BuildingListOutput
            {
                id = a.Id,
                communityIdName = SqlFunc.MergeString(b.Name, "/", a.Code)
            })
            .ToListAsync();
    }
}




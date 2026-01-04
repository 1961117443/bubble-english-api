using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Building;
using QT.PRM.Dto.Room;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 房间管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "房间管理", Name = "Room", Order = 602)]
[Route("api/extend/prm/[controller]")]
public class RoomService : QTBaseService<RoomEntity, RoomCrInput, RoomUpInput, RoomInfoOutput, RoomListPageInput, RoomListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<RoomEntity> _repository;

    /// <summary>
    /// 初始化房间服务实例
    /// </summary>
    /// <param name="repository">房间实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public RoomService(ISqlSugarRepository<RoomEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public override async Task<PageResult<RoomListOutput>> GetList([FromQuery] RoomListPageInput input)
    {
        var data = await _repository.Context.Queryable<RoomEntity>()
            .Select<RoomListOutput>(it => new RoomListOutput
            {
                communityBuilding = SqlFunc.Subqueryable<CommunityEntity>().LeftJoin<BuildingEntity>((a,b)=>a.Id == b.CommunityId)
                .Where((a,b)=> b.Id== it.BuildingId).Select((a,b)=> SqlFunc.MergeString(a.Name,"/",b.Code)),
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<RoomListOutput>.SqlSugarPagedList(data);
    }

    [HttpGet("option-list")]
    public async Task<List<RoomListOutput>> GetOptionList()
    {
        return await _repository.AsQueryable()
            .InnerJoin<BuildingEntity>((x,a)=>x.BuildingId == a.Id)
            .InnerJoin<CommunityEntity>((x,a, b) => a.CommunityId == b.Id)
            .Select((x,a, b) => new RoomListOutput
            {
                id = x.Id,
                communityBuilding = SqlFunc.MergeString(b.Name, "/", a.Code,"/",x.RoomNumber)
            })
            .ToListAsync();
    }
}




using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Resident;
using QT.PRM.Dto.Room;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 住户管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "住户管理", Name = "Resident", Order = 603)]
[Route("api/extend/prm/[controller]")]
public class ResidentService : QTBaseService<ResidentEntity, ResidentCrInput, ResidentUpInput, ResidentInfoOutput, ResidentListPageInput, ResidentListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ResidentEntity> _repository;

    /// <summary>
    /// 初始化住户服务实例
    /// </summary>
    /// <param name="repository">住户实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public ResidentService(ISqlSugarRepository<ResidentEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public override async Task<PageResult<ResidentListOutput>> GetList([FromQuery] ResidentListPageInput input)
    {
        var data = await _repository.Context.Queryable<ResidentEntity>()
          .Select<ResidentListOutput>(it => new ResidentListOutput
          {
              roomName = SqlFunc.Subqueryable<CommunityEntity>()
              .LeftJoin<BuildingEntity>((a, b) => a.Id == b.CommunityId)
              .LeftJoin<RoomEntity>((a, b, c) => b.Id == c.BuildingId)
              .Where((a, b, c) => c.Id == it.RoomId).Select((a, b, c) => SqlFunc.MergeString(a.Name, "/", b.Code, "/", c.RoomNumber)),
          }, true)
          .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ResidentListOutput>.SqlSugarPagedList(data);
    }

    [HttpGet("option-list")]
    public async Task<List<ResidentListOutput>> GetOptionList()
    {
        return await _repository.AsQueryable()
            .InnerJoin<RoomEntity>((w,x)=>w.RoomId == x.Id)
            .InnerJoin<BuildingEntity>((w,x, a) => x.BuildingId == a.Id)
            .InnerJoin<CommunityEntity>((w,x, a, b) => a.CommunityId == b.Id)
            .Select((w,x, a, b) => new ResidentListOutput
            {
                id = w.Id,
                roomName = SqlFunc.MergeString(b.Name, "/", a.Code, "/", x.RoomNumber,"/",w.Name)
            })
            .ToListAsync();
    }
}




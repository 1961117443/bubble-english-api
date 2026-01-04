using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Parking;
using QT.PRM.Dto.Room;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 停车位管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "停车位管理", Name = "Parking", Order = 601)]
[Route("api/extend/prm/[controller]")]
public class ParkingService : QTBaseService<ParkingEntity, ParkingCrInput, ParkingUpInput, ParkingInfoOutput, ParkingListPageInput, ParkingListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ParkingEntity> _repository;

    /// <summary>
    /// 初始化停车位服务实例
    /// </summary>
    /// <param name="repository">停车位实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public ParkingService(ISqlSugarRepository<ParkingEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public override async Task<PageResult<ParkingListOutput>> GetList([FromQuery] ParkingListPageInput input)
    {
        var data = await _repository.Context.Queryable<ParkingEntity>()
          .Select<ParkingListOutput>(it => new ParkingListOutput
          {
              roomName = SqlFunc.Subqueryable<CommunityEntity>()
              .LeftJoin<BuildingEntity>((a, b) => a.Id == b.CommunityId)
              .LeftJoin<RoomEntity>((a,b,c)=> b.Id == c.BuildingId)
              .Where((a, b,c) => c.Id == it.RoomId).Select((a, b,c) => SqlFunc.MergeString(a.Name, "/", b.Code,"/",c.RoomNumber)),
          }, true)
          .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ParkingListOutput>.SqlSugarPagedList(data);
    }
}




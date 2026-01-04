using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Parking;
using QT.PRM.Dto.RoomFee;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 房间收费管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "房间收费管理", Name = "RoomFee", Order = 604)]
[Route("api/extend/prm/[controller]")]
public class RoomFeeService : QTBaseService<RoomFeeEntity, RoomFeeCrInput, RoomFeeUpInput, RoomFeeInfoOutput, RoomFeeListPageInput, RoomFeeListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<RoomFeeEntity> _repository;

    /// <summary>
    /// 初始化房间收费服务实例
    /// </summary>
    /// <param name="repository">房间收费实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public RoomFeeService(ISqlSugarRepository<RoomFeeEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public async override Task<PageResult<RoomFeeListOutput>> GetList([FromQuery] RoomFeeListPageInput input)
    {
        var data = await _repository.Context.Queryable<RoomFeeEntity>()
         .Select<RoomFeeListOutput>(it => new RoomFeeListOutput
         {
             roomName = SqlFunc.Subqueryable<CommunityEntity>()
             .LeftJoin<BuildingEntity>((a, b) => a.Id == b.CommunityId)
             .LeftJoin<RoomEntity>((a, b, c) => b.Id == c.BuildingId)
             .Where((a, b, c) => c.Id == it.RoomId).Select((a, b, c) => SqlFunc.MergeString(a.Name, "/", b.Code, "/", c.RoomNumber)),
             feeItemIdName = SqlFunc.Subqueryable<FeeItemEntity>()
             .Where(a => a.Id == it.FeeId).Select(a => a.Name),
         }, true)
         .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<RoomFeeListOutput>.SqlSugarPagedList(data);
    }
}




using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Resident;
using QT.PRM.Dto.Visitor;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 访客管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "访客管理", Name = "Visitor", Order = 606)]
[Route("api/extend/prm/[controller]")]
public class VisitorService : QTBaseService<VisitorEntity, VisitorCrInput, VisitorUpInput, VisitorInfoOutput, VisitorListPageInput, VisitorListOutput>, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<VisitorEntity> _repository;

    /// <summary>
    /// 初始化访客服务实例
    /// </summary>
    /// <param name="repository">访客实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public VisitorService(ISqlSugarRepository<VisitorEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
        _repository = repository;
    }

    public async override Task<PageResult<VisitorListOutput>> GetList([FromQuery] VisitorListPageInput input)
    {
        var data = await _repository.Context.Queryable<VisitorEntity>()
         .Select<VisitorListOutput>(it => new VisitorListOutput
         {
             residentName = SqlFunc.Subqueryable<CommunityEntity>()
             .LeftJoin<BuildingEntity>((a, b) => a.Id == b.CommunityId)
             .LeftJoin<RoomEntity>((a, b, c) => b.Id == c.BuildingId)
             .LeftJoin<ResidentEntity>((a, b, c,d) => c.Id == d.RoomId)
             .Where((a, b, c,d) => d.Id == it.ResidentId).Select((a, b, c,d) => SqlFunc.MergeString(a.Name, "/", b.Code, "/", c.RoomNumber,"/",d.Name)),
         }, true)
         .ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<VisitorListOutput>.SqlSugarPagedList(data);
    }
}




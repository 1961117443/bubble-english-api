using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Community;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
    /// 苑区管理
    /// </summary>
[ApiDescriptionSettings("物业管理", Tag = "苑区管理", Name = "Community", Order = 600)]
[Route("api/extend/prm/[controller]")]
public class CommunityService : QTBaseService<CommunityEntity, CommunityCrInput, CommunityUpInput, CommunityInfoOutput, CommunityListPageInput, CommunityListOutput>, IDynamicApiController, ITransient
{
    public CommunityService(ISqlSugarRepository<CommunityEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }
}

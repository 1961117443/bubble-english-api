using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.FeeItem;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 收费项目管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "收费项目管理", Name = "FeeItem", Order = 604)]
[Route("api/extend/prm/[controller]")]
public class FeeItemService : QTBaseService<FeeItemEntity, FeeItemCrInput, FeeItemUpInput, FeeItemInfoOutput, FeeItemListPageInput, FeeItemListOutput>, IDynamicApiController, ITransient
{
    /// <summary>
    /// 初始化收费项目服务实例
    /// </summary>
    /// <param name="repository">收费项目实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public FeeItemService(ISqlSugarRepository<FeeItemEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }
}




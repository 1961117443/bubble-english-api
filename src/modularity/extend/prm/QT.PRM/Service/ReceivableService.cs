using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Receivable;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 应收管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "应收管理", Name = "Receivable", Order = 605)]
[Route("api/extend/prm/[controller]")]
public class ReceivableService : QTBaseService<ReceivableEntity, ReceivableCrInput, ReceivableUpInput, ReceivableInfoOutput, ReceivableListPageInput, ReceivableListOutput>, IDynamicApiController, ITransient
{
    /// <summary>
    /// 初始化应收服务实例
    /// </summary>
    /// <param name="repository">应收实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public ReceivableService(ISqlSugarRepository<ReceivableEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }
}

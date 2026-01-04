using Microsoft.AspNetCore.Mvc;
using QT.Common.Core;
using QT.Common.Core.Manager;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.PRM.Dto.Payment;
using QT.PRM.Entitys;
using SqlSugar;

namespace QT.PRM;

/// <summary>
/// 收费流水管理
/// </summary>
[ApiDescriptionSettings("物业管理", Tag = "收费流水管理", Name = "Payment", Order = 605)]
[Route("api/extend/prm/[controller]")]
public class PaymentService : QTBaseService<PaymentEntity, PaymentCrInput, PaymentUpInput, PaymentInfoOutput, PaymentListPageInput, PaymentListOutput>, IDynamicApiController, ITransient
{
    /// <summary>
    /// 初始化收费流水服务实例
    /// </summary>
    /// <param name="repository">收费流水实体仓库</param>
    /// <param name="context">SQL Sugar客户端</param>
    /// <param name="userManager">用户管理器</param>
    public PaymentService(ISqlSugarRepository<PaymentEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }
}




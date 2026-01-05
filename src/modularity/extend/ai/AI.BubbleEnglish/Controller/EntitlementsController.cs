using AI.BubbleEnglish.Dto;
using Microsoft.AspNetCore.Mvc;
using QT;
using QT.Common.Const;
using QT.Common.Security;
using QT.DynamicApiController;
using QT.FriendlyException;

namespace AI.BubbleEnglish.Controller;

/// <summary>
/// 权益查询（会员/主题包）。
/// 目前先返回默认值，后续可对接订单/订阅表。
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleEnglish", Name = "Entitlements", Order = 50)]
[Route("api/bubbleEnglish/entitlements")]
public class EntitlementsController : IDynamicApiController
{
    [HttpGet("me")]
    public EntitlementsResp GetMine()
    {
        // TODO: 从 BubbleSubscriptionPlan/BubbleOrder/PaymentRecord 等表计算实际权益
        _ = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? throw Oops.Oh("未登录");

        return new EntitlementsResp
        {
            IsVip = false,
            Themes = new List<string>()
        };
    }
}

namespace AI.BubbleEnglish;

/// <summary>
/// 权益查询：会员只解锁进阶，主题包单卖
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleEnglish", Name = "Entitlements", Order = 1040)]
[Route("api/bubble/client/[controller]")]
public class BubbleEntitlementsService : IDynamicApiController, ITransient
{
    private readonly SqlSugarClient _db;
    private readonly IUserManager _userManager;

    public BubbleEntitlementsService(ISqlSugarClient context, IUserManager userManager)
    {
        _db = (SqlSugarClient)context;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取当前账号权益
    /// </summary>
    [HttpGet("me")]
    public async Task<EntitlementsOutput> GetMyEntitlements()
    {
        string uid = _userManager.UserId;

        // 会员：优先读取 bubble_user 的扩展字段（BaseUserId 1:1）
        var profile = await _db.Queryable<BubbleUserEntity>()
            .Where(x => x.BaseUserId == uid)
            .FirstAsync();

        bool isVip = false;
        if (profile != null)
        {
            if (profile.IsVip == true)
            {
                // 有到期时间则判断
                if (profile.VipExpiredAt == null || profile.VipExpiredAt.Value > DateTime.Now)
                    isVip = true;
            }
        }

        // 主题包：目前先返回空（建议后续接 bubble_order/bubble_payment_record 做权益落库）
        await Task.CompletedTask;
        return new EntitlementsOutput { isVip = isVip, themes = new List<string>() };
    }
}

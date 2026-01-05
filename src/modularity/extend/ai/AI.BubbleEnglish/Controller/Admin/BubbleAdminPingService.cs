namespace AI.BubbleEnglish;

/// <summary>
/// Bubble 后台接口存活探针
/// </summary>
[ApiDescriptionSettings(ModuleConst.BubbleEnglish, Tag = "BubbleAdmin", Name = "Ping", Order = 2000)]
[Route("api/bubble/admin/[controller]")]
public class BubbleAdminPingService : IDynamicApiController, ITransient
{
    [AllowAnonymous]
    [HttpGet("ping")]
    public object Ping() => new { ok = true, ts = DateTime.UtcNow };
}

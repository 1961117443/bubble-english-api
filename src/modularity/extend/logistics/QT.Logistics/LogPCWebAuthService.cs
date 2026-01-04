using Microsoft.AspNetCore.Http;
using QT.Logistics.Entitys;
using QT.Logistics.Manager;

namespace QT.Logistics;

/// <summary>
/// 业务实现：pc端前台 需要登录认证.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "PC端前台服务", Name = "pcweb", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogPCWebAuthService: IDynamicApiController
{
    private ISqlSugarRepository<LogArticleEntity> _repository;
    private readonly ICacheManager _cacheManager;
    private readonly IWebUserManager _webUserManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 初始化一个<see cref="LogArticleService"/>类型的新实例.
    /// </summary>
    public LogPCWebAuthService(
        ISqlSugarRepository<LogArticleEntity> LogArticleRepository,
        ICacheManager cacheManager,
        IWebUserManager webUserManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = LogArticleRepository;
        _cacheManager = cacheManager;
        _webUserManager = webUserManager;
        _httpContextAccessor = httpContextAccessor;
    }


    /// <summary>
    /// 获取当前登录用户的信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("CurrentUser")]
    public dynamic GetCurrentUser()
    {
        var userInfo = new
        {
            userId = _webUserManager.UserId,
            realName = _webUserManager.RealName,
            account = _webUserManager.Account,
            role = _webUserManager.Role.ToString(),
        };
        return new
        {
            userInfo
        };
    }

    /// <summary>
    /// 退出.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Logout")]
    public void Logout()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext.SignoutToSwagger();
    }
}

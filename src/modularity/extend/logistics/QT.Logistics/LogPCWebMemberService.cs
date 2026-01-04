using Microsoft.AspNetCore.Http;
using QT.Common.Const;
using QT.Common.Extension;
using QT.Common.Security;
using QT.Logistics.Entitys;
using QT.Logistics.Entitys.Dto.LogOrder;
using QT.Logistics.Entitys.Dto.LogPCWeb;
using QT.Logistics.Interfaces;
using QT.Logistics.Manager;
using QT.Systems.Entitys.Dto.UsersCurrent;

namespace QT.Logistics;

/// <summary>
/// 业务实现：pc端前台 会员角色服务 需要登录认证.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "PC端前台服务", Name = "Member", Order = 200)]
[Route("api/Logistics/pcweb/[controller]")]
public class LogPCWebMemberService: IDynamicApiController
{
    private ISqlSugarRepository<LogArticleEntity> _repository;
    private readonly ICacheManager _cacheManager;
    private readonly IWebUserManager _webUserManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogOrderService _logOrderService;

    /// <summary>
    /// 初始化一个<see cref="LogArticleService"/>类型的新实例.
    /// </summary>
    public LogPCWebMemberService(
        ISqlSugarRepository<LogArticleEntity> LogArticleRepository,
        ICacheManager cacheManager,
        IWebUserManager webUserManager,
        IHttpContextAccessor httpContextAccessor,
        ILogOrderService logOrderService)
    {
        _repository = LogArticleRepository;
        _cacheManager = cacheManager;
        _webUserManager = webUserManager;
        _httpContextAccessor = httpContextAccessor;
        _logOrderService = logOrderService;
    }

    /// <summary>
    /// 获取用户信息.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/CurrentUser")]
    public async Task<LogMemberWebInfoOutput> ActionsCurrentUser()
    {
        var user = await _repository.Context.Queryable<LogMemberEntity>().InSingleAsync(_webUserManager.UserId) ?? throw Oops.Oh("账号不存在！");

        return user.Adapt<LogMemberWebInfoOutput>();
    }

    /// <summary>
    /// 更新用户信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogMemberWebUpInput input)
    {
        var entity = input.Adapt<LogMemberEntity>();
        var isOk = await _repository.Context
            .Updateable(entity)
            .UpdateColumns(it=> new {it.Name,it.Email,it.Gender,it.Address,it.BirthDate})
            .IgnoreColumns(ignoreAllNullColumns: true)
            .ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 修改密码.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/ModifyPassword")]
    public async Task ModifyPassword([FromBody] UsersCurrentActionsModifyPasswordInput input)
    {
        #region 校验验证码
        string? cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, input.timestamp);
        string? imageCode = await _cacheManager.GetAsync<string>(cacheKey);

        if (!input.code.ToLower().Equals(imageCode.ToLower()))
        {
            throw Oops.Oh(ErrorCode.D5015);
        }
        else
        {
            await _cacheManager.DelAsync(cacheKey);
        }
        #endregion

        var user = await _repository.Context.Queryable<LogMemberEntity>().InSingleAsync(_webUserManager.UserId) ?? throw Oops.Oh("账号不存在！");
        if (input.oldPassword != user.Password.ToLower())
            throw Oops.Oh(ErrorCode.D5007);

        user.Password = input.password;
        user.LastModifyTime = DateTime.Now;
        user.LastModifyUserId = _webUserManager.UserId;
        int isOk = await _repository.Context.Updateable<LogMemberEntity>(user).UpdateColumns(it => new
        {
            it.Password,
            it.LastModifyUserId,
            it.LastModifyTime
        }).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.D5008);
    }

    /// <summary>
    /// 获取订单管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Actions/Order/List")]
    public async Task<dynamic> GetList([FromQuery] LogOrderListQueryInput input)
    {
        input.shipperPhone = _webUserManager.Account;
        return await _logOrderService.GetList(input);
    }

}

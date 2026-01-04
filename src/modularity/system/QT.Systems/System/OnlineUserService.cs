using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Models.User;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Message.Entitys.Dto.IM;
using QT.Message.Interfaces;
using QT.Systems.Entitys.Dto.OnlineUser;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace QT.Systems;

/// <summary>
///  业务实现：在线用户.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "OnlineUser", Order = 176)]
[Route("api/system/[controller]")]
public class OnlineUserService : IDynamicApiController, ITransient
{
    /// <summary>
    /// IM回应服务.
    /// </summary>
    private readonly IImReplyService _imReplyService;

    /// <summary>
    /// 缓存管理器.
    /// </summary>
    private readonly ICacheManager _cacheManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="OnlineUserService"/>类型的新实例.
    /// </summary>
    public OnlineUserService(
        IImReplyService imReplyService,
        ICacheManager cacheManager,
        IUserManager userManager)
    {
        _imReplyService = imReplyService;
        _cacheManager = cacheManager;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取在线用户列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] KeywordInput input)
    {
        var tenantId = _userManager.TenantId;
        var userOnlineList = new List<UserOnlineModel>();
        var onlineKey = string.Format("{0}{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        if (_cacheManager.Exists(onlineKey))
        {
            userOnlineList = await GetOnlineUserList(tenantId);
            if (!input.keyword.IsNullOrEmpty())
                userOnlineList = userOnlineList.FindAll(x => x.userName.Contains(input.keyword));
        }

        return userOnlineList.Adapt<List<OnlineUserListOutput>>();
    }

    /// <summary>
    /// 强制下线.
    /// </summary>
    /// <param name="id"></param>
    [HttpDelete("{id}")]
    public async void ForcedOffline(string id)
    {
        var tenantId = _userManager.TenantId;
        var list = await GetOnlineUserList(tenantId);
        var user = list.Find(it => it.tenantId == tenantId && it.userId == id);
        if (user != null)
        {
            _imReplyService.ForcedOffline(user.connectionId);
            await DelOnlineUser(tenantId, user.userId);
            await DelUserInfo(tenantId, user.userId);
        }
    }

    /// <summary>
    /// 批量下线在线用户.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input">下线用户信息.</param>
    [HttpDelete("")]
    public async void Clear(string id, [FromBody] BatchOnlineInput input)
    {
        var tenantId = _userManager.TenantId;
        var list = await GetOnlineUserList(tenantId);
        var userList = list.FindAll(it => it.tenantId == tenantId && input.ids.Contains(it.userId));
        userList.ForEach(async item =>
        {
            _imReplyService.ForcedOffline(item.connectionId);
            await DelOnlineUser(tenantId, item.userId);
            await DelUserInfo(tenantId, item.userId);
        });

    }

    /// <summary>
    /// 获取在线用户列表.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <returns></returns>
    public async Task<List<UserOnlineModel>> GetOnlineUserList(string tenantId)
    {
        var cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        return await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);
    }

    /// <summary>
    /// 删除在线用户ID.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    public async Task<bool> DelOnlineUser(string tenantId, string userId)
    {
        var cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        var list = await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);
        var online = list.Find(it => it.userId == userId);
        list.RemoveAll((x) => x.connectionId == online.connectionId);
        return await _cacheManager.SetAsync(cacheKey, list);
    }

    /// <summary>
    /// 删除用户登录信息缓存.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="userId">用户ID.</param>
    /// <returns></returns>
    public async Task<bool> DelUserInfo(string tenantId, string userId)
    {
        var cacheKey = string.Format("{0}{1}_{2}", CommonConst.CACHEKEYUSER, tenantId, userId);
        return await _cacheManager.DelAsync(cacheKey);
    }
}
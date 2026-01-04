using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Weixin;
using QT.Common.Const;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.RemoteRequest.Extensions;
using SqlSugar;
using System.Linq.Expressions;

namespace QT.CMS;

/// <summary>
/// 微信公众号菜单
/// </summary>
//[Route("admin/weixin/menu")]
//[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "menu", Name = "menu", Order = 200)]
[Route("api/cms/admin/weixin/[controller]")]
public class WeixinMenuController : IDynamicApiController
{
    //private readonly IWxMenuService _menuService;
    private readonly ISqlSugarRepository<WxAccount> _repository;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public WeixinMenuController(ISqlSugarRepository<WxAccount> repository)
    {
        //_menuService = menuService;
        _repository = repository;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/weixin/menu/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.View)]
    public async Task<WxMenuCreate> Get([FromRoute] int id)
    {
        //获取AccessToken凭证
        var token = await this.GetAccessTokenAsync(x => x.Id == id);
        //发送请求得到结果
        var requestUrl = $"{WxConfing.ApiMpHost}/cgi-bin/get_current_selfmenu_info?access_token={token.AccessToken}";
        var resultBody = await requestUrl.GetAsStringAsync();
        var result = JSON.Deserialize<WxMenuResult>(resultBody);
        if (result == null || result.selfMenuInfo == null)
        {
            throw Oops.Oh($"获取菜单出错，返回消息：{resultBody}");
        }
        if (result.ErrCode != 0)
        {
            throw Oops.Oh($"获取菜单出错，错误码：{result.ErrCode}，错误消息：{result.ErrMsg}");
        }
        //转换成创建模型
        var menuCreate = new WxMenuCreate();
        foreach (var button in result.selfMenuInfo.button)
        {
            menuCreate.button.Add(new WxMenuCreateButton
            {
                type = button.type,
                name = button.name,
                key = button.key,
                url = button.url,
                appId = button.appId,
                pagePath = button.pagePath,
                mediaId = button.mediaId,
                articleId = button.articleId,
                subButton = button!.subButton!.list
            });
        }
        return menuCreate;
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/weixin/menu/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.Edit)]
    public async Task Update([FromRoute] int id, [FromBody] WxMenuCreate modelDto)
    {
        //获取AccessToken凭证
        var token = await this.GetAccessTokenAsync(x => x.Id == id);
        //发送请求得到结果
        var requestUrl = $"{WxConfing.ApiMpHost}/cgi-bin/menu/create?access_token={token.AccessToken}";
        var resultBody = await requestUrl.SetBody(modelDto).PostAsStringAsync();
        var result = JSON.Deserialize<WxMenuResult>(resultBody);
        if (result == null)
        {
            throw Oops.Oh($"更新菜单出错，返回消息：{resultBody}");
        }
        if (result.ErrCode != 0)
        {
            throw Oops.Oh($"更新菜单出错，错误码：{result.ErrCode}，错误消息：{result.ErrMsg}");
        }
    }

    /// <summary>
    /// 删除记录
    /// 示例：/admin/weixin/menu/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    //[AuthorizeFilter("WeixinAccount", ActionType.Delete)]
    public async Task Delete([FromRoute] int id)
    {
        //获取AccessToken凭证
        var token = await this.GetAccessTokenAsync(x => x.Id == id);
        //发送请求得到结果
        var requestUrl = $"{WxConfing.ApiMpHost}/cgi-bin/menu/delete?access_token={token.AccessToken}";
        var resultBody = await requestUrl.GetAsStringAsync();
        var result = JSON.Deserialize<WxJsonResult>(resultBody);
        if (result == null)
        {
            throw Oops.Oh($"删除菜单出错，返回消息：{resultBody}");
        }
        if (result.ErrCode != 0)
        {
            throw Oops.Oh($"删除菜单出错，错误码：{result.ErrCode}，错误消息：{result.ErrMsg}");
        }
    }
    #endregion

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    private async Task<WxAccessTokenResult> GetAccessTokenAsync(Expression<Func<WxAccount, bool>> funcWhere)
    {
        //获取公众号账户信息
        var accountModel = await _repository.SingleAsync(funcWhere);
        if (accountModel == null)
        {
            throw Oops.Oh("公众号账户不存在，请检查后重试。");
        }
        //发送请求得到结果
        var requestUrl = $"{WxConfing.ApiMpHost}/cgi-bin/token?grant_type=client_credential&appid={accountModel.AppId}&secret={accountModel.AppSecret}";
        var resultBody = await requestUrl.GetAsStringAsync();
        var result = JSON.Deserialize<WxAccessTokenResult>(resultBody);
        if (result == null)
        {
            throw Oops.Oh($"获取AccessToken出错，返回消息：{resultBody}");
        }
        if (result.ErrCode != 0)
        {
            throw Oops.Oh($"获取AccessToken出错，错误码：{result.ErrCode}，错误消息：{result.ErrMsg}");
        }
        return result;
    }
}

using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using QT.Common.Core.Dto.IWechatAppProxy;
using QT.Common.Extension;
using QT.FriendlyException;
using QT.RemoteRequest;
using QT.RemoteRequest.Extensions;

namespace QT.Common.Core;

/// <summary>
/// 微信小程序请求代理
/// </summary>
public interface IWechatAppProxy: IHttpDispatchProxy
{
    /// <summary>
    /// 获取不限制的二维码
    /// </summary>
    /// <returns></returns>
    [Post("https://api.weixin.qq.com/wxa/getwxacodeunlimit")]
    public Task<Stream> GetWxacodeUnlimit([Body] WxacodeUnlimitRequest input, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);


    /// <summary>
    /// 获取手机号
    /// </summary>
    /// <returns></returns>
    [Post("https://api.weixin.qq.com/wxa/business/getuserphonenumber")]
    public Task<WxGetUserPhonenumberResponse> GetUserPhonenumber([Body] WxGetUserPhonenumberRequest input, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);

    // 全局拦截，类中每一个方法都会触发
    [Interceptor(InterceptorTypes.Request)]
    static void OnRequesting(HttpRequestMessage req)
    {
        var cache = App.GetService<IMemoryCache>();
        var appId = App.Configuration["Wechat:miniprogram:AppID"];
        var appSecret = App.Configuration["Wechat:miniprogram:AppSecret"];
        //var envVersion = App.Configuration["Wechat:miniprogram:env_version"];
        var access_token = cache.GetOrCreate($"IWechatAppProxy:{appId}:ACCESS_TOKEN", entry =>
        {
            var getAccessTokenStr = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={appSecret}".GetAsStringAsync().GetAwaiter().GetResult();
            if (getAccessTokenStr.IsNotEmptyOrNull())
            {
                var getAccessToken = JObject.Parse(getAccessTokenStr);
                if (getAccessToken["access_token"].IsNotEmptyOrNull())
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(7000);
                    return getAccessToken["access_token"]!.ToString();
                }
            }
            throw Oops.Oh("获取微信小程序接口调用凭据失败！");
        });
        // 追加更多参数
        req.AppendQueries(new Dictionary<string, object> {
            { "access_token", access_token}
        });
    }

}

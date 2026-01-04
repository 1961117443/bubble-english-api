using QT.CMS.Entitys;

namespace QT.CMS.Interfaces;

/// <summary>
/// 支付接口
/// </summary>
public interface IWeChatExecuteService
{
    /// <summary>
    /// APP下单支付
    /// </summary>
    Task<WeChatPayAppParamDto> AppPayAsync(WeChatPayDto modelDto);

    /// <summary>
    /// JSAPI下单支付
    /// </summary>
    Task<WeChatPayJsApiParamDto> JsApiPayAsync(WeChatPayDto modelDto, bool mp = true);

    /// <summary>
    /// H5下单支付
    /// </summary>
    Task<WeChatPayH5ParamDto> H5PayAsync(WeChatPayDto modelDto);

    /// <summary>
    /// 扫码下单支付
    /// </summary>
    Task<WeChatPayNativeParamDto> NativePayAsync(WeChatPayDto modelDto);

    /// <summary>
    /// 拼接OAuth2的URI
    /// </summary>
    Task<string> OAuthAsync(WeChatPayOAuthDto modelDto);

    /// <summary>
    /// 申请退款
    /// </summary>
    Task<bool> RefundAsync(WeChatPayRefundDto modelDto);
}

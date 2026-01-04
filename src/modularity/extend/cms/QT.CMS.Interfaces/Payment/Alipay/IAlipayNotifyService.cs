using Microsoft.AspNetCore.Http;
using QT.CMS.Entitys;

namespace QT.CMS.Interfaces;

public interface IAlipayNotifyService
{
    /// <summary>
    /// APP支付回调通知
    /// </summary>
    Task<AlipayAppNotifyDto> AppPay(HttpRequest request);

    /// <summary>
    /// 电脑支付回调通知
    /// </summary>
    Task<AlipayPageNotifyDto> PagePay(HttpRequest request);

    /// <summary>
    /// 手机支付回调通知
    /// </summary>
    Task<AlipayWapNotifyDto> WapPay(HttpRequest request);
}

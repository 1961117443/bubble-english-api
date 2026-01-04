using Microsoft.AspNetCore.Http;
using QT.CMS.Entitys;

namespace QT.CMS.Interfaces;

public interface IWeChatNotifyService
{
    /// <summary>
    /// 确认解密回调信息
    /// </summary>
    Task<WeChatPayNotifyDecryptDto?> ConfirmAsync(HttpRequest request, int paymentId);
}

using QT.CMS.Entitys;

namespace QT.CMS.Interfaces;

/// <summary>
/// 支付宝支付接口
/// </summary>
public interface IAlipayExecuteService
{
    /// <summary>
    /// APP下单支付
    /// </summary>
    Task<string> AppPayAsync(AlipayTradeDto modelDto);

    /// <summary>
    /// 电脑网站下单支付
    /// </summary>
    Task<AlipayPageParamDto> PcPayAsync(AlipayTradeDto modelDto);

    /// <summary>
    /// 手机网站下单支付
    /// </summary>
    Task<AlipayPageParamDto> H5PayAsync(AlipayTradeDto modelDto);

    /// <summary>
    /// 处理退款请求
    /// </summary>
    Task<bool> RefundAsync(AlipayRefundDto modelDto);
}

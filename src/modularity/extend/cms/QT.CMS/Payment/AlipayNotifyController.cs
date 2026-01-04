namespace QT.CMS;

/// <summary>
/// 支付宝支付回调
/// </summary>
[Route("api/cms/alipay/notify")]
[ApiController]
public class AlipayNotifyController : ControllerBase
{
    private readonly IAlipayNotifyService _alipayNotifyService;
    private readonly IPaymentCollectionService _paymentCollectionService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public AlipayNotifyController(IAlipayNotifyService alipayNotifyService, IPaymentCollectionService paymentCollectionService)
    {
        _alipayNotifyService = alipayNotifyService;
        _paymentCollectionService = paymentCollectionService;
    }

    /// <summary>
    /// 支付成功回调
    /// 示例：/alipay/notify/transactions/app
    /// </summary>
    [HttpPost("transactions/{type}")]
    public async Task<IActionResult> Transactions([FromRoute] string type)
    {
        bool result = false;
        if (type.ToLower() == "app")
        {
            var model = await _alipayNotifyService.AppPay(Request);
            if (model != null && model.OutTradeNo != null)
            {
                //修改订单状态
                result = await _paymentCollectionService.ConfirmAsync(model.OutTradeNo);
            }
        }
        if (type.ToLower() == "pc")
        {
            var model = await _alipayNotifyService.PagePay(Request);
            if (model != null && model.OutTradeNo != null)
            {
                //修改订单状态
                result = await _paymentCollectionService.ConfirmAsync(model.OutTradeNo);
            }
        }
        if (type.ToLower() == "h5")
        {
            var model = await _alipayNotifyService.WapPay(Request);
            if (model != null && model.OutTradeNo != null)
            {
                //修改订单状态
                result = await _paymentCollectionService.ConfirmAsync(model.OutTradeNo);
            }
        }
        //返回结果
        if (!result)
        {
            return BadRequest("fail");
        }
        return Ok("success");
    }
}
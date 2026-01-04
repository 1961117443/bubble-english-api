namespace QT.CMS;

/// <summary>
/// 微信支付回调
/// </summary>
[Route("api/cms/wechatpay/notify")]
[ApiController]
public class WeChatPayNotifyController : ControllerBase
{
    private readonly IWeChatNotifyService _weChatNotifyService;
    private readonly IPaymentCollectionService _paymentCollectionService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public WeChatPayNotifyController(IWeChatNotifyService weChatNotifyService, IPaymentCollectionService paymentCollectionService)
    {
        _weChatNotifyService = weChatNotifyService;
        _paymentCollectionService = paymentCollectionService;
    }

    /// <summary>
    /// 支付成功回调
    /// 示例：/wechatpay/notify/transactions/1
    /// </summary>
    [HttpPost("transactions/{paymentId}")]
    public async Task<IActionResult> Transactions([FromRoute] int paymentId)
    {
        bool result = false; //处理状态
        var notify = await _weChatNotifyService.ConfirmAsync(Request, paymentId);
        if (notify != null && notify.TradeState == "SUCCESS" && notify.OutTradeNo != null)
        {
            //修改订单状态
            result = await _paymentCollectionService.ConfirmAsync(notify.OutTradeNo);
        }
        //返回结果
        if (result)
        {
            return Ok(new WeChatPayNotifyResultDto());
        }
        return BadRequest(new WeChatPayNotifyResultDto()
        {
            Code = "FAIL",
            Message = "FAIL"
        });
    }
}